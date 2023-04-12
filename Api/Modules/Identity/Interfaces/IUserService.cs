namespace Api.Modules.Identity.Interfaces
{
    public interface IUserService
    {
        public string GenerateToken(Guid accountId, string email);
        public Guid? GetAccountId();
    }
}
