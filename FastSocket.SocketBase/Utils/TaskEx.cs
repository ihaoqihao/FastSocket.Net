using System;
using System.Threading;

namespace Sodao.FastSocket.SocketBase.Utils
{
    /// <summary>
    /// task ex
    /// </summary>
    static public class TaskEx
    {
        /// <summary>
        /// 延迟执行某个动作
        /// </summary>
        /// <param name="dueTime"></param>
        /// <param name="callback"></param>
        /// <exception cref="ArgumentOutOfRangeException">dueTime</exception>
        /// <exception cref="ArgumentNullException">callback is null</exception>
        public static void Delay(int dueTime, Action callback)
        {
            if (dueTime < -1) throw new ArgumentOutOfRangeException("dueTime");
            if (callback == null) throw new ArgumentNullException("callback");

            Timer timer = null;
            timer = new Timer(_ =>
            {
                try { callback(); }
                catch (Exception ex) { Log.Trace.Error(ex.Message, ex); }
                finally { timer.Dispose(); }
            }, null, dueTime, Timeout.Infinite);
        }
    }
}