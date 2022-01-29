﻿using Grpc.Core;
using GrpcChat.Config;
using GrpcChat.Handlers;
using GrpcChat.Interfaces;

namespace GrpcChat.Services;

public class ChatServerService : Chat.ChatBase
{
    private string _username;
    private int isSessionActiveValue;
    private IMessageLoopHandler messageLoopHandler;

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

        messageLoopHandler = new MessageLoopHandler(streamReader, streamWriter);

        Console.WriteLine("New client connected, messages are ready to be accepted in console.");
        Console.WriteLine("Send 'q' to stop dialog.");

        var clientTask = HandleClientMessage(context);
        var serverTask = HandleServerMessage(context);

        await Task.WhenAll(clientTask, serverTask);

        Interlocked.Exchange(ref isSessionActiveValue, 0);
        Console.WriteLine("Session is closed");
    }

    private async Task HandleServerMessage(ServerCallContext context)
    {
        await messageLoopHandler.HandleSendLoop(_username, context.CancellationToken);
    }

    private async Task HandleClientMessage(ServerCallContext context)
    {
        await messageLoopHandler.HandleReceiveLoop(context.CancellationToken);
        
        Interlocked.Exchange(ref isSessionActiveValue, 0);
        Console.WriteLine("Client disconnected");
    }
}
