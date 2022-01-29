using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcChat.Config;
using GrpcChat.Interfaces;
using GrpcChat.Services;

namespace GrpcChat.ChatRunners
{
    public class ChatRunner : IChatRunner
    {
        private readonly int _port;
        private readonly string _username;

        public ChatRunner(int port, string username)
        {
            _port = port;
            _username = username;
        }

        public async Task RunAsServer(WebApplicationBuilder builder)
        {
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenLocalhost(_port, listenOptions =>
                {
                    listenOptions.UseHttps();
                });
            });
            builder.WebHost.ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
            });
            var config = new ServerConfig { Username = _username };
            builder.Services.AddSingleton(config);

            var service = new ChatServerService(config);
            builder.Services.AddSingleton(service);
            builder.Services.AddGrpc();

            var app = builder.Build();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ChatServerService>();
                endpoints.MapGet("/", async context => await context.Response.WriteAsync("gRPC is being used for communication"));
            });
            Console.WriteLine("Waiting for client...");

            await app.RunAsync();
        }

        public async Task RunAsClient(string ip)
        {
            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            using var channel = GrpcChannel.ForAddress($"https://{ip}:{_port}",
                new GrpcChannelOptions { HttpHandler = httpHandler });
            var client = new Chat.ChatClient(channel);

            Console.WriteLine("Connecting...");

            using var streaming = client.SendMessage(new Metadata());

            try
            {
                var response = Task.Run(async () =>
                {
                    while (await streaming.ResponseStream.MoveNext())
                    {
                        Console.WriteLine($"{streaming.ResponseStream.Current.Name}: {streaming.ResponseStream.Current.Text}");
                    }
                });

                Console.WriteLine("Write messages to send them to server. Use 'q' to quit.");
                var line = Console.ReadLine();
                while (line is not null && !line.Equals("q", StringComparison.OrdinalIgnoreCase) && !response.IsCompleted)
                {
                    await streaming.RequestStream.WriteAsync(new ChatMessage
                    {
                        Time = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
                        Name = _username,
                        Text = line
                    });

                    line = Console.ReadLine();
                }
            }
            catch
            {
                Console.WriteLine("Server rejected connection");
            }
            finally
            {
                await streaming.RequestStream.CompleteAsync();
            }
        }

        private class Builder : IChatRunnerBuilder
        {
            private readonly int _port;
            private string? _username;

            public Builder(int port)
            {
                _port = port;
            }

            public IChatRunnerBuilder WithUsername(string username)
            {
                _username = username;
                return this;
            }

            public IChatRunner Build()
            {
                if (string.IsNullOrEmpty(_username))
                {
                    _username = $"Unkown{Guid.NewGuid()}";
                }

                return new ChatRunner(_port, _username);
            }
        }

        public static IChatRunnerBuilder WithPort(int port) => new Builder(port);
    }
}
