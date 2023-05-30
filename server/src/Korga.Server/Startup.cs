using Korga.Server.ChurchTools;
using Korga.Server.ChurchTools.Hosting;
using Korga.Server.EmailDelivery;
using Korga.Server.EmailRelay;
using Korga.Server.Extensions;
using Korga.Server.Services;
using Korga.Server.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Korga.Server;

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

        services.AddSingleton<LdapService>();

        services.AddChurchToolsApi();

        services.AddKorgaMySqlDatabase();

        services.AddSpaStaticFiles(options => options.RootPath = environment.WebRootPath);

        if (environment.IsDevelopment())
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin();
                    builder.AllowAnyMethod();
                    builder.AllowAnyHeader();
                });
            });
        }

        services.AddControllers();

        services.AddOpenApiDocument();

        // Use Configuration manually because options are not available in ConfigureService
        // Instead of returning a fake service when disabled we don't register any hosted service at all
        if (Configuration.GetValue<bool>("ChurchTools:EnableSync"))
        {
            services.AddTransient<ChurchToolsSyncService>();
            services.AddHostedService<ChurchToolsSyncHostedService>();
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

        if (env.IsDevelopment())
        {
            app.UseCors();
        }

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseOpenApi();
        app.UseSwaggerUi3();

        app.UseSpa(spa => spa.UseVueSpaFileProvider());
    }
}
