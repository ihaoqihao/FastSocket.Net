using System;

namespace Sodao.FastSocket.Server.Protocol
{
    /// <summary>
    /// a upd protocol
    /// </summary>
    /// <typeparam name="TCommandInfo"></typeparam>
    public interface IUdpProtocol<TCommandInfo> where TCommandInfo : Command.ICommandInfo
    {
        /// <summary>
        /// find command info
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        TCommandInfo FindCommandInfo(ArraySegment<byte> buffer);
    }
}