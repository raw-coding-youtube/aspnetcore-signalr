using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Authentication
{
    // [Authorize]
    // [Authorize(AuthenticationSchemes = Startup.CustomCookieScheme)]
    [Authorize(AuthenticationSchemes = Startup.CustomCookieScheme + "," + Startup.CustomTokenScheme)]
    public class ProtectedHub : Hub
    {
        [Authorize("Cookie")]
        public object CookieProtected()
        {
            return CompileResult();
        }

        [Authorize("Token")]
        public object TokenProtected()
        {
            return CompileResult();
        }

        private object CompileResult() =>
            new
            {
                UserId = Context.UserIdentifier,
                Claims = Context.User.Claims.Select(x => new { x.Type, x.Value })
            };
    }
}