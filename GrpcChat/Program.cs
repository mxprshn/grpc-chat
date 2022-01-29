using GrpcChat.ChatRunners;
using GrpcChat.Handlers;

var inputHandler = new ConsoleInputHandler();

var username = inputHandler.HandleUsernameInput();
var isServer = inputHandler.HandleIsServerInput();
var port = inputHandler.HandlePortInput(isServer);

var chatRunner = ChatRunner
    .WithPort(port)
    .WithUsername(username)
    .Build();

if (isServer)
{
    var builder = WebApplication.CreateBuilder(args);

    await chatRunner.RunAsServer(builder);

    return;
}

var ip = inputHandler.HandleIpInput();

await chatRunner.RunAsClient(ip);