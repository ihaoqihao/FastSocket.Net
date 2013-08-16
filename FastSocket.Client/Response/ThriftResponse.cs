
namespace Sodao.FastSocket.Client.Response
{
    /// <summary>
    /// thrift response.
    /// </summary>
    public class ThriftResponse : IResponse
    {
        private int _seqID;
        /// <summary>
        /// buffer
        /// </summary>
        public readonly byte[] Buffer = null;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="seqID"></param>
        /// <param name="buffer"></param>
        public ThriftResponse(int seqID, byte[] buffer)
        {
            this._seqID = seqID;
            this.Buffer = buffer;
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