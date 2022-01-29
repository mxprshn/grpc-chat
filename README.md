# grpc-chat

![build](https://github.com/mxprshn/grpc-chat/actions/workflows/grpc-chat.yml/badge.svg?branch=feature/mvp_chat)

Console peer-to-peer chat written in C# with gRPC bidirectional streaming.

## How to build

Install .NET runtime (6.0.x) and run

```
dotnet run .\GrpcChat.sln
```

Or build the project directly from Visual Studio (2022).

## How to use

The application can be run either as a server or as a client (and connect to running server).

To run as a server:

```
What is your name? Johny
Run as server? [y/n] (n): y
Enter port to host the server on: 5001 (for example)
Waiting for client...
Oh, hi Mark
```

To run as a client:

```
What is your name? Mark
Run as server? [y/n] (n): n
Enter port of the server to connect to: 5001
Enter IP address of the server: localhost
Connecting...
Write messages to send them to server. Use 'q' to quit.
Johny: Oh, hi Mark
```

