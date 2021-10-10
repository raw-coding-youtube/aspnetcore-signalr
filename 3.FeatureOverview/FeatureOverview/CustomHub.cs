using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FeatureOverview
{
    public interface IClientInterface
    {
        Task ClientHook(Data data);
    }

    public class CustomHub : Hub<IClientInterface>
    {
        private readonly ILogger<CustomHub> _logger;

        public CustomHub(ILogger<CustomHub> logger) => _logger = logger;

        public void ServerHook(Data data)
        {
            _logger.LogInformation("Receiving data: {0}, {1}", data, Context.ConnectionId);
        }

        public Task PingAll()
        {
            _logger.LogInformation("pinging everyone");
            return Clients.All.ClientHook(new(111, "ping all"));
        }

        public Task SelfPing()
        {
            _logger.LogInformation("self pinging");
            return Clients.Caller.ClientHook(new(222, "self ping"));
        }

        [HubMethodName("invocation_with_return")]
        public Data JustAFunction()
        {
            return new(1, "returned data from JustAFunction");
        }

        // -------------------------------
        // on connected/disconnected hooks
        // -------------------------------
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}