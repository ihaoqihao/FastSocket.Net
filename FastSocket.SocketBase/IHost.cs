using System.Net.Sockets;

namespace Sodao.FastSocket.SocketBase
{
    /// <summary>
    /// socket connection host interface
    /// </summary>
    public interface IHost
    {
        /// <summary>
        /// get socket buffer size
        /// </summary>
        int SocketBufferSize { get; }
        /// <summary>
        /// get message buffer size
        /// </summary>
        int MessageBufferSize { get; }

        /// <summary>
        /// create new <see cref="IConnection"/>
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        IConnection NewConnection(Socket socket);
        /// <summary>
        /// get <see cref="IConnection"/> by connectionID
        /// </summary>
        /// <param name="connectionID"></param>
        /// <returns></returns>
        IConnection GetConnectionByID(long connectionID);
        /// <summary>
        /// list all <see cref="IConnection"/>
        /// </summary>
        /// <returns></returns>
        IConnection[] ListAllConnection();
        /// <summary>
        /// get connection count.
        /// </summary>
        /// <returns></returns>
        int CountConnection();

        /// <summary>
        /// 启动
        /// </summary>
        void Start();
        /// <summary>
        /// 停止
        /// </summary>
        void Stop();
    }
}