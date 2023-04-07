using Api.Modules.Identity.Data.Tables;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Text;

namespace Api.Modules.Identity.Classes
{
    public class Email
    {
        public static async Task<bool> SendVerificationLinkAsync(IConfiguration config, HttpContext http, Verification verification, string recipient)
        {
            var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}/Identity/Verify?code=";
            var dynamicTemplateData = new
            {
                url = baseUrl + Convert.ToBase64String(Encoding.Unicode.GetBytes($"{verification.Id}&{verification.AccountId}&{verification.CreatedOn}")),
            };

            return await SendViaSendGridAsync(config, recipient, config.GetValue<string>("SendGrid:VerificationLinkTemplateId") ?? "", dynamicTemplateData);
        }

        public static async Task<bool> SendEmailChangedAsync(IConfiguration config, string recipient, string newEmail)
        {
            var dynamicTemplateData = new
            {
                newEmail,
            };

            return await SendViaSendGridAsync(config, recipient, config.GetValue<string>("SendGrid:EmailChangedTemplateId") ?? "", dynamicTemplateData);
        }

        public static async Task<bool> SendResetLinkAsync(IConfiguration config, Reset reset, string recipient)
        {
            var dynamicTemplateData = new
            {
                code = Convert.ToBase64String(Encoding.Unicode.GetBytes($"{reset.Id}&{reset.AccountId}&{reset.CreatedOn}")),
            };

            return await SendViaSendGridAsync(config, recipient, config.GetValue<string>("SendGrid:ResetLinkTemplateId") ?? "", dynamicTemplateData);
        }

        private static async Task<bool> SendViaSendGridAsync(IConfiguration config, string recipient, string templateId, object dynamicTemplateData)
        {
            var client = new SendGridClient(config.GetValue<string>("SendGrid:ApiKey"));
            var from = new EmailAddress(config.GetValue<string>("SendGrid:Email"), config.GetValue<string>("SendGrid:Name"));
            var to = new EmailAddress(recipient);
            SendGridMessage message = MailHelper.CreateSingleTemplateEmail(from, to, templateId, dynamicTemplateData);
            var response = await client.SendEmailAsync(message);
            if (!response.IsSuccessStatusCode)
                return false;

            return true;
        }
    }
}
