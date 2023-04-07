using System.Net.Mail;

namespace Api.Modules.Identity.Classes
{
    public class Validation
    {
        public static List<string> EmailCheck(string email)
        {
            var errors = new List<string>();

            if (String.IsNullOrWhiteSpace(email))
                errors.Add("Email address must be provided");

            if (email.Length > 256)
                errors.Add("Email address maximum length is 256 characters");

            if (!MailAddress.TryCreate(email, out _))
                errors.Add("Email address format invalid");

            return errors;
        }

        public static List<string> PasswordCheck(string password)
        {
            var errors = new List<string>();

            if (String.IsNullOrWhiteSpace(password))
                errors.Add("Password must be provided");

            if (password.Length < 8)
                errors.Add("Password minimum length is 8 characters");

            if (password.Length > 72)
                errors.Add("Password maximum length is 72 characters");

            return errors;
        }
    }
}
