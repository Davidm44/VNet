using System;
using System.Collections.Generic;
using System.Text;
using VNet.Util;
using ENet;
using System.Threading;
using NetStack.Buffers;


namespace VNet
{
    public class NetServer
    {


        private int MESSAGE_OUTPUT_QUEUE_SIZE = 100000;
        private int MESSAGE_INPUT_QUEUE_SIZE = 100000;
        private int MAX_PEERS = 4000;


        private ArrayPool<byte> receiveBufferPool = ArrayPool<byte>.Create(1500, 100);

        public RingBuffer<OutgoingPacket> outgoingPackets;
        public RingBuffer<IncomingPacket> incomingPackets;

         Dictionary<uint, NetPeer> clients = new Dictionary<uint, NetPeer>();


        public delegate void OnConnectedEvent(NetPeer peer);
        public delegate void OnReceivedEvent(NetPeer peer, byte[] data, int length);
        public delegate void OnDisconnectedEvent(NetPeer peer);
        public delegate void OnTimeoutEvent(NetPeer peer);

        public OnConnectedEvent OnConnected;
        public OnReceivedEvent OnReceived;
        public OnDisconnectedEvent OnDisconnected;
        public OnTimeoutEvent OnTimeout;


        //ENet stuff
        private Host host;
        private Address address;
        private ENet.Event netEvent;

        private Thread networkThread;
        private Thread receiveThread;

        private void CreateNetPeer(Peer _peer)
        {
            NetPeer _netPeer = new NetPeer();
            _netPeer.server = this;
            _netPeer.peer = _peer;
            _netPeer.id = _peer.ID;

            clients.Add(_netPeer.id, _netPeer);

        }

        public Dictionary<uint,NetPeer> GetPeers()
        {
            return clients;
        }

        public void Start(ushort _port)
        {
            outgoingPackets = new RingBuffer<OutgoingPacket>(MESSAGE_OUTPUT_QUEUE_SIZE);
            incomingPackets = new RingBuffer<IncomingPacket>(MESSAGE_INPUT_QUEUE_SIZE);

            ENet.Library.Initialize();

            host = new Host();
            Address address = new Address();

            address.Port = _port;
            host.Create(address, MAX_PEERS);


            /*
            networkThread = new Thread(NetworkThread);
            networkThread.Start();

            receiveThread = new Thread(ReceiveThread);
            receiveThread.Start();
            */
        }

        public void Poll()
        {

            int bufferLength = 0;
            //while (true)
            //{
                bool polled = false;
                while (!polled)
                {

                    if (host.CheckEvents(out netEvent) <= 0)
                    {
                        if (host.Service(0, out netEvent) <= 0)
                            break;

                        polled = true;
                    }

                    switch (netEvent.Type)
                    {
                        case ENet.EventType.Connect:
                           
                            CreateNetPeer(netEvent.Peer);
                            OnConnected(clients[netEvent.Peer.ID]);
                            break;


                        case ENet.EventType.Receive:

                            byte[] buffer = receiveBufferPool.Rent(netEvent.Packet.Length);
                            netEvent.Packet.CopyTo(buffer);

                            IncomingPacket ip = new IncomingPacket();
                            ip.data = buffer;
                            ip.length = netEvent.Packet.Length;
                            ip.peerID = netEvent.Peer.ID;
                            incomingPackets.Enqueue(ip);
                            netEvent.Packet.Dispose();

                            break;

                        case ENet.EventType.Disconnect:

                            OnDisconnected(clients[netEvent.Peer.ID]);
                            clients.Remove(netEvent.Peer.ID);
                            
                            break;

                        case ENet.EventType.Timeout:

                            OnTimeout(clients[netEvent.Peer.ID]);
                            clients.Remove(netEvent.Peer.ID);
                           
                            break;
                    }


                }

                //send messages in queue
                int counter = 0;
                for (int i = 0; i < outgoingPackets.Count; i++)
                {
                    OutgoingPacket packet;
                    if (outgoingPackets.TryDequeue(out packet))
                    {
                        if (clients.ContainsKey(packet.peerID))
                        {
                            clients[packet.peerID].peer.Send(0, ref packet.packet);

                            /*
                            byte[] tempBuffer = new byte[packet.packet.Length];
                            packet.packet.CopyTo(tempBuffer);
                            Console.WriteLine("[Network Send] {0}", BitConverter.ToString(tempBuffer));
                            */

                            counter++;
                        }
                    }
                }
            //}
        }


        public void Receive()
        {
            //while (true)
            //{
                IncomingPacket incomingPacket;
                while (incomingPackets.TryDequeue(out incomingPacket))
                {
                    PacketUtil.HandleGroup(
                        incomingPacket.data,
                        incomingPacket.length,

                        (byte[] _data, int _len) => {
                            OnReceived(clients[incomingPacket.peerID],_data, _len);
                            
                        }
                        );

                    receiveBufferPool.Return(incomingPacket.data);


                }
            //}
        }


        public void Send(OutgoingPacket packet)
        {
            outgoingPackets.Enqueue(packet);
        }

    }
}
