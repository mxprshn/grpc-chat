namespace GrpcChat.Interfaces
{
    public interface ISessionHandler
    {
        void StartSession();
        void StopSession();
        bool IsSessionActive { get; }
    }
}
