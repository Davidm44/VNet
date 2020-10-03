using System;
using System.Collections.Generic;
using System.Text;
using ENet;
using NetStack.Buffers;
using VNet.Util;

namespace VNet
{
    public enum SEND_TYPE
    {
        SEND_UNRELIABLE = 0,
        SEND_RELIABLE = 1,
        SEND_SEQUENCED = 2
    }


    public struct OutgoingPacket
    {
        public int channelID;
        public uint peerID;
        public Packet packet;
        public SEND_TYPE sendType;

    }

    public struct IncomingPacket
    {
        public uint peerID;
        public byte[] data;
        public int length;
    }

    public class PacketUtil
    {

        private static ArrayPool<byte> packetHandlerPool = ArrayPool<byte>.Create(1500, 50);

        public static void HandleGroup(byte[] data, int length,Action<byte[],int> iteratePacketCallback)
        {
            byte[] packetData = packetHandlerPool.Rent(1024);
            int sizeRead = 0;
            unsafe
            {
                fixed (byte* _data = data)
                {
                    do
                    {
                        byte packetSize = *(_data + sizeRead);
                        fixed (byte* dest = packetData)
                        {
                            Memory.MemoryCopy((void*)dest, (void*)(_data + sizeRead + 1), packetSize);
                            sizeRead += packetSize + 1;
                        }

                        //callback
                        iteratePacketCallback(packetData, packetSize);

                    } while (sizeRead < length);
                }
            }

            packetHandlerPool.Return(packetData);
        }
    }

}
