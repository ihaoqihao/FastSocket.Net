using System;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// send context
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public sealed class SendContext<TMessage> where TMessage : class, Messaging.IMessage
    {
        private readonly bool _allowRetry;
        private readonly SocketBase.IConnection _connection = null;
        private readonly SocketClient<TMessage> _client = null;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="client"></param>
        /// <param name="allowRetry"></param>
        /// <exception cref="ArgumentNullException">connection is null.</exception>
        /// <exception cref="ArgumentNullException">client is null.</exception>
        public SendContext(SocketBase.IConnection connection,
            SocketClient<TMessage> client,
            bool allowRetry = true)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (client == null) throw new ArgumentNullException("client");

            this._connection = connection;
            this._client = client;
            this._allowRetry = allowRetry;
        }

        /// <summary>
        /// new request
        /// </summary>
        /// <param name="name"></param>
        /// <param name="payload"></param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <param name="onException"></param>
        /// <param name="onResult"></param>
        /// <returns></returns>
        public Request<TMessage> NewRequest(string name,
            byte[] payload,
            int millisecondsReceiveTimeout,
            Action<Exception> onException,
            Action<TMessage> onResult)
        {
            return this._client.NewRequest(name, payload, millisecondsReceiveTimeout,
                onException, onResult);
        }
        /// <summary>
        /// send
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="ArgumentNullException">request is null.</exception>
        public void Send(Request<TMessage> request)
        {
            if (request == null) throw new ArgumentNullException("request");

            request.AllowRetry = this._allowRetry;
            this._connection.BeginSend(request);
        }
    }
}