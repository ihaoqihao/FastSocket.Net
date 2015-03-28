using System;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// socket service interface.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface ISocketService<TMessage> where TMessage : class, Messaging.IMessage
    {
        /// <summary>
        /// 当建立socket连接时，会调用此方法
        /// </summary>
        /// <param name="connection"></param>
        void OnConnected(SocketBase.IConnection connection);
        /// <summary>
        /// 发送回调
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        /// <param name="isSuccess"></param>
        void OnSendCallback(SocketBase.IConnection connection, SocketBase.Packet packet, bool isSuccess);
        /// <summary>
        /// 当接收到客户端新消息时，会调用此方法.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        void OnReceived(SocketBase.IConnection connection, TMessage message);
        /// <summary>
        /// 当socket连接断开时，会调用此方法
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        void OnDisconnected(SocketBase.IConnection connection, Exception ex);
        /// <summary>
        /// 当发生异常时，会调用此方法
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        void OnException(SocketBase.IConnection connection, Exception ex);
    }
}