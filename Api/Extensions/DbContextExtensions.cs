using Api.Modules.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Extensions
{
    public static class DbContextExtensions
    {
        public static IServiceCollection AddDbContexts(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddDbContext<IdentityContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

            return services;
        }
    }
}
