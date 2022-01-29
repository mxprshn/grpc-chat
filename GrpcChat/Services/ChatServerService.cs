using Grpc.Core;
using GrpcChat.Config;
using GrpcChat.Handlers;
using GrpcChat.Interfaces;

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
            IAsyncStreamReader<ChatMessage> streamReader,
            IServerStreamWriter<ChatMessage> streamWriter,
            ServerCallContext context)
    {
        if (Interlocked.CompareExchange(ref isSessionActiveValue, 1, 0) == 1)
        {
            await streamWriter.WriteAsync(new ChatMessage { Name = _username, Text = "Server has active session" });
            context.Status = Status.DefaultCancelled;
            return;
        }

        var messageLoopHandler = new MessageLoopHandler(streamReader, streamWriter);

        Console.WriteLine("New client connected, messages are ready to be accepted in console.");
        Console.WriteLine("Send 'q' to stop dialog.");

        var clientTask = HandleClientMessage(messageLoopHandler, context);
        var serverTask = messageLoopHandler.HandleSendLoop(_username);

        await Task.WhenAll(clientTask, serverTask);

        Interlocked.Exchange(ref isSessionActiveValue, 0);
        Console.WriteLine("Session is closed");
    }

    private async Task HandleClientMessage(IMessageLoopHandler messageLoopHandler, ServerCallContext context)
    {
        await messageLoopHandler.HandleReceiveLoop(context.CancellationToken);
        
        Interlocked.Exchange(ref isSessionActiveValue, 0);
        Console.WriteLine("Client disconnected");
    }
}
