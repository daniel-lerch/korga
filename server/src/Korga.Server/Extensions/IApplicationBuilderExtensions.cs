using Korga.Server.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Korga.Server.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMigrations(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            using (DatabaseContext context = scope.ServiceProvider.GetRequiredService<DatabaseContext>())
            {
                context.Database.EnsureCreated();
            }

            return app;
        }
    }
}
