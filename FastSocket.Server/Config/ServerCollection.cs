using System.Configuration;

namespace Sodao.FastSocket.Server.Config
{
    /// <summary>
    /// 服务器集合。
    /// </summary>
    [ConfigurationCollection(typeof(Server), AddItemName = "server")]
    public class ServerCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// 创建新元素。
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new Server();
        }
        /// <summary>
        /// 获取指定元素的Key。
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            var server = element as Server;
            return server.Name;
        }
        /// <summary>
        /// 获取指定位置的对象。
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Server this[int i]
        {
            get { return BaseGet(i) as Server; }
        }
    }
}