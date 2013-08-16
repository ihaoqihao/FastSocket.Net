using System;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// udp service interface.
    /// </summary>
    /// <typeparam name="TCommandInfo"></typeparam>
    public interface IUdpService<TCommandInfo> where TCommandInfo : class, Command.ICommandInfo
    {
        /// <summary>
        /// OnReceived
        /// </summary>
        /// <param name="session"></param>
        /// <param name="cmdInfo"></param>
        void OnReceived(UdpSession session, TCommandInfo cmdInfo);
        /// <summary>
        /// OnError
        /// </summary>
        /// <param name="session"></param>
        /// <param name="ex"></param>
        void OnError(UdpSession session, Exception ex);
    }
}