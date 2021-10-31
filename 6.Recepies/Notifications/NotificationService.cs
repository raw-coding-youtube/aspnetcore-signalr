using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Notifications
{
    public record Notification(string ForUserId, string Message);

    public interface INotificationSink
    {
        ValueTask PushAsync(Notification notification);
    }

    public class NotificationService : BackgroundService, INotificationSink
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationService> _logger;
        private readonly ConnectionMultiplexer _redis;
        private readonly Channel<Notification> _channel;

        public ValueTask PushAsync(Notification notification) => _channel.Writer.WriteAsync(notification);

        public NotificationService(
            IServiceProvider serviceProvider,
            ILogger<NotificationService> logger
        )
        {
            _channel = Channel.CreateUnbounded<Notification>();
            _serviceProvider = serviceProvider;
            _logger = logger;
            _redis = ConnectionMultiplexer.Connect("127.0.0.1");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Distributed Scenario
            await _redis.GetSubscriber().SubscribeAsync("notification", async (c, m) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var hub = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();
                var (forUserId, message) = JsonSerializer.Deserialize<Notification>(m);

                var payload = new { Message = message };
                _logger.LogInformation($"Sending redis notification '{m}'");
                await hub.Clients.User(forUserId).SendAsync("Notify", payload, stoppingToken);
            });

            // Local
            while (true)
            {
                try
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var (forUserId, message) = await _channel.Reader.ReadAsync(stoppingToken);

                    using var scope = _serviceProvider.CreateScope();

                    var hub = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

                    var payload = new { Message = message };
                    _logger.LogInformation($"Sending channel notification '{message}' to {forUserId}");
                    await hub.Clients.User(forUserId).SendAsync("Notify", payload, stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error in notification service.");
                }
            }
        }
    }
}