using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LongPolling
{
    public class Startup
    {
        private readonly Channel<string> _channel;

        public Startup()
        {
            _channel = Channel.CreateUnbounded<string>();
            // Task.Run(async () =>
            // {
            //     int i = 0;
            //     while (true)
            //     {
            //         await _channel.Writer.WriteAsync($"data: {i++}");
            //         await Task.Delay(5000);
            //     }
            // });
        }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/listen", async ctx =>
                {
                    if (await _channel.Reader.WaitToReadAsync())
                    {
                        if (_channel.Reader.TryRead(out var data))
                        {
                            ctx.Response.StatusCode = 200;
                            await ctx.Response.WriteAsync(data);
                            return;
                        }
                    }

                    ctx.Response.StatusCode = 200;
                });

                endpoints.Map("/send", async ctx =>
                {
                    if (ctx.Request.Query.TryGetValue("m", out var m))
                    {
                        Console.WriteLine("message to send: " + m);
                        await _channel.Writer.WriteAsync(m);
                    }

                    ctx.Response.StatusCode = 200;
                });
            });
        }
    }
}