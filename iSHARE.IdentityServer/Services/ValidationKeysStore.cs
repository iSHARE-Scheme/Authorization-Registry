﻿using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using iSHARE.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace iSHARE.IdentityServer.Services
{
    public class ValidationKeysStore : IValidationKeysStore
    {
        private readonly IDigitalSigner _digitalSigner;

        public ValidationKeysStore(IDigitalSigner digitalSigner)
        {
            _digitalSigner = digitalSigner;
        }

        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            var publicKey = await _digitalSigner.GetPublicKey();
            var certificate = publicKey.ConvertToX509Certificate2();
            var securityKey = new X509SecurityKey(certificate);

            return new[]
            {
                new SecurityKeyInfo
                {
                    Key = securityKey,
                    SigningAlgorithm = SecurityAlgorithms.RsaSha256
                }
            };
        }
    }
}
