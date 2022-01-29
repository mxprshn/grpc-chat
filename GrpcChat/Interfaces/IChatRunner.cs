namespace GrpcChat.Interfaces
{
    public interface IChatRunner
    {
        Task RunAsServer(WebApplicationBuilder builder);
        Task RunAsClient(string serverAddress);
    }
}
