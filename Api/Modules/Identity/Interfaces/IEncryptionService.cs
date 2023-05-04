namespace Api.Modules.Identity.Interfaces
{
    public interface IEncryptionService
    {
        string GenerateHash(string password);
        bool VerifyHash(string password, string hash);
    }
}
