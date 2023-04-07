using Api.Modules.Identity.Interfaces;

namespace Api.Modules.Identity.Models
{
    public class CredentialsModel : ICredentials
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public CredentialsModel()
        {
            Email = "";
            Password = "";
        }
    }
}