using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace StreamingData_7
{
    public class StreamingHub : Hub
    {
        public ChannelReader<string> Download(
            Data data,
            CancellationToken cancellationToken
        )
        {
            var channel = Channel.CreateUnbounded<string>();

            Task.Run(async () =>
            {
                try
                {
                    for (var i = 0; i < data.Count; i++)
                    {
                        await channel.Writer.WriteAsync($"{i}_{data.Message}", cancellationToken);
                        await Task.Delay(1000, cancellationToken);
                    }
                }
                catch (Exception ignored)
                {
                }
                finally
                {
                    channel.Writer.Complete();
                }
            }, cancellationToken);


            return channel.Reader;
        }

        public async Task Upload(ChannelReader<Data> dataStream)
        {
            while (await dataStream.WaitToReadAsync())
            {
                if (dataStream.TryRead(out var data))
                {
                    Console.WriteLine("Received Data: {0},{1}", data.Count, data.Message);
                }
            }
        }
    }
}