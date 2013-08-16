
namespace Sodao.FastSocket.Server.Command
{
    /// <summary>
    /// thrift command info.
    /// </summary>
    public sealed class ThriftCommandInfo : ICommandInfo
    {
        /// <summary>
        /// new
        /// </summary>
        /// <param name="buffer"></param>
        public ThriftCommandInfo(byte[] buffer)
        {
            this.Buffer = buffer;
        }

        /// <summary>
        /// get the current command name.
        /// </summary>
        public string CmdName
        {
            get { return null; }
        }
        /// <summary>
        /// buffer
        /// </summary>
        public byte[] Buffer
        {
            get;
            private set;
        }        
    }
}