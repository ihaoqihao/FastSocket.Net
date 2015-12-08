using Sodao.FastSocket.Server;
using Sodao.FastSocket.Server.Messaging;
using Sodao.FastSocket.SocketBase;
using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketServerManager.Init();
            SocketServerManager.Start();

            //每隔10秒强制断开所有连接
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(1000 * 10);
                    IHost host;
                    if (SocketServerManager.TryGetHost("quickStart", out host))
                    {
                        var arr = host.ListAllConnection();
                        foreach (var c in arr) c.BeginDisconnect();
                    }
                }
            });
            Console.ReadLine();
        }
    }

    public class MyService : AbsSocketService<CommandLineMessage>
    {
        public override void OnConnected(IConnection connection)
        {
            base.OnConnected(connection);
            Console.WriteLine(connection.RemoteEndPoint.ToString() + " " + connection.ConnectionID.ToString() + " connected");
            connection.BeginReceive();
        }

        public override void OnReceived(IConnection connection, CommandLineMessage message)
        {
            base.OnReceived(connection, message);
            switch (message.CmdName)
            {
                case "echo":
                    message.Reply(connection, "echo_reply " + message.Parameters[0]);
                    break;
                case "init":
                    Console.WriteLine("connection:" + connection.ConnectionID.ToString() + " init");
                    message.Reply(connection, "init_reply ok");
                    break;
                default:
                    message.Reply(connection, "error unknow command ");
                    break;
            }
        }

        public override void OnDisconnected(IConnection connection, Exception ex)
        {
            base.OnDisconnected(connection, ex);
            Console.WriteLine(connection.RemoteEndPoint.ToString() + " disconnected");
        }

        public override void OnException(IConnection connection, Exception ex)
        {
            base.OnException(connection, ex);
            Console.WriteLine(ex.ToString());
        }
    }
}