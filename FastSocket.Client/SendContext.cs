using System;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// send context
    /// </summary>
    public sealed class SendContext
    {
        private readonly bool _allowRetry;
        private readonly SocketBase.IConnection _connection = null;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="allowRetry"></param>
        /// <exception cref="ArgumentNullException">connection is null.</exception>
        /// <exception cref="ArgumentNullException">client is null.</exception>
        public SendContext(SocketBase.IConnection connection, bool allowRetry = true)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            this._connection = connection;
            this._allowRetry = allowRetry;
        }

        /// <summary>
        /// send
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="request"></param>
        /// <exception cref="ArgumentNullException">request is null.</exception>
        public void Send<TMessage>(Request<TMessage> request) where TMessage : Messaging.IMessage
        {
            if (request == null) throw new ArgumentNullException("request");

            request.AllowRetry = this._allowRetry;
            this._connection.BeginSend(request);
        }
    }
}