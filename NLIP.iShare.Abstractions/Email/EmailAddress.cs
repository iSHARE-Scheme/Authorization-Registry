﻿namespace NLIP.iShare.Abstractions.Email
{
    public class EmailAddress
    {
        public string Address { get; set; }
        public string DisplayName { get; set; }

        public EmailAddress(string address, string displayName)
        {
            address.NotNullOrEmpty(nameof(address));
            Address = address;
            DisplayName = displayName;
        }
        public EmailAddress() { }
    }
}