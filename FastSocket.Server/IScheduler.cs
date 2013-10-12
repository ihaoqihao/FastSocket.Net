using System.Threading;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// schedule interface
    /// </summary>
    public interface IScheduler
    {
        /// <summary>
        /// Queues a method for execution.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        void Post(WaitCallback callback, object state = null);
    }
}