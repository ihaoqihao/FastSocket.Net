using System;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// thrift client
    /// </summary>
    public class ThriftClient : SocketClient<Messaging.ThriftMessage>
    {
        /// <summary>
        /// new
        /// </summary>
        public ThriftClient()
            : base(new Protocol.ThriftProtocol())
        {
        }
        /// <summary>
        /// new
        /// </summary>
        /// <param name="socketBufferSize"></param>
        /// <param name="messageBufferSize"></param>
        public ThriftClient(int socketBufferSize, int messageBufferSize)
            : base(new Protocol.ThriftProtocol(), socketBufferSize, messageBufferSize, 3000, 3000)
        {
        }
        /// <summary>
        /// new
        /// </summary>
        /// <param name="socketBufferSize"></param>
        /// <param name="messageBufferSize"></param>
        /// <param name="millisecondsSendTimeout"></param>
        /// <param name="millisecondsReceiveTimeout"></param>
        public ThriftClient(int socketBufferSize,
            int messageBufferSize,
            int millisecondsSendTimeout,
            int millisecondsReceiveTimeout)
            : base(new Protocol.ThriftProtocol(),
            socketBufferSize,
            messageBufferSize,
            millisecondsSendTimeout,
            millisecondsReceiveTimeout)
        {
        }

        /// <summary>
        /// send
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cmdName"></param>
        /// <param name="seqID"></param>
        /// <param name="payload"></param>
        /// <param name="onException"></param>
        /// <param name="onResult"></param>
        /// <exception cref="ArgumentNullException">payload is null</exception>
        /// <exception cref="ArgumentNullException">onException is null</exception>
        /// <exception cref="ArgumentNullException">onResult is null</exception>
        public void Send(string service, string cmdName, int seqID, byte[] payload,
            Action<Exception> onException, Action<byte[]> onResult)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            if (onException == null) throw new ArgumentNullException("onException");
            if (onResult == null) throw new ArgumentNullException("onResult");

            base.Send(new Request<Messaging.ThriftMessage>(seqID, string.Concat(service, ".", cmdName), payload,
                base.MillisecondsReceiveTimeout, onException, message => onResult(message.Payload)));
        }
    }
}