
namespace Sodao.FastSocket.SocketBase
{
    /// <summary>
    /// packet send status
    /// </summary>
    public enum SendStatus : byte
    {
        /// <summary>
        /// 发送成功
        /// </summary>
        Success = 1,
        /// <summary>
        /// 发送失败
        /// </summary>
        Failed = 2
    }
}