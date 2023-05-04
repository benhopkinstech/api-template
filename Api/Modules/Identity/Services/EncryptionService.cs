using Api.Modules.Identity.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace Api.Modules.Identity.Services
{
    public class EncryptionService : IEncryptionService
    {
        public string GenerateHash(string password)
        {
            int workFactor = 12;

            return BC.EnhancedHashPassword(password, workFactor);
        }

        public bool VerifyHash(string password, string hash)
        {
            return BC.EnhancedVerify(password, hash);
        }
    }
}
