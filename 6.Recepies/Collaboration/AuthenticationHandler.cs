using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Collaboration
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
            if (!Context.Request.Query.ContainsKey("user")
                || !Context.Request.Query.ContainsKey("color"))
            {
                return Task.FromResult(AuthenticateResult.Fail("bad params"));
            }

            var claim = new Claim("userid", Context.Request.Query["user"]);
            var colorClaim = new Claim("color", Context.Request.Query["color"]);
            var identity = new ClaimsIdentity(new[] { claim, colorClaim }, "MyScheme");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "MyScheme");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}