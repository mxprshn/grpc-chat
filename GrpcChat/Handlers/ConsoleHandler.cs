namespace GrpcChat.Handlers
{
    public class ConsoleInputHandler
    {
        public bool HandleIsServerInput()
        {
            Console.Write("Enter [y]/[Y] to use server mode: ");
            var isServerInput = Console.ReadLine();
            return string.Equals(isServerInput, "y", StringComparison.OrdinalIgnoreCase);
        }

        public int HandlePortInput(bool isServer)
        {
            int port;
            var enterPortPromptString = $"Enter port {(isServer ? "to host server on" : "of server to connect to")}";

            string? portInput;
            do
            {
                Console.WriteLine(enterPortPromptString);
                portInput = Console.ReadLine();
            } while (!int.TryParse(portInput, out port));

            return port;
        }
    }
}
