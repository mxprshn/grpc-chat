using Spectre.Console;

namespace GrpcChat.Handlers
{
    public class ConsoleInputHandler
    {
        public string HandleUsernameInput() =>
            AnsiConsole.Ask<string>("What is your name?");

        public bool HandleIsServerInput() =>
            AnsiConsole.Confirm("Run as server?", false);

        public string HandleIpInput() =>
            AnsiConsole.Ask<string>("Enter IP address of the server:");

        public int HandlePortInput(bool isServer) =>
            AnsiConsole.Ask<int>($"Enter port {(isServer ? "to host the server on" : "of the server to connect to")}:");
    }
}
