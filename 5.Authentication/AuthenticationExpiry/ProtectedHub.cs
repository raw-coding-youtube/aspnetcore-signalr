using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AuthenticationExpiry
{
    [Authorize(AuthenticationSchemes = Startup.CustomCookieScheme)]
    public class ProtectedHub : Hub
    {
        public string AuthorizedResource()
        {
            return "authorized resource";
        }

        // no reason can be specified
        public void Abort() => Context.Abort();
    }
}