using System;
using System.Collections.Generic;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// socket service for command.
    /// </summary>
    /// <typeparam name="TCommandInfo"></typeparam>
    public abstract class CommandSocketService<TCommandInfo> : ISocketService<TCommandInfo>
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

        #region ISocketService Methods
        /// <summary>
        /// 当建立socket连接时，会调用此方法
        /// </summary>
        /// <param name="connection"></param>
        public virtual void OnConnected(SocketBase.IConnection connection)
        {
            //开始异步接收数据.
            connection.BeginReceive();
        }
        /// <summary>
        /// start sending
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        public void OnStartSending(SocketBase.IConnection connection, SocketBase.Packet packet)
        {
        }
        /// <summary>
        /// send callback
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        /// <param name="status"></param>
        public virtual void OnSendCallback(SocketBase.IConnection connection, SocketBase.Packet packet, SocketBase.SendStatus status)
        {
        }
        /// <summary>
        /// 当接收到客户端新消息时，会调用此方法.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="cmdInfo"></param>
        public virtual void OnReceived(SocketBase.IConnection connection, TCommandInfo cmdInfo)
        {
            if (connection == null || cmdInfo == null || string.IsNullOrEmpty(cmdInfo.CmdName)) return;

            Command.ICommand<TCommandInfo> cmd = null;
            if (this._dicCommand.TryGetValue(cmdInfo.CmdName, out cmd)) cmd.ExecuteCommand(connection, cmdInfo);
            else this.HandleUnKnowCommand(connection, cmdInfo);
        }
        /// <summary>
        /// OnDisconnected
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        public virtual void OnDisconnected(SocketBase.IConnection connection, Exception ex)
        {
        }
        /// <summary>
        /// OnException
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        public virtual void OnException(SocketBase.IConnection connection, Exception ex)
        {
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// add command.
        /// </summary>
        /// <param name="cmd"></param>
        /// <exception cref="ArgumentNullException">cmd is null</exception>
        /// <exception cref="ArgumentNullException">cmd.Name is null</exception>
        protected virtual void AddCommand(Command.ICommand<TCommandInfo> cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (string.IsNullOrEmpty(cmd.Name)) throw new ArgumentNullException("cmd.name");
            this._dicCommand[cmd.Name] = cmd;
        }
        /// <summary>
        /// handle unknow command.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandInfo"></param>
        protected abstract void HandleUnKnowCommand(SocketBase.IConnection connection, TCommandInfo commandInfo);
        #endregion
    }
}