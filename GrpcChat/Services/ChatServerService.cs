using Grpc.Core;
using GrpcChat.Config;

namespace GrpcChat.Services;

public class ChatServerService : Chat.ChatBase
{
    private string _username;
    private int isSessionActiveValue;

    public ChatServerService(ServerConfig serverConfig)
    {
        _username = serverConfig.Username;
        isSessionActiveValue = 0;
    }

    public override async Task SendMessage(
            IAsyncStreamReader<ChatMessage> requestStream,
            IServerStreamWriter<ChatMessage> responseStream,
            ServerCallContext context)
    {
        if (Interlocked.CompareExchange(ref isSessionActiveValue, 1, 0) == 1)
        {
            await responseStream.WriteAsync(new ChatMessage { Name = _username, Text = "Server has active session" });
            context.Status = Status.DefaultCancelled;
            return;
        }

        Console.WriteLine("New client connected, messages are ready to be accepted in console.");

        var clientTask = HandleClientMessage(requestStream, context);
        var serverTask = HandleServerMessage(responseStream, context);

        await Task.WhenAll(clientTask, serverTask);

        Interlocked.Exchange(ref isSessionActiveValue, 0);
        Console.WriteLine("Session is closed");
    }

    private async Task HandleServerMessage(IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
    {
        string? messageText;

        while (!context.CancellationToken.IsCancellationRequested)
        {
            messageText = Console.ReadLine();

            await responseStream.WriteAsync(new ChatMessage
            {
                Name = _username,
                Text = messageText,
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

        Interlocked.Exchange(ref isSessionActiveValue, 0);
        Console.WriteLine("Client disconnected");
    }
}
