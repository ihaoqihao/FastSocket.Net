using System;

namespace Sodao.FastSocket.SocketBase
{
    /// <summary>
    /// 消息处理handler
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="readlength"></param>
    public delegate void MessageProcessHandler(ArraySegment<byte> buffer, int readlength);

    /// <summary>
    /// message received eventArgs
    /// </summary>
    public sealed class MessageReceivedEventArgs
    {
        /// <summary>
        /// process callback
        /// </summary>
        private readonly MessageProcessHandler _processCallback = null;
        /// <summary>
        /// Buffer
        /// </summary>
        public readonly ArraySegment<byte> Buffer;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="processCallback"></param>
        /// <exception cref="ArgumentNullException">processCallback is null</exception>
        public MessageReceivedEventArgs(ArraySegment<byte> buffer, MessageProcessHandler processCallback)
        {
            if (processCallback == null) throw new ArgumentNullException("processCallback");
            this.Buffer = buffer;
            this._processCallback = processCallback;
        }

        /// <summary>
        /// 设置已读取长度
        /// </summary>
        /// <param name="readlength"></param>
        public void SetReadlength(int readlength)
        {
            this._processCallback(this.Buffer, readlength);
        }
    }
}