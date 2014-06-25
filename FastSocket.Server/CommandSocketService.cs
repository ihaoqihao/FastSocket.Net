using System;
using System.Collections.Generic;
using System.Threading;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// socket service for command.
    /// </summary>
    /// <typeparam name="TCommandInfo"></typeparam>
    public class CommandSocketService<TCommandInfo> : AbsSocketService<TCommandInfo>
        where TCommandInfo : class, Command.ICommandInfo
    {
        #region Private Members
        /// <summary>
        /// command dictionary.
        /// </summary>
        private readonly Dictionary<string, Command.ICommand<TCommandInfo>> _dicCommand =
            new Dictionary<string, Command.ICommand<TCommandInfo>>();
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        public CommandSocketService()
        {
            //通过反射加载命令
            var assembly = this.GetType().Assembly;
            var commands = SocketBase.Utils.ReflectionHelper.GetImplementObjects<Command.ICommand<TCommandInfo>>(assembly);
            if (commands != null && commands.Length > 0)
            {
                foreach (var cmd in commands) this.AddCommand(cmd);
            }
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// 当建立socket连接时，会调用此方法
        /// </summary>
        /// <param name="connection"></param>
        public override void OnConnected(SocketBase.IConnection connection)
        {
            //开始异步接收数据.
            connection.BeginReceive();
        }
        /// <summary>
        /// 当接收到客户端新消息时，会调用此方法.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="cmdInfo"></param>
        public override void OnReceived(SocketBase.IConnection connection, TCommandInfo cmdInfo)
        {
            if (string.IsNullOrEmpty(cmdInfo.CmdName)) return;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Command.ICommand<TCommandInfo> cmd = null;
                this._dicCommand.TryGetValue(cmdInfo.CmdName, out cmd);
                try
                {
                    if (cmd == null) this.HandleUnKnowCommand(connection, cmdInfo);
                    else cmd.ExecuteCommand(connection, cmdInfo);
                }
                catch (Exception ex)
                {
                    this.OnCommandExecException(connection, cmdInfo, ex);
                }
            });
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// add command.
        /// </summary>
        /// <param name="cmd"></param>
        /// <exception cref="ArgumentNullException">cmd is null</exception>
        /// <exception cref="ArgumentNullException">cmd.Name is null</exception>
        protected void AddCommand(Command.ICommand<TCommandInfo> cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (string.IsNullOrEmpty(cmd.Name)) throw new ArgumentNullException("cmd.name");

            this._dicCommand[cmd.Name] = cmd;
        }
        /// <summary>
        /// on command exec exception
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="cmdInfo"></param>
        /// <param name="ex"></param>
        protected virtual void OnCommandExecException(SocketBase.IConnection connection, TCommandInfo cmdInfo, Exception ex)
        {

        }
        /// <summary>
        /// handle unknow command.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandInfo"></param>
        protected virtual void HandleUnKnowCommand(SocketBase.IConnection connection, TCommandInfo commandInfo)
        {
        }
        #endregion
    }
}