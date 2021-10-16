using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Authentication
{
    public class Startup
    {
        public const string CustomCookieScheme = nameof(CustomCookieScheme);
        public const string CustomTokenScheme = nameof(CustomTokenScheme);

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, CustomCookie>(CustomCookieScheme, _ =>
                {
                })
                .AddJwtBearer(CustomTokenScheme, o =>
                {
                    o.Events = new()
                    {
                        OnMessageReceived = (context) =>
                        {
                            var path = context.HttpContext.Request.Path;
                            if (path.StartsWithSegments("/protected")
                                || path.StartsWithSegments("/token"))
                            {
                                var accessToken = context.Request.Query["access_token"];

                                if (!string.IsNullOrWhiteSpace(accessToken))
                                {
                                    // context.Token = accessToken;

                                    var claims = new Claim[]
                                    {
                                        new("user_id", accessToken),
                                        new("token", "token_claim"),
                                    };
                                    var identity = new ClaimsIdentity(claims, CustomTokenScheme);
                                    context.Principal = new(identity);
                                    context.Success();
                                }
                            }

                            return Task.CompletedTask;
                        },
                    };
                });

            services.AddAuthorization(c =>
            {
                c.AddPolicy("Cookie", pb => pb
                    .AddAuthenticationSchemes(CustomCookieScheme)
                    .RequireAuthenticatedUser());

                c.AddPolicy("Token", pb => pb
                    // schema get's ignored in signalr
                    .AddAuthenticationSchemes(CustomTokenScheme)
                    .RequireClaim("token")
                    .RequireAuthenticatedUser());
            });

            services.AddSignalR();

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
                endpoints.MapHub<ProtectedHub>("/protected", o =>
                {
                    // o.Transports = HttpTransportType.LongPolling;
                });

                endpoints.Map("/get-cookie", ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.Cookies.Append("signalr-auth-cookie", Guid.NewGuid().ToString(), new()
                    {
                        Expires = DateTimeOffset.UtcNow.AddSeconds(30)
                    });
                    return ctx.Response.WriteAsync("");
                });

                endpoints.Map("/token", ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    return ctx.Response.WriteAsync(ctx.User?.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value);
                }).RequireAuthorization("Token");

                endpoints.Map("/cookie", ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    return ctx.Response.WriteAsync(ctx.User?.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value);
                }).RequireAuthorization("Cookie");
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