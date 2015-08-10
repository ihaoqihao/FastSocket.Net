using System;
using System.Linq;
using System.Text;

namespace Sodao.FastSocket.Client.Protocol
{
    /// <summary>
    /// 命令行协议
    /// </summary>
    public sealed class CommandLineProtocol : IProtocol<Messaging.CommandLineMessage>
    {
        static private readonly string[] SPLITER =
            new string[] { " " };

        /// <summary>
        /// return false
        /// </summary>
        public bool IsAsync
        {
            get { return false; }
        }
        /// <summary>
        /// return 1
        /// </summary>
        public int DefaultSyncSeqID
        {
            get { return 1; }
        }
        /// <summary>
        /// parse
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        /// <exception cref="BadProtocolException">bad command line protocol</exception>
        public Messaging.CommandLineMessage Parse(SocketBase.IConnection connection,
            ArraySegment<byte> buffer, out int readlength)
        {
            if (buffer.Count < 2)
            {
                readlength = 0;
                return null;
            }

            //查找\r\n标记符
            for (int i = buffer.Offset, len = buffer.Offset + buffer.Count; i < len; i++)
            {
                if (buffer.Array[i] == 13 && i + 1 < len && buffer.Array[i + 1] == 10)
                {
                    readlength = i + 2 - buffer.Offset;

                    if (readlength == 2) return new Messaging.CommandLineMessage(1, string.Empty);

                    string command = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, readlength - 2);
                    var arr = command.Split(SPLITER, StringSplitOptions.RemoveEmptyEntries);

                    if (arr.Length == 0) return new Messaging.CommandLineMessage(1, string.Empty);
                    if (arr.Length == 1) return new Messaging.CommandLineMessage(1, arr[0]);
                    return new Messaging.CommandLineMessage(1, arr[0], arr.Skip(1).ToArray());
                }
            }
            readlength = 0;
            return null;
        }
    }
}