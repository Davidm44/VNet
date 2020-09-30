using System;
using System.Collections.Generic;
using System.Text;
using VNet.Util;

namespace VNet
{
    
    public struct PacketGroupData
    {
        public int size;
        public byte[] data;
    }

    public class PacketGroup
    {

        private const ushort MTU_SIZE = 1000; //


        private int numPackets = 0;
        private List<PacketGroupData> packetGroups = new List<PacketGroupData>();
        private PacketGroupData currPacketGroup = new PacketGroupData();


        public PacketGroup()
        {

        }

        public int GetPacketCount()
        {
            return numPackets;
        }

        public List<PacketGroupData> GetPackets()
        {
            List<PacketGroupData> _packets = new List<PacketGroupData>();
            _packets.AddRange(packetGroups);
            if (currPacketGroup.data != null)
            {
                _packets.Add(currPacketGroup);
            }
            return _packets;
        }

        public void Clear()
        {
            packetGroups.Clear();
            currPacketGroup = new PacketGroupData();
            numPackets = 0;
        }

        public void AddPacket(byte[] packetData, int length)
        {
            
            int lengthOfPacket = length;


            if (currPacketGroup.data == null)
            {
                currPacketGroup.data = new byte[MTU_SIZE];
            }

            if (currPacketGroup.size + lengthOfPacket + 1 >= MTU_SIZE)
            {
                packetGroups.Add(currPacketGroup);
                currPacketGroup = new PacketGroupData();
                currPacketGroup.data = new byte[MTU_SIZE];
            }

            unsafe
            {
                fixed (byte* destPointer = currPacketGroup.data)
                {
                    fixed (byte* srcPointer = packetData)
                    {
                        *(destPointer + currPacketGroup.size) = (byte)length;
                        Memory.MemoryCopy((void*)(destPointer + currPacketGroup.size + 1), (void*)srcPointer, lengthOfPacket);
                        currPacketGroup.size += lengthOfPacket + 1;
                    }
                }
            }

            numPackets++;

        }
    }
}
