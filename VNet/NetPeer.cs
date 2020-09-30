using System;
using System.Collections.Generic;
using System.Text;
using ENet;
using VNet.Util;

namespace VNet
{
    public class NetPeer
    {

        public uint id;
        public Peer peer;
        public NetServer server;

        public PacketGroup deferredPackets = new PacketGroup();


        public void Send(byte[] data, bool isReliable = false)
        {
            byte[] fixedData = new byte[data.Length + 1];

            unsafe
            {
                fixed (byte* dest = fixedData)
                {
                    *dest = (byte)data.Length;
                    fixed (byte* src = data)
                    {
                        Memory.MemoryCopy((void*)(dest + 1), (void*)src, data.Length);
                    }
                }
            }


            PacketFlags pf = PacketFlags.None;
            if (isReliable == true)
            {
                pf = PacketFlags.Reliable;
            }

            Packet p = default(Packet);

            p.Create(fixedData, pf);
            OutgoingPacket packet = new OutgoingPacket();
            packet.peerID = id;
            packet.packet = p;

            server.Send(packet);

        }

        public void SendDeferred(byte[] data, bool isReliable = false)
        {
            /*byte[] fixedData = new byte[data.Length + 1];

            unsafe
            {
                fixed (byte* dest = fixedData)
                {
                    *dest = (byte)data.Length;
                    fixed (byte* src = data)
                    {
                        Memory.MemoryCopy((void*)(dest + 1), (void*)src, data.Length);
                    }
                }
            }*/

            deferredPackets.AddPacket(data, data.Length);
            return;

            /*
            PacketFlags pf = PacketFlags.None;
            if (isReliable == true)
            {
                pf = PacketFlags.Reliable;
            }

            Packet p = default(Packet);

            p.Create(fixedData, pf);
            OutgoingPacket packet = new OutgoingPacket();
            packet.peerID = id;
            packet.packet = p;

            server.Send(packet);
            */

        }

        public void Flush()
        {

            if (deferredPackets.GetPacketCount() == 0)
                return;


            foreach(PacketGroupData pgd in deferredPackets.GetPackets())
            {
                PacketFlags pf = PacketFlags.None;
                

                Packet p = default(Packet);

                p.Create(pgd.data,0,pgd.size, pf);
                OutgoingPacket packet = new OutgoingPacket();
                packet.peerID = id;
                packet.packet = p;

                server.Send(packet);
            }

            deferredPackets.Clear();
        }
    }
}
