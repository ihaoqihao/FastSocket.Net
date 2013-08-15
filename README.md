FastSocket.Net
==============

c#异步通信库

<p>FastSocket是一个轻量级易扩展的c#异步socket通信库（基于SocketAsyncEventArgs,传说中的IOCP），项目开始于2011年，经过近3年不断调整与改进，目前在功能和性能上均有不错的表现。支持.net 4.0+和mono 2.6+。</p>
<p>项目地址:<a href="https://github.com/devhong/FastSocket.Net">https://github.com/devhong/FastSocket.Net</a>&nbsp;(源码暂未上传)</p>
<p>FastSocket内置了命令行、二进制、thrift协议，基于此开发了Zookeeper, Redis, Thrift等c#异步客户端，接下来将会一一公开。</p>
<p>&nbsp;通过两个简单示例来了解一下FastSocket(在QuickStart/下有对应源码）</p>
<p><span style="font-size: 14pt; font-family: 黑体;">1:简单的命令行服务</span></p>
<p><span style="font-size: 14pt; font-family: 黑体;"><span style="font-size: 13px;">新建控制台项目，添加FastSocket.SocketBase,FastSocket.Server引用</span><br /></span></p>
<p><span style="font-size: 14pt; font-family: 黑体;"><span style="font-size: 13px;">自定义服务实现MyService</span></span></p>
<div class="cnblogs_code">
<pre><span style="font-size: 13px;"><span style="color: #0000ff;">using</span><span style="color: #000000;"> System;
</span><span style="color: #0000ff;">using</span><span style="color: #000000;"> Sodao.FastSocket.Server;
</span><span style="color: #0000ff;">using</span><span style="color: #000000;"> Sodao.FastSocket.Server.Command;
</span><span style="color: #0000ff;">using</span><span style="color: #000000;"> Sodao.FastSocket.SocketBase;

</span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;summary&gt;</span>
<span style="color: #808080;">///</span><span style="color: #008000;"> 实现自定义服务
</span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;/summary&gt;</span>
<span style="color: #0000ff;">public</span> <span style="color: #0000ff;">class</span> MyService : CommandSocketService&lt;StringCommandInfo&gt;<span style="color: #000000;">
{
    </span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;summary&gt;</span>
    <span style="color: #808080;">///</span><span style="color: #008000;"> 当连接时会调用此方法
    </span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;/summary&gt;</span>
    <span style="color: #808080;">///</span> <span style="color: #808080;">&lt;param name="connection"&gt;&lt;/param&gt;</span>
    <span style="color: #0000ff;">public</span> <span style="color: #0000ff;">override</span> <span style="color: #0000ff;">void</span><span style="color: #000000;"> OnConnected(IConnection connection)
    {
        </span><span style="color: #0000ff;">base</span><span style="color: #000000;">.OnConnected(connection);
        Console.WriteLine(connection.RemoteEndPoint.ToString() </span>+ <span style="color: #800000;">"</span><span style="color: #800000;"> connected</span><span style="color: #800000;">"</span><span style="color: #000000;">);
        connection.BeginSend(PacketBuilder.ToCommandLine(</span><span style="color: #800000;">"</span><span style="color: #800000;">welcome</span><span style="color: #800000;">"</span><span style="color: #000000;">));
    }
    </span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;summary&gt;</span>
    <span style="color: #808080;">///</span><span style="color: #008000;"> 当连接断开时会调用此方法
    </span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;/summary&gt;</span>
    <span style="color: #808080;">///</span> <span style="color: #808080;">&lt;param name="connection"&gt;&lt;/param&gt;</span>
    <span style="color: #808080;">///</span> <span style="color: #808080;">&lt;param name="ex"&gt;&lt;/param&gt;</span>
    <span style="color: #0000ff;">public</span> <span style="color: #0000ff;">override</span> <span style="color: #0000ff;">void</span><span style="color: #000000;"> OnDisconnected(IConnection connection, Exception ex)
    {
        </span><span style="color: #0000ff;">base</span><span style="color: #000000;">.OnDisconnected(connection, ex);
        Console.ForegroundColor </span>=<span style="color: #000000;"> ConsoleColor.Red;
        Console.WriteLine(connection.RemoteEndPoint.ToString() </span>+ <span style="color: #800000;">"</span><span style="color: #800000;"> disconnected</span><span style="color: #800000;">"</span><span style="color: #000000;">);
        Console.ForegroundColor </span>=<span style="color: #000000;"> ConsoleColor.Gray;
    }
    </span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;summary&gt;</span>
    <span style="color: #808080;">///</span><span style="color: #008000;"> 当发生错误时会调用此方法
    </span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;/summary&gt;</span>
    <span style="color: #808080;">///</span> <span style="color: #808080;">&lt;param name="connection"&gt;&lt;/param&gt;</span>
    <span style="color: #808080;">///</span> <span style="color: #808080;">&lt;param name="ex"&gt;&lt;/param&gt;</span>
    <span style="color: #0000ff;">public</span> <span style="color: #0000ff;">override</span> <span style="color: #0000ff;">void</span><span style="color: #000000;"> OnException(IConnection connection, Exception ex)
    {
        </span><span style="color: #0000ff;">base</span><span style="color: #000000;">.OnException(connection, ex);
        Console.WriteLine(</span><span style="color: #800000;">"</span><span style="color: #800000;">error: </span><span style="color: #800000;">"</span> +<span style="color: #000000;"> ex.ToString());
    }
    </span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;summary&gt;</span>
    <span style="color: #808080;">///</span><span style="color: #008000;"> 处理未知命令
    </span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;/summary&gt;</span>
    <span style="color: #808080;">///</span> <span style="color: #808080;">&lt;param name="connection"&gt;&lt;/param&gt;</span>
    <span style="color: #808080;">///</span> <span style="color: #808080;">&lt;param name="commandInfo"&gt;&lt;/param&gt;</span>
    <span style="color: #0000ff;">protected</span> <span style="color: #0000ff;">override</span> <span style="color: #0000ff;">void</span><span style="color: #000000;"> HandleUnKnowCommand(IConnection connection, StringCommandInfo commandInfo)
    {
        commandInfo.Reply(connection, </span><span style="color: #800000;">"</span><span style="color: #800000;">unknow command:</span><span style="color: #800000;">"</span> +<span style="color: #000000;"> commandInfo.CmdName);
    }
}</span></span></pre>
</div>
<p>Exit命令</p>
<div class="cnblogs_code">
<pre><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;summary&gt;</span>
<span style="color: #808080;">///</span><span style="color: #008000;"> 退出命令
</span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;/summary&gt;</span>
<span style="color: #0000ff;">public</span> <span style="color: #0000ff;">sealed</span> <span style="color: #0000ff;">class</span> ExitCommand : ICommand&lt;StringCommandInfo&gt;<span style="color: #000000;">
{
    </span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;summary&gt;</span>
    <span style="color: #808080;">///</span><span style="color: #008000;"> 返回命令名称
    </span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;/summary&gt;</span>
    <span style="color: #0000ff;">public</span> <span style="color: #0000ff;">string</span><span style="color: #000000;"> Name
    {
        </span><span style="color: #0000ff;">get</span> { <span style="color: #0000ff;">return</span> <span style="color: #800000;">"</span><span style="color: #800000;">exit</span><span style="color: #800000;">"</span><span style="color: #000000;">; }
    }
    </span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;summary&gt;</span>
    <span style="color: #808080;">///</span><span style="color: #008000;"> 执行命令
    </span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;/summary&gt;</span>
    <span style="color: #808080;">///</span> <span style="color: #808080;">&lt;param name="connection"&gt;&lt;/param&gt;</span>
    <span style="color: #808080;">///</span> <span style="color: #808080;">&lt;param name="commandInfo"&gt;&lt;/param&gt;</span>
    <span style="color: #0000ff;">public</span> <span style="color: #0000ff;">void</span><span style="color: #000000;"> ExecuteCommand(IConnection connection, StringCommandInfo commandInfo)
    {
        connection.BeginDisconnect();</span><span style="color: #008000;">//</span><span style="color: #008000;">断开连接</span>
<span style="color: #000000;">    }
}</span></pre>
</div>
<p>App.config配置</p>
<div class="cnblogs_code">
<pre><span style="color: #0000ff;">&lt;?</span><span style="color: #ff00ff;">xml version="1.0"</span><span style="color: #0000ff;">?&gt;</span>
<span style="color: #0000ff;">&lt;</span><span style="color: #800000;">configuration</span><span style="color: #0000ff;">&gt;</span>

  <span style="color: #0000ff;">&lt;</span><span style="color: #800000;">configSections</span><span style="color: #0000ff;">&gt;</span>
    <span style="color: #0000ff;">&lt;</span><span style="color: #800000;">section </span><span style="color: #ff0000;">name</span><span style="color: #0000ff;">="socketServer"</span><span style="color: #ff0000;">
             type</span><span style="color: #0000ff;">="Sodao.FastSocket.Server.Config.SocketServerConfig, FastSocket.Server_1.0"</span><span style="color: #0000ff;">/&gt;</span>
  <span style="color: #0000ff;">&lt;/</span><span style="color: #800000;">configSections</span><span style="color: #0000ff;">&gt;</span>

  <span style="color: #0000ff;">&lt;</span><span style="color: #800000;">socketServer</span><span style="color: #0000ff;">&gt;</span>
    <span style="color: #0000ff;">&lt;</span><span style="color: #800000;">servers</span><span style="color: #0000ff;">&gt;</span>
      <span style="color: #0000ff;">&lt;</span><span style="color: #800000;">server </span><span style="color: #ff0000;">name</span><span style="color: #0000ff;">="cmdline"</span><span style="color: #ff0000;">
              port</span><span style="color: #0000ff;">="8400"</span><span style="color: #ff0000;">
              socketBufferSize</span><span style="color: #0000ff;">="8192"</span><span style="color: #ff0000;">
              messageBufferSize</span><span style="color: #0000ff;">="8192"</span><span style="color: #ff0000;">
              maxMessageSize</span><span style="color: #0000ff;">="102400"</span><span style="color: #ff0000;">
              maxConnections</span><span style="color: #0000ff;">="20000"</span><span style="color: #ff0000;">
              serviceType</span><span style="color: #0000ff;">="CommandLine.MyService, CommandLine"</span><span style="color: #ff0000;">
              protocol</span><span style="color: #0000ff;">="commandLine"</span><span style="color: #0000ff;">/&gt;</span>
    <span style="color: #0000ff;">&lt;/</span><span style="color: #800000;">servers</span><span style="color: #0000ff;">&gt;</span>
  <span style="color: #0000ff;">&lt;/</span><span style="color: #800000;">socketServer</span><span style="color: #0000ff;">&gt;</span>

<span style="color: #0000ff;">&lt;/</span><span style="color: #800000;">configuration</span><span style="color: #0000ff;">&gt;</span></pre>
</div>
<p>初始化及启动服务</p>
<div class="cnblogs_code">
<pre><span style="color: #0000ff;">static</span> <span style="color: #0000ff;">void</span> Main(<span style="color: #0000ff;">string</span><span style="color: #000000;">[] args)
{
    SocketServerManager.Init();
    SocketServerManager.Start();

    Console.ReadLine();
}</span></pre>
</div>
<p>启动服务，然后在cmd中运行telnet 127.0.0.1 8400, 运行截图如下：</p>
<p><img src="http://images.cnitblog.com/blog/21702/201308/15220257-a74cd62ae2c64d5eb4da160d44212272.png" alt="" /></p>
<p><img src="http://images.cnitblog.com/blog/21702/201308/15220409-b328d92f13d94c45b0a06452b2930d5a.png" alt="" /></p>
<p>其中welcome中当连接建立时服务端发送到终端的。</p>
<p>connection.BeginSend(PacketBuilder.ToCommandLine("welcome"));</p>
<p>unknow command:Hello是因为没有对应的"Hello"命令实现由HandleUnKnowCommand输出的</p>
<div class="cnblogs_code">
<pre><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;summary&gt;</span>
<span style="color: #808080;">///</span><span style="color: #008000;"> 处理未知命令
</span><span style="color: #808080;">///</span> <span style="color: #808080;">&lt;/summary&gt;</span>
<span style="color: #808080;">///</span> <span style="color: #808080;">&lt;param name="connection"&gt;&lt;/param&gt;</span>
<span style="color: #808080;">///</span> <span style="color: #808080;">&lt;param name="commandInfo"&gt;&lt;/param&gt;</span>
<span style="color: #0000ff;">protected</span> <span style="color: #0000ff;">override</span> <span style="color: #0000ff;">void</span><span style="color: #000000;"> HandleUnKnowCommand(IConnection connection, StringCommandInfo commandInfo)
{
    commandInfo.Reply(connection, </span><span style="color: #800000;">"</span><span style="color: #800000;">unknow command:</span><span style="color: #800000;">"</span> +<span style="color: #000000;"> commandInfo.CmdName);
}</span></pre>
</div>
<p>当在终端中键入exit时，解发了ExitCommand.ExecuteCommand方法，服务端主动断开连接，终端退出。</p>
