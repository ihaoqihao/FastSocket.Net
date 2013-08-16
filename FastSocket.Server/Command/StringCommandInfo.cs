using System;

namespace Sodao.FastSocket.Server.Command
{
    /// <summary>
    /// string command info.
    /// </summary>
    public class StringCommandInfo : ICommandInfo
    {
        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="cmdName"></param>
        /// <param name="parameters"></param>
        /// <exception cref="ArgumentNullException">cmdName is null or empty</exception>
        public StringCommandInfo(string cmdName, params string[] parameters)
        {
            if (string.IsNullOrEmpty(cmdName)) throw new ArgumentNullException("cmdName");

            this.CmdName = cmdName;
            this.Parameters = parameters;
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
        /// 参数
        /// </summary>
        public string[] Parameters
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
        /// <param name="value"></param>
        public void Reply(SocketBase.IConnection connection, string value)
        {
            connection.BeginSend(PacketBuilder.ToCommandLine(value));
        }
        #endregion
    }
}