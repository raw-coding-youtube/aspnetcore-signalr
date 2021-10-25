using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DrawingGame
{
    public class AuthenticationHandler : CookieAuthenticationHandler
    {
        public AuthenticationHandler(
            IOptionsMonitor<CookieAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock
        ) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var result = await base.HandleAuthenticateAsync();
            if (result.Succeeded)
            {
                return result;
            }

            var guid = Guid.NewGuid().ToString();
            var claim = new Claim("UserId", guid);

            ClaimsIdentity identity = new ClaimsIdentity(new[] { claim }, "Default");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
            await Context.SignInAsync("Default", claimsPrincipal);
            AuthenticationTicket ticket = new AuthenticationTicket(claimsPrincipal, "Default");
            return AuthenticateResult.Success(ticket);
        }
    }
}