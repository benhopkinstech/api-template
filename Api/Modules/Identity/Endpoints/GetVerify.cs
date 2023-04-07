using Api.Modules.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Api.Modules.Identity.Endpoints
{
    public static class GetVerify
    {
        public static async Task<IResult> VerifyAsync(string code, IdentityContext identity)
        {
            if (!Convert.TryFromBase64String(code, new byte[code.Length], out _))
                return Results.NotFound();

            var decodedItems = Encoding.Unicode.GetString(Convert.FromBase64String(code)).Split('&');
            if (decodedItems.Length != 3)
                return Results.NotFound();

            if (!Guid.TryParse(decodedItems[0], out var verificationId) || !Guid.TryParse(decodedItems[1], out var accountId) || !DateTime.TryParse(decodedItems[2], out var verificationCreated))
                return Results.NotFound();

            if (DateTime.UtcNow > verificationCreated.AddDays(3))
                return Results.BadRequest("Verification link expired");

            var accountVerification = await identity.Verification.Where(x => x.Id == verificationId).FirstOrDefaultAsync();
            if (accountVerification == null)
                return Results.NotFound();

            var difference = accountVerification.CreatedOn - verificationCreated;
            if (accountVerification.Id != verificationId || accountVerification.AccountId != accountId || difference > TimeSpan.FromSeconds(3))
                return Results.NotFound();

            var account = await identity.Account.Where(x => x.Id == accountId).Include(x => x.Verification).FirstOrDefaultAsync();
            if (account == null)
                return Results.NotFound();

            if (account.Verified)
            {
                identity.Verification.RemoveRange(account.Verification);
                await identity.SaveChangesAsync();
                return Results.Ok("Account already verified");
            }

            account.Verified = true;
            account.VerifiedOn = DateTime.UtcNow;
            account.UpdatedOn = DateTime.UtcNow;
            identity.Verification.RemoveRange(account.Verification);
            await identity.SaveChangesAsync();
            return Results.Redirect("http://www.google.com?success=1");
        }
    }
}
