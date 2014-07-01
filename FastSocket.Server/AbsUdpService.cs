using System;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// udp service interface.
    /// </summary>
    /// <typeparam name="TCommandInfo"></typeparam>
    public abstract class AbsUdpService<TCommandInfo> where TCommandInfo : class, Command.ICommandInfo
    {
        /// <summary>
        /// OnReceived
        /// </summary>
        /// <param name="session"></param>
        /// <param name="cmdInfo"></param>
        public virtual void OnReceived(UdpSession session, TCommandInfo cmdInfo)
        {
        }
    }
}