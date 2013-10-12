using System.Threading;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// use thread pool schedule.
    /// </summary>
    public sealed class ThreadPoolScheduler : IScheduler
    {
        /// <summary>
        /// Queues a method for execution.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public void Post(WaitCallback callback, object state = null)
        {
            ThreadPool.QueueUserWorkItem(callback, state);
        }
    }
}