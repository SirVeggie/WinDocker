using System;

namespace Apprentice.Tools {
    public static class BitControl {

        /// <summary>Get the specified portion as an integer. Zero based.</summary>
        /// <param name="end">The end parameter is exclusive.</param>
        public static int Slice(int source, int start, int end = 32) {
            int mask = GenerateMask(start, end);
            return (source & mask) >> start;
        }

        /// <summary>Get a bit at a specific position. Zero based.</summary>
        public static int GetBit(int source, int index) => (source >> index) & 1;
        /// <summary>Get a bit at a specific position as a boolean. Zero based.</summary>
        public static bool GetBitBool(int source, int index) => GetBit(source, index) != 0;
        public static int GenerateMask(int start, int end) => GenerateMask(end - start) << start;
        public static int GenerateMask(int size) {
            size = Math.Max(0, size);
            return (int) (uint) Math.Pow(2, size) - 1;
        }

        public static string AsBinaryString(uint n) => AsBinaryString((int) n);
        public static string AsBinaryString(int n) {
            char[] b = new char[32];
            int pos = 31;
            int i = 0;

            while (i < 32) {
                if ((n & (1 << i)) != 0) {
                    b[pos] = '1';
                } else {
                    b[pos] = '0';
                }
                pos--;
                i++;
            }
            return new string(b);
        }

        public static string AsBinaryString(ulong n) {
            char[] b = new char[64];
            int pos = 63;
            int i = 0;

            while (i < 64) {
                if ((n & ((ulong) 1 << i)) != 0) {
                    b[pos] = '1';
                } else {
                    b[pos] = '0';
                }
                pos--;
                i++;
            }
            return new string(b);
        }

        public static int CountBits(long number) {
            if (number == 0) return 1;
            return (int) Math.Log(Math.Abs(number), 2) + 1;
        }
    }
}
