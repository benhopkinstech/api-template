using BC = BCrypt.Net.BCrypt;

namespace Api.Modules.Identity.Classes
{
    public class Encryption
    {
        public static string GenerateHash(string password)
        {
            int workFactor = 12;

            return BC.EnhancedHashPassword(password, workFactor);
        }

        public static bool VerifyHash(string password, string hash)
        {
            return BC.EnhancedVerify(password, hash);
        }
    }
}
