using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcChat;
using GrpcChat.Config;
using GrpcChat.Handlers;
using GrpcChat.Interfaces;
using GrpcChat.Services;

var inputHandler = new ConsoleInputHandler();

var username = inputHandler.HandleUsernameInput();
var isServer = inputHandler.HandleIsServerInput();
var port = inputHandler.HandlePortInput(isServer);

if (isServer)
{
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenLocalhost(port, listenOptions =>
        {
            listenOptions.UseHttps();
        });
    });
    builder.WebHost.ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();
    });
    builder.Services.AddGrpc();

    var config = new ServerConfig { Username = username };
    builder.Services.AddSingleton(config);
    builder.Services.AddSingleton<ISessionHandler>(new SessionHandler());

    var app = builder.Build();
    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapGrpcService<ChatService>();
        endpoints.MapGet("/", async context => await context.Response.WriteAsync("gRPC is being used for communication"));
    });
    Console.WriteLine("Write messages to send them to client");
    app.Run();
}
else
{
    var ip = inputHandler.HandleIpInput();

    var httpHandler = new HttpClientHandler();
    httpHandler.ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    using var channel = GrpcChannel.ForAddress($"https://{ip}:{port}",
        new GrpcChannelOptions {  HttpHandler = httpHandler });
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
        while (!line.Equals("q", StringComparison.OrdinalIgnoreCase) && !response.IsCompleted)
        {
            await streaming.RequestStream.WriteAsync(new ChatMessage
            {
                Time = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
                Name = username,
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