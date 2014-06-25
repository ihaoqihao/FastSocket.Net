using System;
using System.Text;
using System.Threading.Tasks;
using Sodao.FastSocket.Client.Response;
using Sodao.FastSocket.SocketBase.Utils;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// 异步socket客户端
    /// </summary>
    public class AsyncBinarySocketClient : PooledSocketClient<AsyncBinaryResponse>
    {
        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        public AsyncBinarySocketClient()
            : base(new Protocol.AsyncBinaryProtocol())
        {
        }
        /// <summary>
        /// new
        /// </summary>
        /// <param name="socketBufferSize"></param>
        /// <param name="messageBufferSize"></param>
        public AsyncBinarySocketClient(int socketBufferSize, int messageBufferSize)
            : base(new Protocol.AsyncBinaryProtocol(), socketBufferSize, messageBufferSize, 3000, 3000)
        {
        }
        /// <summary>
        /// new
        /// </summary>
        /// <param name="socketBufferSize"></param>
        /// <param name="messageBufferSize"></param>
        /// <param name="millisecondsSendTimeout"></param>
        /// <param name="millisecondsReceiveTimeout"></param>
        public AsyncBinarySocketClient(int socketBufferSize,
            int messageBufferSize,
            int millisecondsSendTimeout,
            int millisecondsReceiveTimeout)
            : base(new Protocol.AsyncBinaryProtocol(),
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
        /// <typeparam name="TResult"></typeparam>
        /// <param name="cmdName"></param>
        /// <param name="payload"></param>
        /// <param name="funcResultFactory"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        public Task<TResult> Send<TResult>(string cmdName, byte[] payload,
            Func<AsyncBinaryResponse, TResult> funcResultFactory, object asyncState = null)
        {
            return this.Send(null, cmdName, payload, funcResultFactory, asyncState);
        }
        /// <summary>
        /// new
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="consistentKey"></param>
        /// <param name="cmdName"></param>
        /// <param name="payload"></param>
        /// <param name="funcResultFactory"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">cmdName is null or empty.</exception>
        /// <exception cref="ArgumentNullException">funcResultFactory is null.</exception>
        public Task<TResult> Send<TResult>(byte[] consistentKey, string cmdName, byte[] payload,
            Func<AsyncBinaryResponse, TResult> funcResultFactory, object asyncState = null)
        {
            if (string.IsNullOrEmpty(cmdName)) throw new ArgumentNullException("cmdName");
            if (funcResultFactory == null) throw new ArgumentNullException("funcResultFactory");

            var seqID = base.NextRequestSeqID();
            var cmdLength = cmdName.Length;
            var messageLength = (payload == null ? 0 : payload.Length) + cmdLength + 6;
            var sendBuffer = new byte[messageLength + 4];

            //write message length
            Buffer.BlockCopy(NetworkBitConverter.GetBytes(messageLength), 0, sendBuffer, 0, 4);
            //write seqID.
            Buffer.BlockCopy(NetworkBitConverter.GetBytes(seqID), 0, sendBuffer, 4, 4);
            //write response flag length.
            Buffer.BlockCopy(NetworkBitConverter.GetBytes((short)cmdLength), 0, sendBuffer, 8, 2);
            //write response flag
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(cmdName), 0, sendBuffer, 10, cmdLength);
            //write body buffer
            if (payload != null && payload.Length > 0)
                Buffer.BlockCopy(payload, 0, sendBuffer, 10 + cmdLength, payload.Length);

            var source = new TaskCompletionSource<TResult>(asyncState);
            base.Send(new Request<Response.AsyncBinaryResponse>(consistentKey, seqID, cmdName, sendBuffer, base.MillisecondsReceiveTimeout,
                ex => source.TrySetException(ex), response =>
                {
                    TResult result;
                    try { result = funcResultFactory(response); }
                    catch (Exception ex) { source.TrySetException(ex); return; }
                    source.TrySetResult(result);
                }));
            return source.Task;
        }
        #endregion
    }
}