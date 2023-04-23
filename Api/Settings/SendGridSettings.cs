namespace Api.Settings
{
    public class SendGridSettings
    {
        public string ApiKey { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string VerificationLinkTemplateId { get; set; }
        public string EmailChangedTemplateId { get; set; }
        public string ResetLinkTemplateId { get; set; }
        public string ResetUrl { get; set; }

        public SendGridSettings()
        {
            ApiKey = string.Empty;
            Email = string.Empty;
            Name = string.Empty;
            VerificationLinkTemplateId = string.Empty;
            EmailChangedTemplateId = string.Empty;
            ResetLinkTemplateId = string.Empty;
            ResetUrl = string.Empty;
        }
    }
}
