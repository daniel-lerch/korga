using Mailist.ChurchTools;
using Mailist.ChurchTools.Hosting;
using Mailist.EmailRelay;
using Mailist.Extensions;
using Mailist.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mailist.EmailDelivery;
using Mailist.Filters;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

namespace Mailist;

public class Startup
{
    private readonly IWebHostEnvironment environment;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        environment = env;
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMailistOptions(Configuration);

        services.AddChurchToolsApi();

        services.AddMailistMySqlDatabase();

        services.AddTransient<PersonFilterService>();

        services.AddControllers();

        services.AddOpenApiDocument();

        services.AddHealthChecks();

        if (!environment.IsDevelopment())
        {
            services.AddDataProtection().PersistKeysToFileSystem(new(Path.Combine(environment.ContentRootPath, "secrets")));
        }

        services.AddOAuthAuthentication(Configuration, environment);

        // Use Configuration manually because options are not available in ConfigureService
        // Instead of returning a fake service when disabled we don't register any hosted service at all
        if (Configuration.GetValue<bool>("ChurchTools:EnableSync"))
        {
            services.AddTransient<ChurchToolsSyncService>();
            services.AddHostedService<ChurchToolsSyncHostedService>();
            services.AddHostedService<ChurchToolsPermissionsHostedService>();
        }

        if (Configuration.GetValue<bool>("EmailDelivery:Enable"))
        {
            services.AddSingleton<JobQueue<EmailDeliveryJobController>>();
            services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<JobQueue<EmailDeliveryJobController>>());
            services.AddScoped<EmailDeliveryService>();

            if (Configuration.GetValue<bool>("EmailRelay:Enable"))
            {
                services.AddSingleton<JobQueue<EmailRelayJobController>>();
                services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<JobQueue<EmailRelayJobController>>());
                services.AddScoped<ImapReceiverService>();
                services.AddScoped<DistributionListService>();
                services.AddScoped<MimeMessageCreationService>();
                services.AddHostedService<EmailRelayHostedService>();
            }
        }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHosting();

        app.UseRouting();

        app.UseMailistCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/healthz").RequireHost("localhost:*");
        });

        app.UseOpenApi();
        app.UseSwaggerUi();
    }
}
