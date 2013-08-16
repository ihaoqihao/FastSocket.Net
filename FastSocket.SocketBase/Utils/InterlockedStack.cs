using System.Threading;

namespace Sodao.FastSocket.SocketBase.Utils
{
    /// <summary>
    /// non-locking stack.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InterlockedStack<T>
    {
        #region Private Members
        private readonly Node head;
        private int _count;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        public InterlockedStack()
        {
            this.head = new Node(default(T));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// push
        /// </summary>
        /// <param name="item"></param>
        public void Push(T item)
        {
            var node = new Node(item);
            do { node.Next = this.head.Next; }
            while (Interlocked.CompareExchange(ref this.head.Next, node, node.Next) != node.Next);

            Interlocked.Increment(ref this._count);
        }
        /// <summary>
        /// try pop
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryPop(out T value)
        {
            value = default(T);
            Node node;
            do
            {
                node = head.Next;
                if (node == null) return false;
            }
            while (Interlocked.CompareExchange(ref head.Next, node.Next, node) != node);

            value = node.Value;
            Interlocked.Decrement(ref this._count);
            return true;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// get count
        /// </summary>
        public int Count
        {
            get { return Thread.VolatileRead(ref this._count); }
        }
        #endregion

        #region Private Class
        /// <summary>
        /// node
        /// </summary>
        private class Node
        {
            /// <summary>
            /// value
            /// </summary>
            public readonly T Value;
            /// <summary>
            /// next
            /// </summary>
            public Node Next;

            /// <summary>
            /// new
            /// </summary>
            /// <param name="value"></param>
            public Node(T value)
            {
                this.Value = value;
            }
        }
        #endregion
    }
}