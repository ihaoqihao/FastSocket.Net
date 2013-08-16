using System.Net;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// upd server interface
    /// </summary>
    public interface IUdpServer
    {
        /// <summary>
        /// 开始
        /// </summary>
        void Start();
        /// <summary>
        /// stop
        /// </summary>
        void Stop();
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="payload"></param>
        void SendTo(EndPoint endPoint, byte[] payload);
    }

    /// <summary>
    /// upd server interface
    /// </summary>
    /// <typeparam name="TCommandInfo"></typeparam>
    public interface IUdpServer<TCommandInfo> : IUdpServer where TCommandInfo : class, Command.ICommandInfo
    {

    }
}