using Sodao.FastSocket.Server;
using Sodao.FastSocket.Server.Messaging;
using Sodao.FastSocket.SocketBase;
using System;

namespace CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketServerManager.Init();
            SocketServerManager.Start();

            Console.ReadLine();
        }
    }

    /// <summary>
    /// 实现自定义服务
    /// </summary>
    public class MyService : AbsSocketService<CommandLineMessage>
    {
        public override void OnConnected(IConnection connection)
        {
            base.OnConnected(connection);
            Console.WriteLine(connection.RemoteEndPoint.ToString() + " connected");
        }
        public override void OnReceived(IConnection connection, CommandLineMessage message)
        {
            base.OnReceived(connection, message);
            Console.WriteLine(message.CmdName);
            message.Reply(connection, DateTime.UtcNow.ToString());
        }
        public override void OnDisconnected(IConnection connection, Exception ex)
        {
            base.OnDisconnected(connection, ex);
            Console.WriteLine(connection.RemoteEndPoint.ToString() + " disconnected");
        }
    }
}