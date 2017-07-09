﻿using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace BruteforceLib.Hashing
{
    // Implementation of Bob Jenkins' hash function in C# (96 bit internal state)
    public class Jenkins96 : HashAlgorithm
    {
        private uint a, b, c;
        private ulong hashValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint rot(uint x, int k)
        {
            return (x << k) | (x >> (32 - k));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Mix()
        {
            a -= c; a ^= rot(c, 4); c += b;
            b -= a; b ^= rot(a, 6); a += c;
            c -= b; c ^= rot(b, 8); b += a;
            a -= c; a ^= rot(c, 16); c += b;
            b -= a; b ^= rot(a, 19); a += c;
            c -= b; c ^= rot(b, 4); b += a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Final()
        {
            c ^= b; c -= rot(b, 14);
            a ^= c; a -= rot(c, 11);
            b ^= a; b -= rot(a, 25);
            c ^= b; c -= rot(b, 16);
            a ^= c; a -= rot(c, 4);
            b ^= a; b -= rot(a, 14);
            c ^= b; c -= rot(b, 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ComputeHash(string str)
        {
            string input = str.Replace('/', '\\').ToUpper();
            ComputeHash(Encoding.ASCII.GetBytes(input));
            return hashValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Initialize()
        {
            //a = 0;
            //b = 0;
            //c = 0;
            //hashValue = 0;
            //hashBytes = null;
        }

        protected override unsafe void HashCore(byte[] array, int ibStart, int cbSize)
        {
            int length = array.Length;
            a = b = c = 0xdeadbeef + (uint)length;
            //a = b = c = (uint)length + (uint)*&pc - 0x21524111;

            fixed (byte* bb = array)
            {
                uint* u = (uint*)bb;

                if ((*u & 0x3) == 0)
                {
                    uint* k = u;

                    while (length > 12)
                    {
                        a += k[0];
                        b += k[1];
                        c += k[2];
                        Mix();
                        length -= 12;
                        k += 3;
                    }

                    switch (length)
                    {
                        case 12: c += k[2]; b += k[1]; a += k[0]; break;
                        case 11: c += k[2] & 0xffffff; b += k[1]; a += k[0]; break;
                        case 10: c += k[2] & 0xffff; b += k[1]; a += k[0]; break;
                        case 9: c += k[2] & 0xff; b += k[1]; a += k[0]; break;
                        case 8: b += k[1]; a += k[0]; break;
                        case 7: b += k[1] & 0xffffff; a += k[0]; break;
                        case 6: b += k[1] & 0xffff; a += k[0]; break;
                        case 5: b += k[1] & 0xff; a += k[0]; break;
                        case 4: a += k[0]; break;
                        case 3: a += k[0] & 0xffffff; break;
                        case 2: a += k[0] & 0xffff; break;
                        case 1: a += k[0] & 0xff; break;
                        case 0:
                            hashValue = ((ulong)c << 32) | b;
                            return;
                    }
                }
                else if ((*u & 0x1) == 0)
                {
                    ushort* k = (ushort*)u;

                    while (length > 12)
                    {
                        a += k[0] + (((uint)k[1]) << 16);
                        b += k[2] + (((uint)k[3]) << 16);
                        c += k[4] + (((uint)k[5]) << 16);
                        Mix();
                        length -= 12;
                        k += 6;
                    }

                    byte* k8 = (byte*)k;

                    switch (length)
                    {
                        case 12:
                            c += k[4] + (((uint)k[5]) << 16);
                            b += k[2] + (((uint)k[3]) << 16);
                            a += k[0] + (((uint)k[1]) << 16);
                            break;
                        case 11:
                            c += k[4] + (((uint)k8[10]) << 16);
                            b += k[2] + (((uint)k[3]) << 16);
                            a += k[0] + (((uint)k[1]) << 16);
                            break;
                        case 10:
                            c += k[4];
                            b += k[2] + (((uint)k[3]) << 16);
                            a += k[0] + (((uint)k[1]) << 16);
                            break;
                        case 9:
                            c += k8[8];
                            b += k[2] + (((uint)k[3]) << 16);
                            a += k[0] + (((uint)k[1]) << 16);
                            break;
                        case 8:
                            b += k[2] + (((uint)k[3]) << 16);
                            a += k[0] + (((uint)k[1]) << 16);
                            break;
                        case 7:
                            b += k[2] + (((uint)k8[6]) << 16);
                            a += k[0] + (((uint)k[1]) << 16);
                            break;
                        case 6:
                            b += k[2];
                            a += k[0] + (((uint)k[1]) << 16);
                            break;
                        case 5:
                            b += k8[4];
                            a += k[0] + (((uint)k[1]) << 16);
                            break;
                        case 4:
                            a += k[0] + (((uint)k[1]) << 16);
                            break;
                        case 3:
                            a += k[0] + (((uint)k8[2]) << 16);
                            break;
                        case 2:
                            a += k[0];
                            break;
                        case 1:
                            a += k8[0];
                            break;
                        case 0:
                            hashValue = ((ulong)c << 32) | b;
                            return;
                    }
                }
                else
                {
                    byte* k = (byte*)u;

                    while (length > 12)
                    {
                        a += k[0];
                        a += ((uint)k[1]) << 8;
                        a += ((uint)k[2]) << 16;
                        a += ((uint)k[3]) << 24;
                        b += k[4];
                        b += ((uint)k[5]) << 8;
                        b += ((uint)k[6]) << 16;
                        b += ((uint)k[7]) << 24;
                        c += k[8];
                        c += ((uint)k[9]) << 8;
                        c += ((uint)k[10]) << 16;
                        c += ((uint)k[11]) << 24;
                        Mix();
                        length -= 12;
                        k += 12;
                    }

                    switch (length)
                    {
                        case 12:
                            c += (((uint)k[11]) << 24) + (((uint)k[10]) << 16) + (((uint)k[9]) << 8) + k[8];
                            b += (((uint)k[7]) << 24) + (((uint)k[6]) << 16) + (((uint)k[5]) << 8) + k[4];
                            a += (((uint)k[3]) << 24) + (((uint)k[2]) << 16) + (((uint)k[1]) << 8) + k[0];
                            break;
                        case 11:
                            c += (((uint)k[10]) << 16) + (((uint)k[9]) << 8) + k[8];
                            b += (((uint)k[7]) << 24) + (((uint)k[6]) << 16) + (((uint)k[5]) << 8) + k[4];
                            a += (((uint)k[3]) << 24) + (((uint)k[2]) << 16) + (((uint)k[1]) << 8) + k[0];
                            break;
                        case 10:
                            c += (((uint)k[9]) << 8) + k[8];
                            b += (((uint)k[7]) << 24) + (((uint)k[6]) << 16) + (((uint)k[5]) << 8) + k[4];
                            a += (((uint)k[3]) << 24) + (((uint)k[2]) << 16) + (((uint)k[1]) << 8) + k[0];
                            break;
                        case 9:
                            c += k[8];
                            b += (((uint)k[7]) << 24) + (((uint)k[6]) << 16) + (((uint)k[5]) << 8) + k[4];
                            a += (((uint)k[3]) << 24) + (((uint)k[2]) << 16) + (((uint)k[1]) << 8) + k[0];
                            break;
                        case 8:
                            b += (((uint)k[7]) << 24) + (((uint)k[6]) << 16) + (((uint)k[5]) << 8) + k[4];
                            a += (((uint)k[3]) << 24) + (((uint)k[2]) << 16) + (((uint)k[1]) << 8) + k[0];
                            break;
                        case 7:
                            b += (((uint)k[6]) << 16) + (((uint)k[5]) << 8) + k[4];
                            a += (((uint)k[3]) << 24) + (((uint)k[2]) << 16) + (((uint)k[1]) << 8) + k[0];
                            break;
                        case 6:
                            b += (((uint)k[5]) << 8) + k[4];
                            a += (((uint)k[3]) << 24) + (((uint)k[2]) << 16) + (((uint)k[1]) << 8) + k[0];
                            break;
                        case 5:
                            b += k[4];
                            a += (((uint)k[3]) << 24) + (((uint)k[2]) << 16) + (((uint)k[1]) << 8) + k[0];
                            break;
                        case 4:
                            a += (((uint)k[3]) << 24) + (((uint)k[2]) << 16) + (((uint)k[1]) << 8) + k[0];
                            break;
                        case 3:
                            a += (((uint)k[2]) << 16) + (((uint)k[1]) << 8) + k[0];
                            break;
                        case 2:
                            a += (((uint)k[1]) << 8) + k[0];
                            break;
                        case 1:
                            a += k[0];
                            break;
                        case 0:
                            hashValue = ((ulong)c << 32) | b;
                            return;
                    }
                }

                Final();
                hashValue = ((ulong)c << 32) | b;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe override byte[] HashFinal()
        {
            return BitConverter.GetBytes(hashValue);
        }
    }
}
