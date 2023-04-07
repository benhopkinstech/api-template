using Api.Modules.Identity.Interfaces;

namespace Api.Modules.Identity.Models
{
    public class PasswordUpdateModel : IPassword
    {
        public string Password { get; set; }
        public string CurrentPassword { get; set; }

        public PasswordUpdateModel()
        {
            Password = "";
            CurrentPassword = "";
        }
    }
}