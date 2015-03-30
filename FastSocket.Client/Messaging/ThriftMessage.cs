
namespace Sodao.FastSocket.Client.Messaging
{
    /// <summary>
    /// thrift message.
    /// </summary>
    public class ThriftMessage : IMessage
    {
        /// <summary>
        /// seqId
        /// </summary>
        private readonly int _seqId;
        /// <summary>
        /// payload
        /// </summary>
        public readonly byte[] Payload = null;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="seqId"></param>
        /// <param name="buffer"></param>
        public ThriftMessage(int seqId, byte[] buffer)
        {
            this._seqId = seqId;
            this.Payload = buffer;
        }

        /// <summary>
        /// get seqID
        /// </summary>
        public int SeqId
        {
            get { return this._seqId; }
        }
    }
}