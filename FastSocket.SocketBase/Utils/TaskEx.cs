using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sodao.FastSocket.SocketBase.Utils
{
    /// <summary>
    /// task ex
    /// </summary>
    static public class TaskEx
    {
        /// <summary>
        /// delay
        /// </summary>
        /// <param name="dueTime"></param>
        static public Task Delay(int dueTime)
        {
            if (dueTime < -1) throw new ArgumentOutOfRangeException("dueTime");

            Timer timer = null;
            var source = new TaskCompletionSource<bool>();
            timer = new Timer(_ =>
            {
                using (timer) source.TrySetResult(true);
            }, null, dueTime, Timeout.Infinite);

            return source.Task;
        }
    }
}