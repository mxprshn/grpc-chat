using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Services
{
    public class ChatService : Chat.ChatBase
    {
        private readonly ILogger<ChatService> _logger;

        public ChatService(ILogger<ChatService> logger)
        {
            _logger = logger;
        }

        public override async Task SendMessage(
            IAsyncStreamReader<ChatMessage> requestStream, 
            IServerStreamWriter<ChatMessage> responseStream, 
            ServerCallContext context)
        {
            var clientTask = HandleClientMessage(requestStream, context);
            var serverTask = HandleServerMessage(responseStream, context);

            await Task.WhenAll(clientTask, serverTask);
        }

        private static async Task HandleServerMessage(IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
        {
            string name = "Keanu Reeves";
            string line;

            while (!context.CancellationToken.IsCancellationRequested)
            {
                Console.Write("Keanu's input: ");
                line = Console.ReadLine();

                await responseStream.WriteAsync(new ChatMessage
                {
                    Time = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
                    Name = name,
                    Text = line
                });
            }
        }

        private async Task HandleClientMessage(IAsyncStreamReader<ChatMessage> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
            {
                var message = requestStream.Current;
                Console.WriteLine($"{message.Name}: {message.Text}");
            }
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
