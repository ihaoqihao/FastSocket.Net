Overview
========
<p>FastSocket是一个轻量级易扩展的c#异步socket通信库，项目开始于2011年，经过近3年不断调整与改进，目前在功能和性能上均有不错的表现。</p>
<p>项目地址:<a href="https://github.com/devhong/FastSocket.Net">https://github.com/devhong/FastSocket.Net</a>&nbsp;</p>
<p>FastSocket内置了命令行、二进制、thrift协议，基于此开发了Zookeeper, Redis, Thrift等c#异步客户端，接下来将会一一公开。</p>
Requirements
============
.Net 4.0 or Mono 2.6
Projects using FastSocket.Net
============
<a href="https://github.com/devhong/Redis.Driver.Net">Redis.Driver</a></ br>
<a href="https://github.com/devhong/Zookeeper.Net">Zookeeper.Net</a></ br>
<a href="https://github.com/devhong/Thrift.Net">Thrift.Net</a></ br>

Example Usage
=============
<p><span style="font-family: 黑体; font-size: 14pt; line-height: 1.5;">简单的命令行服务</span></p>
<p><span style="font-size: 14pt; font-family: 黑体;"><span style="font-size: 13px;">新建控制台项目，添加FastSocket.SocketBase,FastSocket.Server引用</span><br /></span></p>
<p><span style="font-size: 14pt; font-family: 黑体;"><span style="font-size: 13px;">自定义服务实现MyService</span></span></p>

```csharp
/// <summary>
/// 实现自定义服务
/// </summary>
public class MyService : CommandSocketService<StringCommandInfo>
{
    /// <summary>
    /// 当连接时会调用此方法
    /// </summary>
    /// <param name="connection"></param>
    public override void OnConnected(IConnection connection)
    {
        base.OnConnected(connection);
        Console.WriteLine(connection.RemoteEndPoint.ToString() + " connected");
        connection.BeginSend(PacketBuilder.ToCommandLine("welcome"));
    }
    /// <summary>
    /// 当连接断开时会调用此方法
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="ex"></param>
    public override void OnDisconnected(IConnection connection, Exception ex)
    {
        base.OnDisconnected(connection, ex);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(connection.RemoteEndPoint.ToString() + " disconnected");
        Console.ForegroundColor = ConsoleColor.Gray;
    }
    /// <summary>
    /// 当发生错误时会调用此方法
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="ex"></param>
    public override void OnException(IConnection connection, Exception ex)
    {
        base.OnException(connection, ex);
        Console.WriteLine("error: " + ex.ToString());
    }
    /// <summary>
    /// 处理未知命令
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="commandInfo"></param>
    protected override void HandleUnKnowCommand(IConnection connection, StringCommandInfo commandInfo)
    {
        commandInfo.Reply(connection, "unknow command:" + commandInfo.CmdName);
    }
}
```

<p>Exit命令</p>
```csharp
/// <summary>
/// 退出命令
/// </summary>
public sealed class ExitCommand : ICommand<StringCommandInfo>
{
    /// <summary>
    /// 返回命令名称
    /// </summary>
    public string Name
    {
        get { return "exit"; }
    }
    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="commandInfo"></param>
    public void ExecuteCommand(IConnection connection, StringCommandInfo commandInfo)
    {
        connection.BeginDisconnect();//断开连接
    }
}
```

<p>App.config配置</p>
```xml
<?xml version="1.0"?>
<configuration>

  <configSections>
    <section name="socketServer"
             type="Sodao.FastSocket.Server.Config.SocketServerConfig, FastSocket.Server_1.0"/>
  </configSections>

  <socketServer>
    <servers>
      <server name="cmdline"
              port="8400"
              socketBufferSize="8192"
              messageBufferSize="8192"
              maxMessageSize="102400"
              maxConnections="20000"
              serviceType="CommandLine.MyService, CommandLine"
              protocol="commandLine"/>
    </servers>
  </socketServer>

</configuration>
```

<p>初始化及启动服务</p>
```csharp
static void Main(string[] args)
{
    SocketServerManager.Init();
    SocketServerManager.Start();

    Console.ReadLine();
}
```

<p>启动服务，然后在cmd中运行telnet 127.0.0.1 8400, 运行截图如下：</p>
<p><img src="http://images.cnitblog.com/blog/21702/201308/15220257-a74cd62ae2c64d5eb4da160d44212272.png" alt="" /></p>
<p><img src="http://images.cnitblog.com/blog/21702/201308/15220409-b328d92f13d94c45b0a06452b2930d5a.png" alt="" /></p>
<p>其中welcome中当连接建立时服务端发送到终端的。</p>
<p>connection.BeginSend(PacketBuilder.ToCommandLine("welcome"));</p>
<p>unknow command:Hello是因为没有对应的"Hello"命令实现由HandleUnKnowCommand输出的</p>
```csharp
protected override void HandleUnKnowCommand(IConnection connection, StringCommandInfo commandInfo)
{
    commandInfo.Reply(connection, "unknow command:" + commandInfo.CmdName);
}
```
<p>当在终端中键入exit时，触发了ExitCommand.ExecuteCommand方法，服务端主动断开连接，终端退出。</p>
