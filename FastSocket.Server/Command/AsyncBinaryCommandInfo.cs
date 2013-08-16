using System;

namespace Sodao.FastSocket.Server.Command
{
    /// <summary>
    /// async binary command info.
    /// </summary>
    public class AsyncBinaryCommandInfo : ICommandInfo
    {
        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="cmdName"></param>
        /// <param name="seqID"></param>
        /// <param name="buffer"></param>
        /// <exception cref="ArgumentNullException">cmdName is null or empty.</exception>
        public AsyncBinaryCommandInfo(string cmdName, int seqID, byte[] buffer)
        {
            if (string.IsNullOrEmpty(cmdName)) throw new ArgumentNullException("cmdName");

            this.CmdName = cmdName;
            this.SeqID = seqID;
            this.Buffer = buffer;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// get the current command name.
        /// </summary>
        public string CmdName
        {
            get;
            private set;
        }
        /// <summary>
        /// seq id.
        /// </summary>
        public int SeqID
        {
            get;
            private set;
        }
        /// <summary>
        /// 主体内容
        /// </summary>
        public byte[] Buffer
        {
            get;
            private set;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// reply
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="payload"></param>
        public void Reply(SocketBase.IConnection connection, byte[] payload)
        {
            var packet = PacketBuilder.ToAsyncBinary(this.CmdName, this.SeqID, payload);
            connection.BeginSend(packet);
        }
        #endregion
    }
}