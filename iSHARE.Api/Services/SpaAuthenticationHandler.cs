﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using iSHARE.Identity.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
namespace iSHARE.Api.Services
{
    public class SpaAuthenticationHandler : AuthenticationHandler<JwtBearerOptions>
    {
        private OpenIdConnectConfiguration _configuration;
        private readonly IUserHandler _userHandler;
        private readonly IIdentityProviderSignatureValidator _idpSignatureValidator;
        public SpaAuthenticationHandler(IOptionsMonitor<JwtBearerOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserHandler userHandler,
            IIdentityProviderSignatureValidator idpValidator)
            : base(options, logger, encoder, clock)
        {
            _userHandler = userHandler;
            _idpSignatureValidator = idpValidator;
        }
        protected new JwtBearerEvents Events
        {
            get => (JwtBearerEvents)base.Events;
            set => base.Events = value;
        }
        protected override Task<object> CreateEventsAsync() => Task.FromResult<object>(new JwtBearerEvents());
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // Give application opportunity to find from a different location, adjust, or reject token
                var messageReceivedContext = new MessageReceivedContext(Context, Scheme, Options);
                // event can set the token
                await Events.MessageReceived(messageReceivedContext);
                if (messageReceivedContext.Result != null)
                {
                    return messageReceivedContext.Result;
                }
                // If application retrieved token from somewhere else, use that.
                var token = messageReceivedContext.Token;
                if (string.IsNullOrEmpty(token))
                {
                    string authorization = Request.Headers["Authorization"];
                    // If no authorization header found, nothing to process further
                    if (string.IsNullOrEmpty(authorization))
                    {
                        return AuthenticateResult.NoResult();
                    }
                    if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        token = authorization.Substring("Bearer ".Length).Trim();
                    }
                    // If no token found, no further work possible
                    if (string.IsNullOrEmpty(token))
                    {
                        return AuthenticateResult.NoResult();
                    }
                }
                if (_configuration == null && Options.ConfigurationManager != null)
                {
                    _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
                }
                var validationParameters = Options.TokenValidationParameters.Clone();
                if (_configuration != null)
                {
                    var issuers = new[] { _configuration.Issuer };
                    validationParameters.ValidIssuers = validationParameters.ValidIssuers?.Concat(issuers) ?? issuers;
                    validationParameters.IssuerSigningKeys = validationParameters.IssuerSigningKeys?.Concat(_configuration.SigningKeys)
                        ?? _configuration.SigningKeys;
                }
                List<Exception> validationFailures = null;
                foreach (var validator in Options.SecurityTokenValidators)
                {
                    if (!validator.CanReadToken(token))
                    {
                        continue;
                    }

                    ClaimsPrincipal principal;
                    SecurityToken validatedToken;
                    try
                    {
                        principal = validator.ValidateToken(token, validationParameters, out validatedToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogInformation("Failed to validate the token.");
                        // Refresh the configuration for exceptions that may be caused by key rollovers. The user can also request a refresh in the event.
                        if (Options.RefreshOnIssuerKeyNotFound && Options.ConfigurationManager != null
                                                               && ex is SecurityTokenSignatureKeyNotFoundException)
                        {
                            Options.ConfigurationManager.RequestRefresh();
                        }
                        if (validationFailures == null)
                        {
                            validationFailures = new List<Exception>(1);
                        }
                        validationFailures.Add(ex);
                        continue;
                    }

                    var isSigned = await _idpSignatureValidator.IsSigned(token);
                    if (!isSigned)
                    {
                        Logger.LogInformation("Failed to validate the token signature.");
                        return AuthenticateResult.Fail("Invalid signature");
                    }

                    await _userHandler.Handle(principal);

                    Logger.LogInformation("Successfully validated the token.");
                    var tokenValidatedContext = new TokenValidatedContext(Context, Scheme, Options)
                    {
                        Principal = principal,
                        SecurityToken = validatedToken
                    };
                    await Events.TokenValidated(tokenValidatedContext);
                    if (tokenValidatedContext.Result != null)
                    {
                        return tokenValidatedContext.Result;
                    }
                    if (Options.SaveToken)
                    {
                        tokenValidatedContext.Properties.StoreTokens(new[]
                        {
                            new AuthenticationToken { Name = "access_token", Value = token }
                        });
                    }

                    tokenValidatedContext.Success();
                    return tokenValidatedContext.Result;
                }

                if (validationFailures == null)
                {
                    return AuthenticateResult.Fail(
                        "No SecurityTokenValidator available for token: " + token ?? "[null]");
                }

                var authenticationFailedContext = new AuthenticationFailedContext(Context, Scheme, Options)
                {
                    Exception = (validationFailures.Count == 1) ? validationFailures[0] : new AggregateException(validationFailures)
                };
                await Events.AuthenticationFailed(authenticationFailedContext);
                if (authenticationFailedContext.Result != null)
                {
                    return authenticationFailedContext.Result;
                }
                return AuthenticateResult.Fail(authenticationFailedContext.Exception);
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception occurred while processing message.");
                var authenticationFailedContext = new AuthenticationFailedContext(Context, Scheme, Options)
                {
                    Exception = ex
                };
                await Events.AuthenticationFailed(authenticationFailedContext);
                if (authenticationFailedContext.Result != null)
                {
                    return authenticationFailedContext.Result;
                }
                throw;
            }


        }
    }
}
