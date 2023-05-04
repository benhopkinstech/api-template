using Api.Modules.Identity.Interfaces;
using System.Net.Mail;

namespace Api.Modules.Identity.Services
{
    public class ValidationService : IValidationService
    {
        public string[] EmailCheck(string email)
        {
            var errors = new List<string>();

            if (String.IsNullOrWhiteSpace(email))
                errors.Add("Must be provided");

            if (email.Length > 256)
                errors.Add("Maximum length is 256 characters");

            if (!MailAddress.TryCreate(email, out _))
                errors.Add("Invalid address");

            return errors.ToArray();
        }

        public string[] PasswordCheck(string password)
        {
            var errors = new List<string>();

            if (String.IsNullOrWhiteSpace(password))
                errors.Add("Must be provided");

            if (password.Length < 8)
                errors.Add("Minimum length is 8 characters");

            if (password.Length > 72)
                errors.Add("Maximum length is 72 characters");

            return errors.ToArray();
        }
    }
}
