
namespace Sodao.FastSocket.Client.Messaging
{
    /// <summary>
    /// thrift message.
    /// </summary>
    public class ThriftMessage : IMessage
    {
        /// <summary>
        /// seqID
        /// </summary>
        private readonly int _seqID;
        /// <summary>
        /// payload
        /// </summary>
        public readonly byte[] Payload = null;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="seqID"></param>
        /// <param name="buffer"></param>
        public ThriftMessage(int seqID, byte[] buffer)
        {
            this._seqID = seqID;
            this.Payload = buffer;
        }

        /// <summary>
        /// get seqID
        /// </summary>
        public int SeqID
        {
            get { return this._seqID; }
        }
    }
}