using System;

namespace Sodao.FastSocket.Server.Messaging
{
    /// <summary>
    /// thrift message.
    /// </summary>
    public sealed class ThriftMessage : IMessage
    {
        /// <summary>
        /// payload
        /// </summary>
        public readonly byte[] Payload;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="payload"></param>
        public ThriftMessage(byte[] payload)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            this.Payload = payload;
        }
    }
}