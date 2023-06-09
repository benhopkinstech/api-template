﻿namespace Api.Options
{
    public class IdentityOptions
    {
        public bool VerificationRequired { get; set; }
        public double VerificationExpiryHours { get; set; }
        public string VerificationRedirectUrlSuccess { get; set; }
        public string VerificationRedirectUrlFail { get; set; }
        public double ResetExpiryHours { get; set; }

        public IdentityOptions() 
        {
            VerificationRequired = false;
            VerificationExpiryHours = 24;
            VerificationRedirectUrlSuccess = string.Empty;
            VerificationRedirectUrlFail = string.Empty;
            ResetExpiryHours = 1;
        }
    }
}
