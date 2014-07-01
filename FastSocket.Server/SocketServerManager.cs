using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// Socket server manager.
    /// </summary>
    public class SocketServerManager
    {
        #region Private Members
        /// <summary>
        /// host list
        /// </summary>
        static private readonly List<SocketBase.IHost> _listHosts =
            new List<SocketBase.IHost>();
        /// <summary>
        /// key:server name.
        /// </summary>
        static private readonly Dictionary<string, SocketBase.IHost> _dicHosts =
            new Dictionary<string, SocketBase.IHost>();
        #endregion

        #region Static Methods
        /// <summary>
        /// 初始化Socket Server
        /// </summary>
        static public void Init()
        {
            Init("socketServer");
        }
        /// <summary>
        /// 初始化Socket Server
        /// </summary>
        /// <param name="sectionName"></param>
        static public void Init(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName)) throw new ArgumentNullException("sectionName");
            Init(ConfigurationManager.GetSection(sectionName) as Config.SocketServerConfig);
        }
        /// <summary>
        /// 初始化Socket Server
        /// </summary>
        /// <param name="config"></param>
        static public void Init(Config.SocketServerConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (config.Servers == null) return;

            foreach (Config.Server serverConfig in config.Servers)
            {
                //inti protocol
                var objProtocol = GetProtocol(serverConfig.Protocol);
                if (objProtocol == null) throw new InvalidOperationException("protocol");

                //init custom service
                var tService = Type.GetType(serverConfig.ServiceType, false);
                if (tService == null) throw new InvalidOperationException("serviceType");

                var tAbsService = tService;
                while (true)
                {
                    tAbsService = tAbsService.BaseType;
                    if (tAbsService.Name == typeof(AbsSocketService<>).Name) break;
                }

                var objService = Activator.CreateInstance(tService);
                if (objService == null) throw new InvalidOperationException("serviceType");

                //init host.
                var host = Activator.CreateInstance(typeof(SocketServer<>).MakeGenericType(
                    tAbsService.GetGenericArguments()),
                    objService,
                    objProtocol,
                    serverConfig.SocketBufferSize,
                    serverConfig.MessageBufferSize,
                    serverConfig.MaxMessageSize,
                    serverConfig.MaxConnections) as AbsSocketServer;

                host.AddListener(serverConfig.Name, new IPEndPoint(IPAddress.Any, serverConfig.Port));

                _listHosts.Add(host);
                _dicHosts[serverConfig.Name] = host;
            }
        }
        /// <summary>
        /// get protocol.
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        static public object GetProtocol(string protocol)
        {
            switch (protocol)
            {
                case Protocol.ProtocolNames.AsyncBinary: return new Protocol.AsyncBinaryProtocol();
                case Protocol.ProtocolNames.Thrift: return new Protocol.ThriftProtocol();
                case Protocol.ProtocolNames.CommandLine: return new Protocol.CommandLineProtocol();
            }
            return Activator.CreateInstance(Type.GetType(protocol, false));
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        static public void Start()
        {
            foreach (var server in _listHosts) server.Start();
        }
        /// <summary>
        /// 停止服务
        /// </summary>
        static public void Stop()
        {
            foreach (var server in _listHosts) server.Stop();
        }
        /// <summary>
        /// try get host by name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        static public bool TryGetHost(string name, out SocketBase.IHost host)
        {
            return _dicHosts.TryGetValue(name, out host);
        }
        #endregion
    }
}