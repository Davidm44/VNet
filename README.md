# VNet

VNet is a networking library built on top of [ENet](https://github.com/lsalzman/enet). The purpose of this library is to abstract away the network loop and provide an efficient use of threads to handle incoming and outgoing messages. 




Server example:

```csharp
	
NetServer server = new NetServer();

server.OnConnected = (NetPeer _peer) =>
{
    Console.WriteLine("Connected");

};

server.OnReceived = (NetPeer _peer, byte[] data, int length) =>
{
    Console.WriteLine("Received: {0}", BitConverter.ToString(data, 0, length));
};


server.Start(10001);

while(true)
{
    server.Poll();
    server.Receive();
}

```


Client example:

```csharp

NetClient client = new NetClient();

client.OnConnected = () =>
{
    Console.WriteLine("Connected");
};

client.OnReceived = (byte[] data, int length) =>
{
    Console.WriteLine("Received: {0}", BitConverter.ToString(data, 0, length));
};

client.Connect("127.0.0.1", 10001);

while(true)
{
    client.Poll();
    client.Receive();
}

```

References:  
	[ENet](https://github.com/lsalzman/enet)  
	[ENet-CSharp](https://github.com/nxrighthere/ENet-CSharp)  
	[Ring Buffer](https://github.com/dave-hillier/disruptor-unity3d)  
	[High performance memcpy](https://xoofx.com/blog/2010/10/23/high-performance-memcpy-gotchas-in-c/)