using Api.Modules.Identity.Interfaces;

namespace Api.Modules.Identity.Models
{
    public class PasswordModel : IPassword
    {
        public string Password { get; set; }

        public PasswordModel()
        {
            Password = "";
        }
    }
}