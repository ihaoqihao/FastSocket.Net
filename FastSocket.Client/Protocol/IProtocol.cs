using System;

namespace Sodao.FastSocket.Client.Protocol
{
    /// <summary>
    /// 协议接口
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public interface IProtocol<TResponse> where TResponse : Response.IResponse
    {
        /// <summary>
        /// Find Response
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        TResponse FindResponse(SocketBase.IConnection connection, ArraySegment<byte> buffer, out int readlength);
    }
}