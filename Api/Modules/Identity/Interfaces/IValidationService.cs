namespace Api.Modules.Identity.Interfaces
{
    public interface IValidationService
    {
        string[] EmailCheck(string email);
        string[] PasswordCheck(string password);
    }
}
