# VNet

VNet is a networking library built on top of [ENet](https://github.com/lsalzman/enet). The purpose of this library is to abstract away the network loop and provide an efficient use of threads to handle incoming and outgoing messages. 




Server example:

```csharp
	
NetServer server = new NetServer();

server.OnConnected = (NetPeer peer) =>
{
    Console.WriteLine("[Server] Connected");
    peer.Send(new byte[] { 44 });
};

server.OnReceived = (NetPeer peer,byte[] data,int length) =>
{
    Console.WriteLine("[Server] Received: {0}", BitConverter.ToString(data,0,length));
};

server.Start(10001);

```


Client example:

```csharp
NetClient client;

public void Main()
{
    client = new NetClient();

    client.OnConnected = OnConnected;
    client.OnReceived = OnReceived;
    client.Connect("127.0.0.1", 10001);

}


public static void OnConnected()
{
    Console.WriteLine("[Client] Connected");   
}

public static void OnReceived(byte[] data, int length)
{
    Console.WriteLine("[Client] Received: {0}", BitConverter.ToString(data, 0, length));
    client.Send(new byte[] { 1,88 });
}

```

References:  
	[ENet](https://github.com/lsalzman/enet)  
	[ENet-CSharp](https://github.com/nxrighthere/ENet-CSharp)  
	[Ring Buffer](https://github.com/dave-hillier/disruptor-unity3d)  
	[High performance memcpy](https://xoofx.com/blog/2010/10/23/high-performance-memcpy-gotchas-in-c/)