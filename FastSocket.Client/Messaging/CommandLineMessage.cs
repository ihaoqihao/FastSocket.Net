using System;

namespace Sodao.FastSocket.Client.Messaging
{
    /// <summary>
    /// command line message.
    /// </summary>
    public class CommandLineMessage : Messaging.IMessage
    {
        /// <summary>
        /// get the current command name.
        /// </summary>
        public readonly string CmdName;
        /// <summary>
        /// 参数
        /// </summary>
        public readonly string[] Parameters;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="seqId"></param>
        /// <param name="cmdName"></param>
        /// <param name="parameters"></param>
        public CommandLineMessage(int seqId, string cmdName, params string[] parameters)
        {
            if (cmdName == null) throw new ArgumentNullException("cmdName");

            this.SeqId = seqId;
            this.CmdName = cmdName;
            this.Parameters = parameters;
        }

        /// <summary>
        /// get seqId
        /// </summary>
        public int SeqId { get; private set; }
    }
}