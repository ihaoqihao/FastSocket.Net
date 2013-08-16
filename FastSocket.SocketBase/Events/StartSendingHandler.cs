
namespace Sodao.FastSocket.SocketBase
{
    /// <summary>
    /// begin send handler
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="packet"></param>
    public delegate void StartSendingHandler(IConnection connection, Packet packet);
}