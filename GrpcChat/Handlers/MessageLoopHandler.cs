using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChat.Interfaces;

namespace GrpcChat.Handlers
{
    /// <summary>
    /// Class implementing logic of client-server message excehnging loops
    /// </summary>
    public class MessageLoopHandler : IMessageLoopHandler
    {
        private readonly IAsyncStreamReader<ChatMessage> _streamReader;
        private readonly IAsyncStreamWriter<ChatMessage> _streamWriter;

        public MessageLoopHandler(
            IAsyncStreamReader<ChatMessage> streamReader,
            IAsyncStreamWriter<ChatMessage> streamWriter)
        {
            _streamReader = streamReader;
            _streamWriter = streamWriter;
        }

        public async Task HandleReceiveLoop(CancellationToken? token = null)
        {
            if (token.HasValue)
            {
                var cancelSource = new TaskCompletionSource<bool>();
                token.Value.Register(() => cancelSource.SetResult(false));

                var messageAvailable = async () =>
                {
                    var requestTask = _streamReader.MoveNext(CancellationToken.None);
                    var completed = await Task.WhenAny(cancelSource.Task, requestTask);
                    return completed.Result;
                };

                while (await messageAvailable())
                {
                    var message = _streamReader.Current;
                    Console.WriteLine($"{message.Name}: {message.Text}");
                }

                return;
            }

            while (await _streamReader.MoveNext())
            {
                var message = _streamReader.Current;
                Console.WriteLine($"{message.Name}: {message.Text}");
            }
        }

        public async Task HandleSendLoop(string? username, CancellationToken? token = null)
        {
            string? messageText;
            var messageUsername = username ?? "unknown";

            if (token.HasValue)
            {
                while (!token.Value.IsCancellationRequested)
                {
                    messageText = Console.ReadLine();

                    await _streamWriter.WriteAsync(new ChatMessage
                    {
                        Time = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
                        Name = messageUsername,
                        Text = messageText,
                    });
                }

                return;
            }

            messageText = Console.ReadLine();
            while (messageText is not null && !messageText.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                await _streamWriter.WriteAsync(new ChatMessage
                {
                    Time = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
                    Name = messageUsername,
                    Text = messageText
                });

                messageText = Console.ReadLine();
            }
        }
    }
}
