using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace ServerSentEvents
{
    public class Startup
    {
        private readonly Channel<string> _channel;

        public Startup()
        {
            _channel = Channel.CreateUnbounded<string>();
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

            app.Use(async (ctx, next) =>
            {
                if (ctx.Request.Path.ToString().Equals("/sse"))
                {
                    var response = ctx.Response;
                    response.Headers.Add("Content-Type", "text/event-stream");

                    await response.WriteAsync("event: custom\r");
                    await response.WriteAsync("data: custom event data\r\r");
                    await response.Body.FlushAsync();

                    while (await _channel.Reader.WaitToReadAsync())
                    {
                        var message = await _channel.Reader.ReadAsync();
                        Console.WriteLine("sending message: " + message);
                        await response.WriteAsync($"data: {message}\r\r");

                        await response.Body.FlushAsync();
                    }
                }

                await next();
            });
        }
    }
}