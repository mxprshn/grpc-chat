using GrpcChat.Interfaces;

namespace GrpcChat.Handlers
{
    public class SessionHandler : ISessionHandler
    {
        private int isSessionActiveValue = 0;

        public bool IsSessionActive
        {
            get => Interlocked.CompareExchange(ref isSessionActiveValue, 1, 1) == 1;
            set
            {
                if (value)
                {
                    Interlocked.CompareExchange(ref isSessionActiveValue, 1, 0);
                }
                else
                {
                    Interlocked.CompareExchange(ref isSessionActiveValue, 0, 1);
                }
            }
        }

        public void StartSession()
            => IsSessionActive = true;

        public void StopSession()
            => IsSessionActive = false;
    }
}
