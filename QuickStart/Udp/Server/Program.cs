using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Sodao.FastSocket.Server.UdpServer<MyCmdInfo>(9500, new UdpProtocol(), new UdpService());
            server.Start();
            Console.ReadLine();
        }
    }

    public class UdpService : Sodao.FastSocket.Server.AbsUdpService<MyCmdInfo>
    {
        public override void OnReceived(Sodao.FastSocket.Server.UdpSession session, MyCmdInfo cmdInfo)
        {
            Console.WriteLine(cmdInfo.CmdName);
        }

        public void OnError(Sodao.FastSocket.Server.UdpSession session, Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public class UdpProtocol : Sodao.FastSocket.Server.Protocol.IUdpProtocol<MyCmdInfo>
    {
        public MyCmdInfo FindCommandInfo(ArraySegment<byte> buffer)
        {
            //<<len:32-little-endia,cmdName:8,payload:binary>>
            //len = cmdName.length(1) + payload.length
            if (buffer.Count < 4) { return null; }
            var len = BitConverter.ToInt32(buffer.Array, buffer.Offset);
            if (len < 1) return null;
            if (buffer.Count < len + 4) return null;

            var payload = new byte[len - 1];
            Buffer.BlockCopy(buffer.Array, buffer.Offset, payload, 0, payload.Length);
            return new MyCmdInfo(buffer.Array[buffer.Offset + 4].ToString(), payload);
        }
    }

    public class MyCmdInfo : Sodao.FastSocket.Server.Command.ICommandInfo
    {
        /// <summary>
        /// new
        /// </summary>
        /// <param name="cmdName"></param>
        /// <param name="payload"></param>
        public MyCmdInfo(string cmdName, byte[] payload)
        {
            this.CmdName = cmdName;
            this.Payload = payload;
        }

        public string CmdName
        {
            get;
            private set;
        }
        public byte[] Payload
        {
            get;
            private set;
        }
    }
}