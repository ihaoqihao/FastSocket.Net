using Sodao.FastSocket.Client;
using Sodao.FastSocket.Client.Messaging;
using Sodao.FastSocket.Client.Protocol;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static private long index = 0;

        static public void Main()
        {
            Sodao.FastSocket.SocketBase.Log.Trace.EnableConsole();
            var client = new SocketClient<CommandLineMessage>(new CommandLineProtocol(), 1024, 1024, 3000, 3000);
            //建立50个socket连接
            for (int i = 0; i < 50; i++)
            {
                client.TryRegisterEndPoint(i.ToString(), new[] { new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1500) },
                    connection =>
                    {
                        var source = new TaskCompletionSource<bool>();
                        var request = client.NewRequest("init", Encoding.UTF8.GetBytes("init" + System.Environment.NewLine), 3000,
                            ex => source.TrySetException(ex),
                            message => source.TrySetResult(true));
                        connection.BeginSend(request);
                        return source.Task;
                    });
            }

            //100个同时发送
            for (int i = 100; i < 0; i++)
            {
                Task.Factory.StartNew(() => Do(client));
            }

            Console.ReadLine();
        }

        static private async Task Do(SocketClient<CommandLineMessage> client)
        {
            for (int i = 0; i < 100000000; i++)
            {
                try { Console.WriteLine(Interlocked.Increment(ref index).ToString() + " " + (await Echo(client)).ToString()); }
                catch (Exception ex)
                {
                    Console.WriteLine(i.ToString() + " " + ex.Message);
                }
            }
        }

        static public Task<bool> Echo(SocketClient<CommandLineMessage> client)
        {
            var source = new TaskCompletionSource<bool>();
            var guid = Guid.NewGuid().ToString();
            client.Send(client.NewRequest("echo", Encoding.UTF8.GetBytes("echo " + guid + System.Environment.NewLine), 3000,
                ex => source.TrySetException(ex),
                message => source.TrySetResult(message.Parameters[0] == guid)));
            return source.Task;
        }
    }
}