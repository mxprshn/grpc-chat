namespace GrpcChat.Interfaces
{
    /// <summary>
    /// Interface for building chat runners with parameters
    /// </summary>
    public interface IChatRunnerBuilder
    {
        IChatRunnerBuilder WithUsername(string username);
        IChatRunner Build();
    }
}
