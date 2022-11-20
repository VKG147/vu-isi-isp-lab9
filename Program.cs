using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args) 
        {
            string c = "1000111101000100011010001111100110100001011001111101011101101000";

            string k1 = "010100000010110010101100010001000010100001101000";
            string k2 = "010100001010110010100100010000001100010011000001";
            string k3 = "110100001010110000100110110010101000010000001001";
            string k4 = "111000001010011000100110100010100101011100001000";
            string k5 = "111000001001011000100110000110000101001100100000";
            string k6 = "111000001001001001110010110100000100100000100000";
            string k7 = "101001001101001001110010110000000010101000011000";
            string k8 = "101001100101001101010010101100010011001000011000";
            string k9 = "001001100101001101010011100010110000011000000101";
            string k10 = "001011110101000101010001000110100100001110000100";
            string k11 = "000011110100000111011001000100000100000110000101";
            string k12 = "000111110100000110011001110000100010000010000001";
            string k13 = "000111110000100110001001111000100010001100001001";
            string k14 = "000110110010100010001101001100100001001100001010";
            string k15 = "000110010010110010001100010101000001000100100010";
            string k16 = "010100010010110010001100000011001100000111000100";


            BitArray[] keys = new BitArray[16]
            {
                new BitArray(BitStringToBitArray(k1)),
                new BitArray(BitStringToBitArray(k2)),
                new BitArray(BitStringToBitArray(k3)),
                new BitArray(BitStringToBitArray(k4)),
                new BitArray(BitStringToBitArray(k5)),
                new BitArray(BitStringToBitArray(k6)),
                new BitArray(BitStringToBitArray(k7)),
                new BitArray(BitStringToBitArray(k8)),
                new BitArray(BitStringToBitArray(k9)),
                new BitArray(BitStringToBitArray(k10)),
                new BitArray(BitStringToBitArray(k11)),
                new BitArray(BitStringToBitArray(k12)),
                new BitArray(BitStringToBitArray(k13)),
                new BitArray(BitStringToBitArray(k14)),
                new BitArray(BitStringToBitArray(k15)),
                new BitArray(BitStringToBitArray(k16))
            };

            BitArray m = DecryptDES(BitStringToBitArray(c), keys);

            PrintBitArray(m, nameof(m));

            BitArray m_rev = ReverseBytes(new BitArray(m));

            byte[] bytes = new byte[m_rev.Length / 8];

            m_rev.CopyTo(bytes, 0);

            string m_str = Encoding.ASCII.GetString(bytes);

            Console.WriteLine(m_str);
        }

        static BitArray DecryptDES(BitArray c, BitArray[] keys)
        {
            BitArray r16l16 = ReversePermutate(c, ip1_perm);

            PrintBitArray(r16l16, nameof(r16l16));

            (BitArray r16, BitArray l16) = Split(r16l16);

            BitArray[] l = new BitArray[16];
            BitArray[] r = new BitArray[16];

            l[15] = new BitArray(l16);
            r[15] = new BitArray(r16);

            BitArray l_prev = new BitArray(l16);
            BitArray r_prev = new BitArray(r16);

            for (int i = 15; i > 0; --i)
            {
                l[i-1] = new BitArray(r_prev).Xor(f_DES(l_prev, keys[i]));
                r[i-1] = new BitArray(l_prev);

                PrintBitArray(l[i - 1], $"l{i}");
                PrintBitArray(r[i - 1], $"r{i}");

                l_prev = new BitArray(l[i - 1]);
                r_prev = new BitArray(r[i - 1]);
            }

            BitArray l0 = new BitArray(r[0]).Xor(f_DES(l[0], keys[0]));
            BitArray r0 = new BitArray(l[0]);

            PrintBitArray(l0, nameof(l0));
            PrintBitArray(r0, nameof(r0));

            BitArray ip = Combine(l0, r0);

            BitArray m = ReversePermutate(ip, ip_perm);

            return m;
        }

        static BitArray f_DES(BitArray block, BitArray k)
        {
            // block 32 -> 48
            BitArray block_exp = Permutate(block, E_perm);
            block_exp = block_exp.Xor(k);

            BitArray[] b = new BitArray[8];
            for (int i = 0; i < 8; ++i)
            {
                b[i] = new BitArray(new bool[] {
                    block_exp.Get(i * 6),
                    block_exp.Get(i * 6 + 1),
                    block_exp.Get(i * 6 + 2),
                    block_exp.Get(i * 6 + 3),
                    block_exp.Get(i * 6 + 4),
                    block_exp.Get(i * 6 + 5) });
            }

            BitArray[] s = new BitArray[8];
            for (int i = 0; i < 8; ++i)
            {
                int[][] s_table = new int[3][];
                if (i == 0) s_table = s1;
                else if (i == 1) s_table = s2;
                else if (i == 2) s_table = s3;
                else if (i == 3) s_table = s4;
                else if (i == 4) s_table = s5;
                else if (i == 5) s_table = s6;
                else if (i == 6) s_table = s7;
                else if (i == 7) s_table = s8;

                s[i] = GetSFromB(b[i], s_table);
            }

            BitArray y = new BitArray(32);
            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    y[i * 4 + j] = s[i][j];
                }
            }

            return Permutate(y, P_perm);
        }

        static BitArray GetSFromB(BitArray b, int[][] s_table)
        {
            int i =
                (b[0] ? 1 : 0) * 2 +
                (b[5] ? 1 : 0);
            int j =
                (b[1] ? 1 : 0) * 8 +
                (b[2] ? 1 : 0) * 4 +
                (b[3] ? 1 : 0) * 2 +
                (b[4] ? 1 : 0);

            int val = s_table[i][j];

            BitArray s = new BitArray(4);
            s[3] = val % 2 == 1;
            s[2] = (val / 2) % 2 == 1;
            s[1] = (val / 2 / 2) % 2 == 1;
            s[0] = (val / 2 / 2 / 2) % 2 == 1;

            return s;
        }

        static BitArray Permutate(BitArray k, int[] table)
        {
            BitArray r = new BitArray(table.Length);

            for (int i = 0; i < table.Length; i++)
            {
                r[i] = k[table[i] - 1];
            }

            return r;
        }

        static BitArray ReversePermutate(BitArray r, int[] table)
        {
            BitArray k = new BitArray(table.Max());

            for (int i = 0; i < k.Length; ++i)
            {
                int j = table.ToList().FindIndex(v => v == i + 1);
                k[i] = r[j];
            }

            return k;
        }

        static BitArray ReverseBytes(BitArray bitArray)
        {
            BitArray reversed = new BitArray(bitArray);
            for (int i = 0; i < bitArray.Length / 8; i++)
            {
                for (int j = 0; j < 8; ++j)
                {
                    reversed[i * 8 + j] = bitArray[i * 8 + (7 - j)];
                }
            }
            return reversed;
        }

        static (BitArray, BitArray) Split(BitArray bitArray)
        {
            BitArray l = new BitArray(bitArray.Length / 2);
            BitArray r = new BitArray(bitArray.Length / 2);

            for (int i = 0; i < l.Length; ++i)
            {
                l[i] = bitArray[i];
                r[i] = bitArray[i + bitArray.Length / 2];
            }

            return (l, r);
        }

        static BitArray Combine(BitArray a, BitArray b)
        {
            BitArray result = new BitArray(a.Length + b.Length);
            for (int i = 0; i < a.Length; ++i)
            {
                result[i] = a[i];
                result[i + a.Length] = b[i];
            }

            return result;
        }

        static void PrintBitArray(BitArray b, string name = null)
        {
            if (name != null)
            {
                Console.Write($"{name} = ");
            }

            for (int i = 0; i < b.Length; ++i)
            {
                Console.Write(b[i] ? "1" : "0");
            }
            Console.WriteLine();
        }

        static BitArray BitStringToBitArray(string b)
        {
            BitArray bitArray = new BitArray(b.Length);

            for (int i = 0; i < b.Length; ++i)
            {
                if (b[i] == '1') bitArray[i] = true;
                else if (b[i] == '0') bitArray[i] = false;
                else throw new ArgumentException("Bit string not in correct format.", nameof(b));
            }

            return bitArray;
        }

        static int[] pc1_perm = new int[56]
        {
            57, 49, 41, 33, 25, 17, 9,
            1, 58, 50, 42, 34, 26, 18,
            10, 2, 59, 51, 43, 35, 27,
            19, 11, 3, 60, 52, 44, 36,
            63, 55, 47, 39, 31, 23, 15,
            7, 62, 54, 46, 38, 30, 22,
            14, 6, 61, 53, 45, 37, 29,
            21, 13, 5, 28, 20, 12, 4
        };

        static int[] pc2_perm = new int[48]
        {
            14, 17, 11, 24, 1, 5,
            3, 28, 15, 6, 21, 10,
            23, 19, 12, 4, 26, 8,
            16, 7, 27, 20, 13, 2,
            41, 52, 31, 37, 47, 55,
            30, 40, 51, 45, 33, 48,
            44, 49, 39, 56, 34, 53,
            46, 42, 50, 36, 29, 32
        };

        static int[] rotate = new int[16]
        {
            1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1
        };

        static int[] ip_perm = new int[64]
        {
            58, 50, 42, 34, 26, 18, 10, 2,
            60, 52, 44, 36, 28, 20, 12, 4,
            62, 54, 46, 38, 30, 22, 14, 6,
            64, 56, 48, 40, 32, 24, 16, 8,
            57, 49, 41, 33, 25, 17, 9, 1,
            59, 51, 43, 35, 27, 19, 11, 3,
            61, 53, 45, 37, 29, 21, 13, 5,
            63, 55, 47, 39, 31, 23, 15, 7
        };

        static int[] ip1_perm = new int[64]
        {
            40, 8, 48, 16, 56, 24, 64, 32,
            39, 7, 47, 15, 55, 23, 63, 31,
            38, 6, 46, 14, 54, 22, 62, 30,
            37, 5, 45, 13, 53, 21, 61, 29,
            36, 4, 44, 12, 52, 20, 60, 28,
            35, 3, 43, 11, 51, 19, 59, 27,
            34, 2, 42, 10, 50, 18, 58, 26,
            33, 1, 41, 9, 49, 17, 57, 25
        };

        static int[] E_perm = new int[48]
        {
            32, 1, 2, 3, 4, 5,
            4, 5, 6, 7, 8, 9,
            8, 9, 10, 11, 12, 13,
            12, 13, 14, 15, 16, 17,
            16, 17, 18, 19, 20, 21,
            20, 21, 22, 23, 24, 25,
            24, 25, 26, 27, 28, 29,
            28, 29,30, 31, 32, 1
        };

        static int[] P_perm = new int[32]
        {
            16, 7, 20, 21,
            29, 12, 28, 17,
            1, 15, 23, 26,
            5, 18, 31, 10,
            2, 8, 24, 14,
            32, 27, 3, 9,
            19, 13, 30, 6,
            22, 11, 4, 25
        };

        static int[][] s1 = new int[4][]
        {
            new int[16] { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 },
            new int[16] { 0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9,  5, 3, 8 },
            new int[16] { 4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9,  7, 3, 10, 5, 0 },
            new int[16] { 15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13 }
        };

        static int[][] s2 = new int[4][]
        {
            new int[16] { 15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10 },
            new int[16] { 3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5 },
            new int[16] { 0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15 },
            new int[16] { 13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9 }
        };

        static int[][] s3 = new int[4][]
        {
            new int[16] { 10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8 },
            new int[16] { 13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1 },
            new int[16] { 13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7 },
            new int[16] { 1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12 }
        };

        static int[][] s4 = new int[4][]
        {
            new int[16] { 7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15 },
            new int[16] { 13,  8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9 },
            new int[16] { 10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4 },
            new int[16] { 3, 15,0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14 }
        };

        static int[][] s5 = new int[4][]
        {
            new int[16] { 2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9 },
            new int[16] { 14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6 },
            new int[16] { 4, 2, 1, 11, 10,13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14 },
            new int[16] { 11, 8,12, 7, 1, 14, 2, 13, 6, 15, 0, 9, 10, 4, 5, 3 }
        };

        static int[][] s6 = new int[4][]
        {
            new int[16] { 12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11 },
            new int[16] { 10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8 },
            new int[16] { 9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6 },
            new int[16] { 4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13 }
        };

        static int[][] s7 = new int[4][]
        {
            new int[16] { 4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1 },
            new int[16] { 13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6 },
            new int[16] { 1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0,5, 9, 2 },
            new int[16] { 6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12 }
        };

        static int[][] s8 = new int[4][]
        {
            new int[16] { 13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7 },
            new int[16] { 1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2 },
            new int[16] { 7, 11, 4,1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8 },
            new int[16] { 2, 1, 14, 7, 4, 10, 8, 13, 15, 12, 9, 0, 3, 5, 6, 11 }
        };
    }
}
