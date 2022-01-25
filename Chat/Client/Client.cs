using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chat.Client
{
    public class Client : BackgroundService
    {
        private readonly ILogger<Client> _logger;
        private readonly string _url;

        public Client(ILogger<Client> logger)
        {
            _logger = logger;
            _url = "http://localhost:5000";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var name = "John Wick";

            using var channel = GrpcChannel.ForAddress(_url);
            var client = new Chat.ChatClient(channel);

            using var streaming = client.SendMessage(new Metadata
            {
                new Metadata.Entry("name", name)
            });

            var response = Task.Run(async () =>
            {
                while (await streaming.ResponseStream.MoveNext())
                {
                    Console.WriteLine($"{streaming.ResponseStream.Current.Name}: {streaming.ResponseStream.Current.Text}");
                }
            });

            Console.Write("John's input: ");
            var line = Console.ReadLine();
            while (!stoppingToken.IsCancellationRequested)
            {
                await streaming.RequestStream.WriteAsync(new ChatMessage
                {
                    Time = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
                    Name = name,
                    Text = line
                });

                Console.Write("John's input: ");
                line = Console.ReadLine();
                DeletePrevConsoleLine();
            }

            await streaming.RequestStream.CompleteAsync();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void DeletePrevConsoleLine()
        {
            if (Console.CursorTop == 0) return;
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
    }
}
