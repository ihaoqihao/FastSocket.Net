using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// socket connector
    /// </summary>
    static public class SocketConnector
    {
        /// <summary>
        /// begin connect
        /// </summary>
        /// <param name="endPoint"></param>
        /// <exception cref="ArgumentNullException">endPoint is null</exception>
        static public Task<Socket> Connect(EndPoint endPoint)
        {
            if (endPoint == null) throw new ArgumentNullException("endPoint");

            var source = new TaskCompletionSource<Socket>();
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            var e = new SocketAsyncEventArgs();
            e.UserToken = new Tuple<TaskCompletionSource<Socket>, Socket>(source, socket);
            e.RemoteEndPoint = endPoint;
            e.Completed += OnCompleted;

            bool completed = true;
            try { completed = socket.ConnectAsync(e); }
            catch (Exception ex) { source.TrySetException(ex); }
            if (!completed) ThreadPool.QueueUserWorkItem(_ => OnCompleted(null, e));

            return source.Task;
        }
        /// <summary>
        /// connect completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static private void OnCompleted(object sender, SocketAsyncEventArgs e)
        {
            var t = e.UserToken as Tuple<TaskCompletionSource<Socket>, Socket>;
            var source = t.Item1;
            var socket = t.Item2;
            var error = e.SocketError;

            e.UserToken = null;
            e.Completed -= OnCompleted;
            e.Dispose();

            if (error != SocketError.Success)
            {
                socket.Close();
                source.TrySetException(new SocketException((int)error));
                return;
            }

            source.TrySetResult(socket);
        }
    }
}