using Api.Modules.Identity.Interfaces;

namespace Api.Modules.Identity.Models
{
    public class EmailModel : IEmail
    {
        public string Email { get; set; }

        public EmailModel()
        {
            Email = "";
        }
    }
}