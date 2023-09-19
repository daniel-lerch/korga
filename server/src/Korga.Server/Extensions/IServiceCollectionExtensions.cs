using Korga.Server.ChurchTools;
using Korga.Server.Configuration;
using Korga.Server.EmailDelivery;
using Korga.Server.EmailRelay;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.ComponentModel.DataAnnotations;

namespace Korga.Server.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddKorgaOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection("Database"))
            .ValidateDataAnnotations();
        services.AddOptions<HostingOptions>()
            .Bind(configuration.GetSection("Hosting"))
            .ValidateDataAnnotations();
        services.AddOptions<Configuration.OpenIdConnectOptions>()
           .Bind(configuration.GetSection("OpenIdConnect"))
           .ValidateDataAnnotations();
        services.AddOptions<LdapOptions>()
             .Bind(configuration.GetSection("Ldap"))
             .ValidateDataAnnotations();
        services.AddOptions<ChurchToolsOptions>()
            .Bind(configuration.GetSection("ChurchTools"))
            .Validate(options =>
            {
                if (!options.EnableSync) return true;
                ValidationContext context = new(options);
                return Validator.TryValidateObject(options, context, null);
            });
        services.AddOptions<EmailDeliveryOptions>()
            .Bind(configuration.GetSection("EmailDelivery"))
            .Validate(options =>
            {
                if (!options.Enable) return true;
                ValidationContext context = new(options);
                return Validator.TryValidateObject(options, context, null);
            });
        services.AddOptions<EmailRelayOptions>()
            .Bind(configuration.GetSection("EmailRelay"))
            .Validate(options =>
            {
                if (!options.Enable) return true;
                ValidationContext context = new(options);
                return Validator.TryValidateObject(options, context, null);
            });

        return services;
    }

    public static IServiceCollection AddKorgaMySqlDatabase(this IServiceCollection services)
    {
        services.AddDbContextPool<DatabaseContext>((services, optionsBuilder) =>
        {
            var options = services.GetRequiredService<IOptions<DatabaseOptions>>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            optionsBuilder.UseLoggerFactory(loggerFactory);
            optionsBuilder.UseMySql(
                options.Value.ConnectionString,
                ServerVersion.AutoDetect(options.Value.ConnectionString),
                builder =>
                {
                    builder.MigrationsAssembly($"{nameof(Korga)}.{nameof(Server)}");
                    builder.EnableRetryOnFailure();
                });
        });

        return services;
    }

    public static IServiceCollection AddOpenIdConnectAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        SameSiteMode sameSiteMode = environment.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Strict;

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.Cookie.SameSite = sameSiteMode;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;
                options.LoginPath = PathString.Empty;
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.CorrelationCookie.SameSite = sameSiteMode;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.NonceCookie.SameSite = sameSiteMode;
                options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = configuration.GetValue<string>("OpenIdConnect:Authority");
                options.ClientId = configuration.GetValue<string>("OpenIdConnect:ClientId");
                options.ClientSecret = configuration.GetValue<string>("OpenIdConnect:ClientSecret");
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.RequireHttpsMetadata = false;
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Events.OnRedirectToIdentityProvider = context =>
                {
                    context.HandleResponse();

                    if (environment.IsDevelopment() && context.Request.Host.Equals(new HostString("localhost", 50805)))
                    {
                        context.Properties.RedirectUri = "http://localhost:8080";
                    }
                    else
                    {
                        context.Properties.RedirectUri = (context.Request.IsHttps ? "https://" : "http://") + context.Request.Host + context.Request.PathBase;
                    }

                    #region Code ported from OpenIdConnectHandler.cs
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.State))
                    {
                        context.Properties.Items[OpenIdConnectDefaults.UserstatePropertiesKey] = context.ProtocolMessage.State;
                    }

                    // When redeeming a 'code' for an AccessToken, this value is needed
                    context.Properties.Items.Add(OpenIdConnectDefaults.RedirectUriForCodePropertiesKey, context.ProtocolMessage.RedirectUri);

                    context.ProtocolMessage.State = options.StateDataFormat.Protect(context.Properties);
                    #endregion

                    context.Response.StatusCode = 401;
                    return context.Response.WriteAsJsonAsync(new { OpenIdConnectRedirectUrl = context.ProtocolMessage.CreateAuthenticationRequestUrl() });
                };
            });

        return services;
    }
}
