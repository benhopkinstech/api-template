using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Services;
using Xunit;

namespace Api.Tests.Identity
{
    public class IdentityEncryptionUnitTests
    {
        private readonly IEncryptionService _encryption;

        public IdentityEncryptionUnitTests()
        {
            _encryption = new EncryptionService();
        }

        [Fact]
        public void SamePassword_GeneratesDifferentHashes()
        {
            const string password = "password";

            var firstHash = _encryption.GenerateHash(password);
            var secondHash = _encryption.GenerateHash(password);

            Assert.NotEqual(firstHash, secondHash);
        }

        [Fact]
        public void HashGenerated_Verifies()
        {
            const string password = "password";

            var hash = _encryption.GenerateHash(password);
            var verified = _encryption.VerifyHash(password, hash);

            Assert.True(verified);
        }

        [Fact]
        public void IncorrectPasssword_DoesntVerify()
        {
            const string password = "password";

            var hash = _encryption.GenerateHash(password);
            var verified = _encryption.VerifyHash("passw0rd", hash);

            Assert.False(verified);
        }
    }
}
