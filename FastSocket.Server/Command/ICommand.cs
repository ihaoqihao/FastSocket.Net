
namespace Sodao.FastSocket.Server.Command
{
    /// <summary>
    /// command interface.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// get the command name.
        /// </summary>
        string Name { get; }
    }

    /// <summary>
    /// command interfce.
    /// </summary>
    /// <typeparam name="TCommandInfo"></typeparam>
    public interface ICommand<TCommandInfo> : ICommand where TCommandInfo : ICommandInfo
    {
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandInfo"></param>
        void ExecuteCommand(SocketBase.IConnection connection, TCommandInfo commandInfo);
    }

    /// <summary>
    /// udp command interface
    /// </summary>
    /// <typeparam name="TCommandInfo"></typeparam>
    public interface IUdpCommand<TCommandInfo> : ICommand where TCommandInfo : ICommandInfo
    {
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="session"></param>
        /// <param name="commandInfo"></param>
        void ExecuteCommand(UdpSession session, TCommandInfo commandInfo);
    }
}