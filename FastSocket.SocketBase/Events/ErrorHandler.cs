using System;

namespace Sodao.FastSocket.SocketBase
{
    /// <summary>
    /// error delegate
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="ex"></param>
    public delegate void ErrorHandler(IConnection connection, Exception ex);
}