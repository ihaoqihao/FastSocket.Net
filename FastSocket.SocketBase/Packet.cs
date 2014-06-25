using System;

namespace Sodao.FastSocket.SocketBase
{
    /// <summary>
    /// packet
    /// </summary>
    public class Packet
    {
        #region Members
        /// <summary>
        /// get or set sent size.
        /// </summary>
        internal int SentSize = 0;
        /// <summary>
        /// get the packet created time
        /// </summary>
        public readonly DateTime CreatedTime = DateTime.UtcNow;
        /// <summary>
        /// get payload
        /// </summary>
        public readonly byte[] Payload;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="payload"></param>
        /// <exception cref="ArgumentNullException">payload is null.</exception>
        public Packet(byte[] payload)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            this.Payload = payload;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// get or set tag object
        /// </summary>
        public object Tag { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// 获取一个值，该值指示当前packet是否已发送完毕.
        /// </summary>
        /// <returns>true表示已发送完毕</returns>
        public bool IsSent()
        {
            return this.SentSize >= this.Payload.Length;
        }
        #endregion
    }
}