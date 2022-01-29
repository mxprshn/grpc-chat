namespace GrpcChat.Interfaces
{
    public interface IMessageLoopHandler
    {
        Task HandleReceiveLoop(CancellationToken? token = null);
        Task HandleSendLoop(string username, CancellationToken? token = null);
    }
}
