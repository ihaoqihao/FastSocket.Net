using System;

namespace Sodao.FastSocket.SocketBase
{
    /// <summary>
    /// send callback delegate
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="e"></param>
    public delegate void SendCallbackHandler(IConnection connection, SendCallbackEventArgs e);

    /// <summary>
    /// send callback eventArgs
    /// </summary>
    public sealed class SendCallbackEventArgs
    {
        #region Public Members
        /// <summary>
        /// packet
        /// </summary>
        public readonly Packet Packet;
        /// <summary>
        /// 状态
        /// </summary>
        public readonly SendCallbackStatus Status;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="status"></param>
        /// <exception cref="ArgumentNullException">packet is null</exception>
        public SendCallbackEventArgs(Packet packet, SendCallbackStatus status)
        {
            if (packet == null) throw new ArgumentNullException("packet");
            this.Packet = packet;
            this.Status = status;
        }
        #endregion
    }

    /// <summary>
    /// packet send status
    /// </summary>
    public enum SendCallbackStatus : byte
    {
        /// <summary>
        /// 发送成功
        /// </summary>
        Success = 1,
        /// <summary>
        /// 发送失败
        /// </summary>
        Failed = 2
    }
}