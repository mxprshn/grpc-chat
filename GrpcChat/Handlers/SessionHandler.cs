using GrpcChat.Interfaces;

namespace GrpcChat.Handlers
{
    public class SessionHandler : ISessionHandler
    {
        private bool isSessionActive;

        public SessionHandler()
        {
            isSessionActive = false;
        }

        public bool IsSessionActive
            => isSessionActive;

        public void StartSession()
            => isSessionActive = true;

        public void StopSession()
            => isSessionActive = false;
    }
}
