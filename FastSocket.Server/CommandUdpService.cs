using System;
using System.Collections.Generic;
using System.Threading;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// command udp service
    /// </summary>
    /// <typeparam name="TCommandInfo"></typeparam>
    public class CommandUdpService<TCommandInfo> : AbsUdpService<TCommandInfo>
        where TCommandInfo : class, Command.ICommandInfo
    {
        /// <summary>
        /// command dictionary.
        /// </summary>
        private readonly Dictionary<string, Command.IUdpCommand<TCommandInfo>> _dicCommand =
            new Dictionary<string, Command.IUdpCommand<TCommandInfo>>();

        /// <summary>
        /// new
        /// </summary>
        public CommandUdpService()
        {
            //通过反射加载命令
            var assembly = this.GetType().Assembly;
            var commands = SocketBase.Utils.ReflectionHelper.GetImplementObjects<Command.IUdpCommand<TCommandInfo>>(assembly);
            if (commands != null && commands.Length > 0)
            {
                foreach (var cmd in commands) this.AddCommand(cmd);
            }
        }

        /// <summary>
        /// on received
        /// </summary>
        /// <param name="session"></param>
        /// <param name="cmdInfo"></param>
        public override void OnReceived(UdpSession session, TCommandInfo cmdInfo)
        {
            if (string.IsNullOrEmpty(cmdInfo.CmdName)) return;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Command.IUdpCommand<TCommandInfo> cmd = null;
                this._dicCommand.TryGetValue(cmdInfo.CmdName, out cmd);
                try
                {
                    if (cmd == null) this.HandleUnKnowCommand(session, cmdInfo);
                    else cmd.ExecuteCommand(session, cmdInfo);
                }
                catch (Exception ex)
                {
                    this.OnCommandExecException(session, cmdInfo, ex);
                }
            });
        }

        /// <summary>
        /// add command.
        /// </summary>
        /// <param name="cmd"></param>
        /// <exception cref="ArgumentNullException">cmd is null</exception>
        /// <exception cref="ArgumentNullException">cmd.Name is null</exception>
        protected void AddCommand(Command.IUdpCommand<TCommandInfo> cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (string.IsNullOrEmpty(cmd.Name)) throw new ArgumentNullException("cmd.name");

            this._dicCommand[cmd.Name] = cmd;
        }
        /// <summary>
        /// on command exec exception
        /// </summary>
        /// <param name="session"></param>
        /// <param name="cmdInfo"></param>
        /// <param name="ex"></param>
        protected virtual void OnCommandExecException(UdpSession session, TCommandInfo cmdInfo, Exception ex)
        {
        }
        /// <summary>
        /// handle unknow command.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="commandInfo"></param>
        protected virtual void HandleUnKnowCommand(UdpSession session, TCommandInfo commandInfo)
        {
        }
    }
}