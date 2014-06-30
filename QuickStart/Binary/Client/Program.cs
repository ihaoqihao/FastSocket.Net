using System;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.ThreadPool.SetMinThreads(30, 30);
            Sodao.FastSocket.SocketBase.Log.Trace.EnableConsole();
            Sodao.FastSocket.SocketBase.Log.Trace.EnableDiagnostic();
            Console.WriteLine("press any key start...");

            var client = new Sodao.FastSocket.Client.AsyncBinarySocketClient(8192, 8192, 3000, 3000);
            //注册服务器节点，这里可注册多个(name不能重复）
            client.RegisterServerNode("127.0.0.1:8401", new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8401));

            Console.ReadLine();
            Call(client, 0);
            Console.ReadLine();
        }

        static private void Call(Sodao.FastSocket.Client.AsyncBinarySocketClient client, long i)
        {
            var bytes = new byte[new Random().Next(200, 700)];
            Buffer.BlockCopy(BitConverter.GetBytes(i), 0, bytes, 0, 8);

            Task.Factory.ContinueWhenAll(new Task[] 
            { 
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
                client.Send("each", bytes, res => BitConverter.ToInt64(res.Buffer, 0)) ,
            },
            arr =>
            {
                foreach (var child in arr)
                    if (child.IsFaulted) Console.WriteLine(child.Exception.ToString());

                Console.Write(i.ToString() + " ");
                Call(client, i + 1);
            });
        }
    }
}