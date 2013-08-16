using System;
using System.Threading;

namespace Sodao.FastSocket.SocketBase.Utils
{
    /// <summary>
    /// a non-locking queue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class InterlockedQueue<T>
    {
        #region Private Members
        private Node headNode, tailNode;
        private int _count;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:InterlockedQueue"/> class.
        /// </summary>
        public InterlockedQueue()
        {
            Node node = new Node(default(T));

            this.headNode = node;
            this.tailNode = node;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Removes and returns the item at the beginning of the <see cref="T:InterlockedQueue"/>.
        /// </summary>
        /// <param name="value">The object that is removed from the beginning of the <see cref="T:InterlockedQueue"/>.</param>
        /// <returns><value>true</value> if an item was successfully dequeued; otherwise <value>false</value>.</returns>
        public bool Dequeue(out T value)
        {
            Node head, tail, next;
            while (true)
            {
                // read head
                head = this.headNode;
                tail = this.tailNode;
                next = head.Next;

                // Are head, tail, and next consistent?
                if (Object.ReferenceEquals(this.headNode, head))
                {
                    // is tail falling behind
                    if (Object.ReferenceEquals(head, tail))
                    {
                        // is the queue empty?
                        if (Object.ReferenceEquals(next, null))
                        {
                            value = default(T);
                            // queue is empty and cannot dequeue
                            return false;
                        }

                        Interlocked.CompareExchange<Node>(ref this.tailNode, next, tail);
                    }
                    else // No need to deal with tail
                    {
                        // read value before CAS otherwise another deque might try to free the next node
                        value = next.Value;
                        // try to swing the head to the next node
                        if (Interlocked.CompareExchange<Node>(ref this.headNode, next, head) == head)
                        {
                            Interlocked.Decrement(ref this._count);
                            return true;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// peek
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Peek(out T value)
        {
            Node head, tail, next;
            while (true)
            {
                // read head
                head = this.headNode;
                tail = this.tailNode;
                next = head.Next;

                // Are head, tail, and next consistent?
                if (Object.ReferenceEquals(this.headNode, head))
                {
                    // is tail falling behind
                    if (Object.ReferenceEquals(head, tail))
                    {
                        // is the queue empty?
                        if (Object.ReferenceEquals(next, null))
                        {
                            value = default(T);
                            // queue is empty
                            return false;
                        }

                        Interlocked.CompareExchange<Node>(ref this.tailNode, next, tail);
                    }
                    else // No need to deal with tail
                    {
                        // read value before CAS otherwise another deque might try to free the next node
                        value = next.Value;
                        return true;
                    }
                }
            }
        }
        /// <summary>
        /// Adds an object to the end of the <see cref="T:InterlockedQueue"/>.
        /// </summary>
        /// <param name="value">The item to be added to the <see cref="T:InterlockedQueue"/>. The value can be <value>null</value>.</param>
        public void Enqueue(T value)
        {
            // Allocate a new node from the free list
            Node valueNode = new Node(value);
            while (true)
            {
                Node tail = this.tailNode;
                Node next = tail.Next;

                // are tail and next consistent
                if (Object.ReferenceEquals(tail, this.tailNode))
                {
                    // was tail pointing to the last node?
                    if (Object.ReferenceEquals(next, null))
                    {
                        if (Object.ReferenceEquals(Interlocked.CompareExchange(ref tail.Next, valueNode, next), next))
                        {
                            Interlocked.CompareExchange(ref this.tailNode, valueNode, tail);
                            break;
                        }
                    }
                    else // tail was not pointing to last node
                    {
                        // try to swing Tail to the next node
                        Interlocked.CompareExchange<Node>(ref this.tailNode, next, tail);
                    }
                }
            }
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