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
        /// <param name="seqID"></param>
        /// <param name="cmdName"></param>
        /// <param name="parameters"></param>
        /// <exception cref="ArgumentNullException">cmdName is null.</exception>
        public CommandLineMessage(int seqID, string cmdName, params string[] parameters)
        {
            if (cmdName == null) throw new ArgumentNullException("cmdName");

            this.SeqID = seqID;
            this.CmdName = cmdName;
            this.Parameters = parameters;
        }

        /// <summary>
        /// get seqID
        /// </summary>
        public int SeqID { get; private set; }
    }
}