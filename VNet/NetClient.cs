using System;
using System.Collections.Generic;
using System.Text;
using ENet;
using VNet.Util;

namespace VNet
{
    public class NetClient
    {
        RingBuffer<OutgoingPacket> outgoingPackets = new RingBuffer<OutgoingPacket>(1000);
        RingBuffer<IncomingPacket> incomingPackets = new RingBuffer<IncomingPacket>(1000);

        public delegate void OnReceivedEvent(byte[] data, int length);
        public delegate void OnConnectedEvent();
        public delegate void OnDisconnectedEvent();

        public OnReceivedEvent OnReceived;
        public OnConnectedEvent OnConnected;
        public OnDisconnectedEvent OnDisconnected;

        Host host = new Host();
        ENet.Event netEvent;
        Peer peer;

        System.Threading.Thread networkThread;
        System.Threading.Thread receiveThread;

        public void Connect(string _address,ushort _port)
        {
            if (ENet.Library.Initialize() != true)
            {
                throw new Exception("ENet failed to initalize");
            }


            Address address = new Address();
            address.SetHost(_address);

            address.Port = _port;
            host.Create();

            peer = host.Connect(address);

            //networkThread = new System.Threading.Thread(Poll);
            //networkThread.Start();

            //receiveThread = new System.Threading.Thread(Receive);
            //receiveThread.Start();

        }

        public void Disconnect()
        {
            peer.Disconnect(0);
            networkThread.Abort();
            receiveThread.Abort();
        }

        public void Poll()
        {
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
                            OnConnected();
                            break;

                        case ENet.EventType.Receive:
                            byte[] buffer = new byte[netEvent.Packet.Length];
                            netEvent.Packet.CopyTo(buffer);

                            IncomingPacket ip = new IncomingPacket();
                            ip.data = buffer;
                            ip.length = netEvent.Packet.Length;
                            incomingPackets.Enqueue(ip);
                            netEvent.Packet.Dispose();
                            break;

                        case ENet.EventType.Disconnect:

                            networkThread.Abort();
                            receiveThread.Abort();

                            OnDisconnected();
                            break;
                    }

                }

                OutgoingPacket outgoingPacket;
                for (int i = 0; i < outgoingPackets.Count; i++)
                {
                    outgoingPackets.TryDequeue(out outgoingPacket);
                    peer.Send(0, ref outgoingPacket.packet);
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

                        (byte[]_data,int _len) => { 
                            OnReceived(_data, _len); }
                        );
                    

                }
            //}
        }

        public void Send(byte[] data, bool isReliable = false)
        {
            PacketFlags pf = PacketFlags.None;
            if (isReliable == true)
            {
                pf = PacketFlags.Reliable;
            }

            Packet p = default(Packet);

            p.Create(data, pf);
            OutgoingPacket packet = new OutgoingPacket();
            packet.packet = p;

            outgoingPackets.Enqueue(packet);

        }

    }
}
