using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GrpcChat.Services;

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
        while (!context.CancellationToken.IsCancellationRequested)
        {
            await responseStream.WriteAsync(new ChatMessage
            {
                Name = "Mick",
                Text = Console.ReadLine(),
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
}