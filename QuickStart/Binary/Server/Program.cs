using System;
using Sodao.FastSocket.Server;
using Sodao.FastSocket.Server.Command;
using Sodao.FastSocket.SocketBase;
using System.Linq;

namespace Server
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
    public class MyService : CommandSocketService<AsyncBinaryCommandInfo>
    {
        /// <summary>
        /// 当连接时会调用此方法
        /// </summary>
        /// <param name="connection"></param>
        public override void OnConnected(IConnection connection)
        {
            base.OnConnected(connection);
            Console.WriteLine(connection.RemoteEndPoint.ToString() + " connected");
        }
        /// <summary>
        /// 当连接断开时会调用此方法
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        public override void OnDisconnected(IConnection connection, Exception ex)
        {
            base.OnDisconnected(connection, ex);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(connection.RemoteEndPoint.ToString() + " disconnected");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        /// <summary>
        /// 当发生错误时会调用此方法
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        public override void OnException(IConnection connection, Exception ex)
        {
            base.OnException(connection, ex);
            Console.WriteLine("error: " + ex.ToString());
        }
        /// <summary>
        /// 当服务端发送Packet完毕会调用此方法
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        /// <param name="isSuccess"></param>
        public override void OnSendCallback(IConnection connection, Packet packet, bool isSuccess)
        {
            base.OnSendCallback(connection, packet, isSuccess);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("send " + isSuccess.ToString());
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        /// <summary>
        /// 处理未知的命令
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandInfo"></param>
        protected override void HandleUnKnowCommand(IConnection connection, AsyncBinaryCommandInfo commandInfo)
        {
            Console.WriteLine("unknow command: " + commandInfo.CmdName);
        }
    }

    /// <summary>
    /// sum command
    /// 用于将一组int32数字求和并返回
    /// </summary>
    public sealed class SumCommand : ICommand<AsyncBinaryCommandInfo>
    {
        /// <summary>
        /// 返回服务名称
        /// </summary>
        public string Name
        {
            get { return "sum"; }
        }
        /// <summary>
        /// 执行命令并返回结果
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandInfo"></param>
        public void ExecuteCommand(IConnection connection, AsyncBinaryCommandInfo commandInfo)
        {
            if (commandInfo.Buffer == null || commandInfo.Buffer.Length == 0)
            {
                Console.WriteLine("sum参数为空");
                connection.BeginDisconnect();
                return;
            }
            if (commandInfo.Buffer.Length % 4 != 0)
            {
                Console.WriteLine("sum参数错误");
                connection.BeginDisconnect();
                return;
            }

            int skip = 0;
            var arr = new int[commandInfo.Buffer.Length / 4];
            for (int i = 0, l = arr.Length; i < l; i++)
            {
                arr[i] = BitConverter.ToInt32(commandInfo.Buffer, skip);
                skip += 4;
            }

            commandInfo.Reply(connection, BitConverter.GetBytes(arr.Sum()));
        }
    }
}