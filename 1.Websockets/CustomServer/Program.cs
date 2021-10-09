using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CustomServer
{
    class Program
    {
        // https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_servers
        static async Task Main(string[] args)
        {
            var ip = new IPEndPoint(IPAddress.Loopback, 5000);
            var listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(ip);
            listenSocket.Listen();
            const string EOL = "\r\n";

            while (true)
            {
                var socket = await listenSocket.AcceptAsync();
                var stream = new NetworkStream(socket);
                var buffer = new byte[1024];

                while (true)
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    var data = Encoding.UTF8.GetString(buffer[..bytesRead]);
                    if (data.StartsWith("GET"))
                    {
                        // handshake https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_servers#the_websocket_handshake
                        Console.WriteLine(data);
                        var key = new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value;
                        var acceptWebsocket = Convert.ToBase64String(System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));

                        var responseMessage = "HTTP/1.1 101 Switching Protocols"
                                              + EOL + "Connection: Upgrade"
                                              + EOL + "Upgrade: websocket"
                                              + EOL + "Sec-WebSocket-Accept: " + acceptWebsocket
                                              + EOL + EOL;

                        var response = Encoding.UTF8.GetBytes(responseMessage);
                        await stream.WriteAsync(response, 0, response.Length);
                        stream.Flush();
                    }
                    else
                    {
                        // https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_servers#format
                        // Only for small messages, < 126 bytes

                        // var length = buffer[1] - 0b10000000;
                        var length = buffer[1] - 0x80;
                        var key = buffer[2..6];
                        var wsData = new byte[length];

                        for (int i = 0; i < length; i++)
                        {
                            var offset = 6 + i;
                            wsData[i] = (byte)(buffer[offset] ^ key[i % 4]);
                        }

                        var text = Encoding.UTF8.GetString(wsData);
                        Console.WriteLine("{0}", text);
                    }
                }
            }
        }
    }
}