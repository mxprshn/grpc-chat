namespace GrpcChat.Interfaces
{
    /// <summary>
    /// Interface for running chat in either server or client mode
    /// </summary>
    public interface IChatRunner
    {
        Task RunAsServer(WebApplicationBuilder builder);
        Task RunAsClient(string serverAddress);
    }
}
