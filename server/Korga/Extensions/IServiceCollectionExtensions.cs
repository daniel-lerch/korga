using Korga.ChurchTools;
using Korga.Configuration;
using Korga.EmailDelivery;
using Korga.EmailRelay;
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
using System.Linq;

using OpenIdConnectOptions = Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions;
using KorgaOpenIdConnectOptions = Korga.Configuration.OpenIdConnectOptions;

namespace Korga.Extensions;

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
        services.AddOptions<KorgaOpenIdConnectOptions>()
           .Bind(configuration.GetSection("OpenIdConnect"))
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
                builder => builder.EnableRetryOnFailure());
        });

        return services;
    }

    public static IServiceCollection AddOpenIdConnectAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;
                options.LoginPath = PathString.Empty;
            })
            .AddOpenIdConnect();

        services.AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
            .Configure<ILogger<Startup>, IOptions<KorgaOpenIdConnectOptions>>((options, logger, openIdConnectOptions) =>
            {
                if (string.IsNullOrEmpty(openIdConnectOptions.Value.Authority)
                    || string.IsNullOrEmpty(openIdConnectOptions.Value.ClientId)
                    || string.IsNullOrEmpty(openIdConnectOptions.Value.ClientSecret))
                {
                    logger.LogWarning("OpenID Connect configuration is incomplete. Login will not work.");
                }

                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = openIdConnectOptions.Value.Authority;
                options.ClientId = openIdConnectOptions.Value.ClientId;
                options.ClientSecret = openIdConnectOptions.Value.ClientSecret;
                options.ResponseType = OpenIdConnectResponseType.Code;

                // response_mode=form_post only works when backend and identity provider are on the same site (same effective TLD)
                // because the Nonce and Correlation cookies won't be sent with a request initiated by the identity provider unless SameSite=None
                options.ResponseMode = OpenIdConnectResponseMode.Query;

                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Events.OnRedirectToIdentityProvider = context =>
                {
                    context.HandleResponse();
                    SetFrontendRedirectUri(context);
                    SetBackendRedirectUri(context, environment);

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
                options.Events.OnRedirectToIdentityProviderForSignOut = context =>
                {
                    context.HandleResponse();

                    SetFrontendRedirectUri(context);
                    SetBackendLogoutRedirectUri(context, environment);

                    #region Code ported from OpenIdConnectHandler.cs
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.State))
                    {
                        context.Properties.Items[OpenIdConnectDefaults.UserstatePropertiesKey] = context.ProtocolMessage.State;
                    }

                    context.ProtocolMessage.State = options.StateDataFormat.Protect(context.Properties);
                    #endregion

                    context.Response.StatusCode = 401;
                    return context.Response.WriteAsJsonAsync(new { OpenIdConnectRedirectUrl = context.ProtocolMessage.CreateLogoutRequestUrl() });
                };
            });

        return services;
    }

    private static void SetFrontendRedirectUri(RedirectContext context)
    {
        string? frontendUri = context.Request.Headers.Referer.FirstOrDefault();

        if (string.IsNullOrEmpty(frontendUri))
            frontendUri = context.Request.Headers.Origin.FirstOrDefault();

        if (string.IsNullOrEmpty(frontendUri))
            frontendUri = (context.Request.IsHttps ? "https://" : "http://") + context.Request.Host + context.Request.PathBase;

        context.Properties.RedirectUri = frontendUri;
    }

    private static void SetBackendRedirectUri(RedirectContext context, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            string? backendUri = context.Request.Headers.Referer.FirstOrDefault();

            if (string.IsNullOrEmpty(backendUri))
                backendUri = context.Request.Headers.Origin.FirstOrDefault();

            if (!string.IsNullOrEmpty(backendUri))
            {
                Uri uri = new(backendUri);
                context.ProtocolMessage.RedirectUri = uri.Scheme + Uri.SchemeDelimiter + uri.Authority + context.Request.PathBase + context.Options.CallbackPath;
            }
        }
    }

    private static void SetBackendLogoutRedirectUri(RedirectContext context, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            string? backendUri = context.Request.Headers.Referer.FirstOrDefault();

            if (string.IsNullOrEmpty(backendUri))
                backendUri = context.Request.Headers.Origin.FirstOrDefault();

            if (!string.IsNullOrEmpty(backendUri))
            {
                Uri uri = new(backendUri);
                context.ProtocolMessage.PostLogoutRedirectUri = uri.Scheme + Uri.SchemeDelimiter + uri.Authority + context.Request.PathBase + context.Options.SignedOutCallbackPath;
            }
        }
    }
}
