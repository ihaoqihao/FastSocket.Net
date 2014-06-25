using System;
using Sodao.FastSocket.Client.Response;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// thrift client
    /// </summary>
    public class ThriftClient : PooledSocketClient<ThriftResponse>
    {
        #region Constructors
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
        #endregion

        #region Public Methods
        /// <summary>
        /// send
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cmdName"></param>
        /// <param name="seqID"></param>
        /// <param name="payload"></param>
        /// <param name="onException"></param>
        /// <param name="onResult"></param>
        public void Send(string service, string cmdName, int seqID, byte[] payload,
            Action<Exception> onException, Action<byte[]> onResult)
        {
            this.Send(null, service, cmdName, seqID, payload, onException, onResult);
        }
        /// <summary>
        /// sned
        /// </summary>
        /// <param name="consistentKey"></param>
        /// <param name="service"></param>
        /// <param name="cmdName"></param>
        /// <param name="seqID"></param>
        /// <param name="payload"></param>
        /// <param name="onException"></param>
        /// <param name="onResult"></param>
        /// <exception cref="ArgumentNullException">payload is null or empty</exception>
        /// <exception cref="ArgumentNullException">onException is null</exception>
        /// <exception cref="ArgumentNullException">onResult is null</exception>
        public void Send(byte[] consistentKey, string service, string cmdName, int seqID, byte[] payload,
            Action<Exception> onException, Action<byte[]> onResult)
        {
            if (payload == null || payload.Length == 0) throw new ArgumentNullException("payload");
            if (onException == null) throw new ArgumentNullException("onException");
            if (onResult == null) throw new ArgumentNullException("onResult");

            base.Send(new Request<Response.ThriftResponse>(consistentKey, seqID, string.Concat(service, ".", cmdName), payload,
                base.MillisecondsReceiveTimeout, onException, response => onResult(response.Buffer)));
        }
        #endregion
    }
}