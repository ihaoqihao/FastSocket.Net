using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var udpClient = new System.Net.Sockets.UdpClient();
            udpClient.Connect(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 9500));
            udpClient.Send(new byte[] { 1, 0, 0, 0, 8 }, 5);

            Console.ReadLine();
        }
    }
}