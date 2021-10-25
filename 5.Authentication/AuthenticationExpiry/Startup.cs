using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthenticationExpiry
{
    public class Startup
    {
        public const string CustomCookieScheme = nameof(CustomCookieScheme);

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, CustomCookie>(CustomCookieScheme, _ =>
                {
                });

            services.AddSignalR(o =>
            {
                o.AddFilter<AuthHubFilter>();
            });

            services.AddSingleton<IUserIdProvider, UserIdProvider>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ProtectedHub>("/protected");

                endpoints.Map("/get-cookie", ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.Cookies.Append("signalr-auth-cookie", Guid.NewGuid().ToString(), new()
                    {
                        Expires = DateTimeOffset.UtcNow.AddSeconds(30)
                    });
                    return ctx.Response.WriteAsync("");
                });
            });
        }

        public class CustomCookie : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public CustomCookie(
                IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger,
                UrlEncoder encoder,
                ISystemClock clock
            ) : base(options, logger, encoder, clock)
            {
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                if (Context.Request.Cookies.TryGetValue("signalr-auth-cookie", out var cookie))
                {
                    var claims = new Claim[]
                    {
                        new("user_id", cookie),
                        new("cookie", "cookie_claim"),
                        // load from cookie
                        new("expires", DateTimeOffset.UtcNow.AddSeconds(30).Ticks.ToString()),
                    };
                    var identity = new ClaimsIdentity(claims, CustomCookieScheme);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, new(), CustomCookieScheme);
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }

                return Task.FromResult(AuthenticateResult.Fail("signalr-auth-cookie not found"));
            }
        }

        public class UserIdProvider : IUserIdProvider
        {
            public string GetUserId(HubConnectionContext connection)
            {
                return connection.User?.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            }
        }
    }
}