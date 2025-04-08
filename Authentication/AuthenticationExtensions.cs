using dotnet_api_extensions.Authentication;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

using static dotnet_api_extensions.Authentication.AuthenticationConstants;

namespace dotnet_api_extensions.Authentication
{
    /// <summary>
    /// To add authentication extensions
    /// </summary>
    public static class AuthenticationExtensions
    {
        internal static IEnumerable<string> GetOAuthProtectionPurposes(this IConfigurationSection authenticationSection)
            => authenticationSection.GetAuthProtectionPurposes(DefaultTokenDataProtectionPurposes, CommaSeparatedOAuthDataProtectionPurposes);

        internal static IEnumerable<string> GetCookieAuthProtectionPurposes(this IConfigurationSection authenticationSection)
            => authenticationSection.GetAuthProtectionPurposes(DefaultDataProtectionPurposes, CommaSeparatedDataProtectionPurposes);

        internal static IEnumerable<string> GetAuthProtectionPurposes(this IConfiguration authenticationSection, IList<string> defaultPurposes, string configKey)
        {
            var dataProtectionPurposes = defaultPurposes;

            if (authenticationSection != null)
            {
                var commaSeparatedDataProtectionPurposes = authenticationSection[configKey];

                if (!string.IsNullOrEmpty(commaSeparatedDataProtectionPurposes))
                {
                    dataProtectionPurposes = commaSeparatedDataProtectionPurposes.Split(",");
                }
            }

            return dataProtectionPurposes;
        }

        internal static IConfigurationSection GetAuthenticationSection(this IConfiguration configuration)
        {
            var sectionExists = configuration.GetChildren().Any(item => item.Key == AuthenticationSection);

            if (!sectionExists) return null;

            return configuration.GetSection(AuthenticationSection);
        }

        internal static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var authenticationSection = configuration.GetAuthenticationSection();

            var cookieName = authenticationSection[CookieName];
            if (string.IsNullOrEmpty(cookieName)) cookieName = DefaultCookieName;

            var dataProtectionPurposes = authenticationSection.GetCookieAuthProtectionPurposes();

            var authenticationScheme = authenticationSection[AuthenticationConstants.AuthenticationScheme];
            if (string.IsNullOrEmpty(authenticationScheme)) authenticationScheme = DefaultAuthenticationScheme;

            services.AddAuthentication(authenticationScheme)
                .AddCookie(authenticationScheme, options =>
                {
                    options.Cookie.Name = cookieName;
                    options.SlidingExpiration = true;
                    options.CookieManager = new ChunkingCookieManager();

                    if (double.TryParse(authenticationSection[ExpireTimeInMins], out double expireTimeInMins))
                    {
                        options.ExpireTimeSpan = TimeSpan.FromMinutes(expireTimeInMins);
                    }

                    if (!string.IsNullOrEmpty(authenticationSection[CookieDomain]))
                    {
                        options.Cookie.Domain = authenticationSection[CookieDomain];
                    }

                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                    //Set ticket data format using data protection provider
                    var serviceProvider = services.BuildServiceProvider();
                    var dataProtectionProvider = serviceProvider.GetRequiredService<IDataProtectionProvider>();

                    options.TicketDataFormat = new TicketDataFormat(dataProtectionProvider.CreateProtector(dataProtectionPurposes));

                    //Return 401 if authentication challenge fails. Generally it returns 404. But, we need 401
                    //https://stackoverflow.com/questions/45878166/asp-net-core-2-0-disable-automatic-challenge
                    options.Events.OnRedirectToLogin = OnRedirectToLogin;
                    options.Events.OnRedirectToAccessDenied = OnRedirectToAccessDenied;
                });

            return services;
        }

        private static Task OnRedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        {
            context.Response.Headers["Location"] = context.RedirectUri;
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }

        private static Task OnRedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
        {
            context.Response.Headers["Location"] = context.RedirectUri;
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        }
    }
}