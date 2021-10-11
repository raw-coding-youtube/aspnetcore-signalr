using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace StreamingData_8
{
    public class StreamingHub : Hub
    {
        private readonly string _id;

        public StreamingHub()
        {
            _id = Guid.NewGuid().ToString();
        }

        public string Call() => _id;

        public async IAsyncEnumerable<string> Download(
            Data data,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            for (var i = 0; i < data.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return $"{i}_{data.Message}_{_id}";

                await Task.Delay(1000, cancellationToken);
            }
        }

        public async Task Upload(IAsyncEnumerable<Data> dataStream)
        {
            await foreach (var data in dataStream)
            {
                Console.WriteLine("Received Data: {0},{1},{2}", data.Count, data.Message, _id);
            }
        }
    }
}