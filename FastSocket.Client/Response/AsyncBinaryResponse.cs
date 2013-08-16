
namespace Sodao.FastSocket.Client.Response
{
    /// <summary>
    /// async binary response
    /// </summary>
    public class AsyncBinaryResponse : IResponse
    {
        private int _seqID;
        /// <summary>
        /// flag
        /// </summary>
        public readonly string Flag = null;
        /// <summary>
        /// buffer
        /// </summary>
        public readonly byte[] Buffer = null;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="seqID"></param>
        /// <param name="buffer"></param>
        public AsyncBinaryResponse(string flag, int seqID, byte[] buffer)
        {
            this._seqID = seqID;
            this.Flag = flag;
            this.Buffer = buffer;
        }

        /// <summary>
        /// seqID
        /// </summary>
        public int SeqID
        {
            get { return this._seqID; }
        }
    }
}