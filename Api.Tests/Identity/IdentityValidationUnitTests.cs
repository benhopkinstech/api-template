using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Services;
using Xunit;

namespace Api.Tests.Identity
{
    public class IdentityValidationUnitTests
    {
        private readonly IValidationService _validation;

        public IdentityValidationUnitTests()
        {
            _validation = new ValidationService();
        }

        [Fact]
        public void SevenCharacterPassword_HasErrors()
        {
            const string password = "aaaaaaa";

            var errors = _validation.PasswordCheck(password);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public void SeventyThreeCharacterPassword_HasErrors()
        {
            const string password = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

            var errors = _validation.PasswordCheck(password);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public void TwoHundredFiftySevenCharcterEmail_HasErrors()
        {
            const string email = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa@email.com";

            var errors = _validation.EmailCheck(email);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public void WithoutAtEmail_HasErrors()
        {
            const string email = "testemail.com";

            var errors = _validation.EmailCheck(email);

            Assert.NotEmpty(errors);
        }
    }
}
