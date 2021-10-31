using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Notifications
{
    public class AuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public AuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Context.Request.Query.TryGetValue("user", out var userId))
                return Task.FromResult(AuthenticateResult.Fail("no user id in query string"));


            Logger.LogInformation($"logged in as {userId}");
            var claim = new Claim("userid", userId);
            var identity = new ClaimsIdentity(new[] { claim }, "MyScheme");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "MyScheme");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}