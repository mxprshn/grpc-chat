namespace GrpcChat.Interfaces
{
    public interface IChatRunnerBuilder
    {
        IChatRunnerBuilder WithUsername(string username);
        IChatRunner Build();
    }
}
