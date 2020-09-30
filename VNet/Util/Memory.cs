using System;
using System.Collections.Generic;
using System.Text;

namespace VNet.Util
{
    class Memory
    {
        public static unsafe void MemoryCopy(void* dest, void* src, int count)
        {
            int block;

            block = count >> 3;

            long* pDest = (long*)dest;
            long* pSrc = (long*)src;

            for (int i = 0; i < block; i++)
            {
                *pDest = *pSrc; pDest++; pSrc++;
            }
            dest = pDest;
            src = pSrc;
            count = count - (block << 3);

            if (count > 0)
            {
                byte* pDestB = (byte*)dest;
                byte* pSrcB = (byte*)src;
                for (int i = 0; i < count; i++)
                {
                    *pDestB = *pSrcB; pDestB++; pSrcB++;
                }
            }
        }
    }
}
