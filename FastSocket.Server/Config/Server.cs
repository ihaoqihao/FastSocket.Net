using System.Configuration;

namespace Sodao.FastSocket.Server.Config
{
    /// <summary>
    /// server
    /// </summary>
    public class Server : ConfigurationElement
    {
        /// <summary>
        /// 名称
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
        }
        /// <summary>
        /// 端口号。
        /// </summary>
        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return (int)this["port"]; }
        }

        /// <summary>
        /// Socket Buffer Size
        /// 默认8192 bytes
        /// </summary>
        [ConfigurationProperty("socketBufferSize", IsRequired = false, DefaultValue = 8192)]
        public int SocketBufferSize
        {
            get { return (int)this["socketBufferSize"]; }
        }
        /// <summary>
        /// Message Buffer Size
        /// 默认1024 bytes
        /// </summary>
        [ConfigurationProperty("messageBufferSize", IsRequired = false, DefaultValue = 8192)]
        public int MessageBufferSize
        {
            get { return (int)this["messageBufferSize"]; }
        }
        /// <summary>
        /// max message size,
        /// 默认4MB
        /// </summary>
        [ConfigurationProperty("maxMessageSize", IsRequired = false, DefaultValue = 1024 * 1024 * 4)]
        public int MaxMessageSize
        {
            get { return (int)this["maxMessageSize"]; }
        }
        /// <summary>
        /// 最大连接数，默认2W
        /// </summary>
        [ConfigurationProperty("maxConnections", IsRequired = false, DefaultValue = 20000)]
        public int MaxConnections
        {
            get { return (int)this["maxConnections"]; }
        }

        /// <summary>
        /// ServiceType
        /// </summary>
        [ConfigurationProperty("serviceType", IsRequired = true)]
        public string ServiceType
        {
            get { return (string)this["serviceType"]; }
        }
        /// <summary>
        /// 协议, 默认命令行协议
        /// </summary>
        [ConfigurationProperty("protocol", IsRequired = false, DefaultValue = "commandLine")]
        public string Protocol
        {
            get { return (string)this["protocol"]; }
        }
    }
}