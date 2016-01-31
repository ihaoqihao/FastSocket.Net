Overview
========
<p>项目地址:<a href="https://github.com/devhong/FastSocket.Net">https://github.com/devhong/FastSocket.Net</a>&nbsp;</p>
<p>在Nuget官方源中搜索fastsocket可快速安装引用</p>
<p>QQ群：257612438</p>
<p>FastSocket内置了命令行、二进制、thrift协议，基于此开发了Zookeeper, Redis, Thrift等c#异步客户端。</p>
Requirements
============
.Net 4.0 or Mono 2.6
Projects using FastSocket.Net
============
- <a href="https://github.com/devhong/Redis.Driver.Net">Redis.Driver</a>
- <a href="https://github.com/devhong/Zookeeper.Net">Zookeeper.Net</a>
- <a href="https://github.com/devhong/Thrift.Net">Thrift.Net</a>

Example Usage
=============
<h3>1: 简单的命令行服务.</h3>
<p>新建控制台项目，添加FastSocket.SocketBase,FastSocket.Server引用</p>
<p>自定义服务实现MyService</p>

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
             type="Sodao.FastSocket.Server.Config.SocketServerConfig, FastSocket.Server"/>
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


<h3>2: 在服务中使用自定义二进制协议</h3>

<div>新建控制台项目，命名为Server</div>
<div>添加FastSocket.SocketBase,FastSocket.Server引用</div>

<div>Socket命令服务类: Sodao.FastSocket.Server.CommandSocketService泛型类</div>
<div>其中需要实现Socket连接，断开，异常，发送完回调及处理未知命令的方法</div>

<div>内置的二进制命令对象: Sodao.FatSocket.Server.Command.AsyncBinaryCommandInfo</div>
<div>由一个command name,一个唯一标识SeqId和主题内容buffer构建。</div>

<div>定义服务类MyService继承CommandSocketService类，</div>
<div>泛型类型为上述的AsyncBinanryCommandInfo</div>
</p>

```csharp
/// <summary>
/// 实现自定义服务
/// </summary>
public class MyService : CommandSocketService<AsyncBinaryCommandInfo>
{
    /// <summary>
    /// 当连接时会调用此方法
    /// </summary>
    /// <param name="connection"></param>
    public override void OnConnected(IConnection connection)
    {
        base.OnConnected(connection);
        Console.WriteLine(connection.RemoteEndPoint.ToString() + " connected");
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
    /// 当服务端发送Packet完毕会调用此方法
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="e"></param>
    public override void OnSendCallback(IConnection connection, SendCallbackEventArgs e)
    {
        base.OnSendCallback(connection, e);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("send " + e.Status.ToString());
        Console.ForegroundColor = ConsoleColor.Gray;
    }
    /// <summary>
    /// 处理未知的命令
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="commandInfo"></param>
    protected override void HandleUnKnowCommand(IConnection connection, AsyncBinaryCommandInfo commandInfo)
    {
        Console.WriteLine("unknow command: " + commandInfo.CmdName);
    }
}
```

<div>实现一个命令如示例项目中的SumCommand类，命令类需要实现ICommand泛型接口</div>
<div>即服务中可以进行处理的服务契约</div>
<div>而泛型类型即上述的AsyncBinaryCommandInfo</div>

```csharp
/// <summary>
/// sum command
/// 用于将一组int32数字求和并返回
/// </summary>
public sealed class SumCommand : ICommand<AsyncBinaryCommandInfo>
{
    /// <summary>
    /// 返回服务名称
    /// </summary>
    public string Name
    {
        get { return "sum"; }
    }
    /// <summary>
    /// 执行命令并返回结果
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="commandInfo"></param>
    public void ExecuteCommand(IConnection connection, AsyncBinaryCommandInfo commandInfo)
    {
        if (commandInfo.Buffer == null || commandInfo.Buffer.Length == 0)
        {
            Console.WriteLine("sum参数为空");
            connection.BeginDisconnect();
            return;
        }
        if (commandInfo.Buffer.Length % 4 != 0)
        {
            Console.WriteLine("sum参数错误");
            connection.BeginDisconnect();
            return;
        }

        int skip = 0;
        var arr = new int[commandInfo.Buffer.Length / 4];
        for (int i = 0, l = arr.Length; i < l; i++)
        {
            arr[i] = BitConverter.ToInt32(commandInfo.Buffer, skip);
            skip += 4;
        }

        commandInfo.Reply(connection, BitConverter.GetBytes(arr.Sum()));
    }
}
```
app.config
```xml
<?xml version="1.0"?>
<configuration>

  <configSections>
    <section name="socketServer"
             type="Sodao.FastSocket.Server.Config.SocketServerConfig, FastSocket.Server"/>
  </configSections>

  <socketServer>
    <servers>
      <server name="binary"
              port="8401"
              socketBufferSize="8192"
              messageBufferSize="8192"
              maxMessageSize="102400"
              maxConnections="20000"
              serviceType="Server.MyService, Server"
              protocol="asyncBinary"/>
    </servers>
  </socketServer>

</configuration>
```

<div>其中section name="socketServer" 为服务端默认读取的sectionName</div>
<div>type为反射自FastSocket.Server中的config类型</div>
<div>server配置中，name自定，serviceType为上述实现的服务类反射类型</div>
<div>协议名为asyncBinary</div>

<div>在Main函数中启动服务</div>
</p>
```csharp
static void Main(string[] args)
{
    SocketServerManager.Init();
    SocketServerManager.Start();

    Console.ReadLine();
}
```

<div>新建控制台应用程序，命名为Client</div>
<div>添加FastSocket.Client,FastSocket.SocketBase引用</div>

<div>客户端的代码为组织命令向服务端请求</div>
<div>创建一个Sodao.FastSocket.Client.AsyncBinarySocketClient的实例</div>
<div>并通过RegisterServerNode来注册服务端节点，需要注意name必须唯一</div>
<div>并且地址为我们服务端运行的地址，端口为服务端配置文件中配置的端口号</div>
```csharp
static void Main(string[] args)
{
    var client = new Sodao.FastSocket.Client.AsyncBinarySocketClient(8192, 8192, 3000, 3000);
    //注册服务器节点，这里可注册多个(name不能重复）
    client.RegisterServerNode("127.0.0.1:8401", new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8401));
    //client.RegisterServerNode("127.0.0.1:8402", new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.2"), 8401));

    //组织sum参数, 格式为<<i:32-limit-endian,....N>>
    //这里的参数其实也可以使用thrift, protobuf, bson, json等进行序列化，
    byte[] bytes = null;
    using (var ms = new System.IO.MemoryStream())
    {
        for (int i = 1; i <= 1000; i++) ms.Write(BitConverter.GetBytes(i), 0, 4);
        bytes = ms.ToArray();
    }
    //发送sum命令
    client.Send("sum", bytes, res => BitConverter.ToInt32(res.Buffer, 0)).ContinueWith(c =>
    {
        if (c.IsFaulted)
        {
            Console.WriteLine(c.Exception.ToString());
            return;
        }
        Console.WriteLine(c.Result);
    });

    Console.ReadLine();
}
```
