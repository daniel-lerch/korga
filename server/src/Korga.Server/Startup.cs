using Korga.Server.Extensions;
using Korga.Server.Services;
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

        services.AddHostedService<EmailRelayHostedService>();
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
