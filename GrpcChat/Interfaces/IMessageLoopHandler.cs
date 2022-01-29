namespace GrpcChat.Interfaces
{
    /// <summary>
    /// Interface for handling client-server message exchanging loops
    /// </summary>
    public interface IMessageLoopHandler
    {
        Task HandleReceiveLoop(CancellationToken? token = null);
        Task HandleSendLoop(string? username, CancellationToken? token = null);
    }
}
