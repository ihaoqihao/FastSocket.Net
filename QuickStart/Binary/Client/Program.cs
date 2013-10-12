using System;
using System.Linq;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Sodao.FastSocket.SocketBase.Log.Trace.EnableConsole();
            Sodao.FastSocket.SocketBase.Log.Trace.EnableDiagnostic();

            var client = new Sodao.FastSocket.Client.AsyncBinarySocketClient(8192, 8192, 3000, 3000);
            //注册服务器节点，这里可注册多个(name不能重复）
            client.RegisterServerNode("127.0.0.1:8401", new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8401));
            //client.RegisterServerNode("127.0.0.1:8402", new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.2"), 8401));

            for (int j = 0; j < 10000; j++)
            {
                //组织sum参数, 格式为<<i:32-limit-endian,....N>>
                //这里的参数其实也可以使用thrift, protobuf, bson, json等进行序列化，
                byte[] bytes = null;
                using (var ms = new System.IO.MemoryStream())
                {
                    for (int i = j; i <= j + 10; i++) ms.Write(BitConverter.GetBytes(i), 0, 4);
                    bytes = ms.ToArray();
                }

                //发送sum命令
                client.Send("sum", bytes, res => BitConverter.ToInt32(res.Buffer, 0)).ContinueWith(c =>
                {
                    if (c.IsFaulted) { Console.WriteLine(c.Exception.ToString()); return; }
                    Console.WriteLine(c.Result);
                });
            }
            Console.ReadLine();
        }
    }
}