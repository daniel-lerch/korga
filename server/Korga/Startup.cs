using Korga.ChurchTools;
using Korga.ChurchTools.Hosting;
using Korga.EmailRelay;
using Korga.Extensions;
using Korga.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Korga.EmailDelivery;
using Korga.Filters;

namespace Korga;

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
        services.AddKorgaOptions(Configuration);

        services.AddChurchToolsApi();

        services.AddKorgaMySqlDatabase();

        services.AddTransient<PersonFilterService>();

        services.AddSpaStaticFiles(options => options.RootPath = environment.WebRootPath);

        services.AddControllers();

        services.AddOpenApiDocument();

        services.AddOpenIdConnectAuthentication(Configuration, environment);

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

        app.UseSpaStaticFiles();

        app.UseRouting();

        app.UseKorgaCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseOpenApi();
        app.UseSwaggerUi();

        app.UseSpa(spa => spa.UseVueSpaFileProvider());
    }
}
