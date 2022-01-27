using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChat.Interfaces;

namespace GrpcChat.Services;

public class ChatService : Chat.ChatBase
{
    private readonly ISessionHandler _sessionHandler;

    public ChatService(ISessionHandler sessionHandler)
    {
        _sessionHandler = sessionHandler;
    }

    public override async Task SendMessage(
            IAsyncStreamReader<ChatMessage> requestStream,
            IServerStreamWriter<ChatMessage> responseStream,
            ServerCallContext context)
    {
        if (_sessionHandler.IsSessionActive)
        {
            await responseStream.WriteAsync(new ChatMessage { Name = "Mick", Text = "Server has active session" });
            context.Status = Status.DefaultCancelled;
            return;
        }

        _sessionHandler.StartSession();

        var clientTask = HandleClientMessage(requestStream, context);
        var serverTask = HandleServerMessage(responseStream, context);

        await Task.WhenAll(clientTask, serverTask);

        _sessionHandler.StopSession();

        Console.WriteLine("Session is closed");
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
        var cancelSource = new TaskCompletionSource<bool>();
        context.CancellationToken.Register(() => cancelSource.SetResult(false));

        var messageAvailable = async () =>
        {
            var requestTask = requestStream.MoveNext(CancellationToken.None);
            var completed = await Task.WhenAny(cancelSource.Task, requestTask);
            return completed.Result;
        };

        while (await messageAvailable())
        {
            var message = requestStream.Current;
            Console.WriteLine($"{message.Name}: {message.Text}");
        }

        Console.WriteLine("Client disconnected");
    }
}
