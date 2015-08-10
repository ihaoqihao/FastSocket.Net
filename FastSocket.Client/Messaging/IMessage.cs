
namespace Sodao.FastSocket.Client.Messaging
{
    /// <summary>
    /// message interface
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// message id
        /// </summary>
        int SeqID { get; }
    }
}