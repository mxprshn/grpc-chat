using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcChat;
using GrpcChat.Services;

Console.Write("Is server (true/false): ");
var isServer = bool.Parse(Console.ReadLine());
int port;

if (isServer)
{
    Console.Write("Port to host server: ");
    port = int.Parse(Console.ReadLine());

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
    Console.Write("Server port: ");
    port = int.Parse(Console.ReadLine());

    var httpHandler = new HttpClientHandler();
    httpHandler.ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    using var channel = GrpcChannel.ForAddress($"https://localhost:{port}",
        new GrpcChannelOptions {  HttpHandler = httpHandler });
    var client = new Chat.ChatClient(channel);

    var name = "Jo";
    Console.WriteLine("Write messages to send them to server");

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

    var line = Console.ReadLine();
    while (!line.Equals("q", StringComparison.OrdinalIgnoreCase))
    {
        await streaming.RequestStream.WriteAsync(new ChatMessage
        {
            Time = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
            Name = name,
            Text = line
        });

        line = Console.ReadLine();
    }

    await streaming.RequestStream.CompleteAsync();
}