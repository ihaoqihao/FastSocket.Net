using Sodao.FastSocket.SocketBase;
using System;

namespace Sodao.FastSocket.Server.Protocol
{
    /// <summary>
    /// 协议接口
    /// </summary>
    /// <typeparam name="TCommandInfo"></typeparam>
    public interface IProtocol<TCommandInfo> where TCommandInfo : Command.ICommandInfo
    {
        /// <summary>
        /// Find CommandInfo
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="maxMessageSize"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        TCommandInfo FindCommandInfo(IConnection connection, ArraySegment<byte> buffer,
            int maxMessageSize, out int readlength);
    }
}