using System;

namespace Apprentice.Tools {
    public static class Matht {

        #region clamping
        public static double Clamp(double value, double min, double max) => Math.Min(Math.Max(value, min), max);
        public static float Clamp(float value, float min, float max) => Math.Min(Math.Max(value, min), max);
        public static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);

        /// <summary></summary>
        /// <param name="max">Max is inclusive.</param>
        public static int CyclicalClampInclusive(int x, int min, int max) => FloorToInt(CyclicalClampInclusive((double) x, min, max));
        /// <summary></summary>
        /// <param name="max">Max is inclusive.</param>
        public static double CyclicalClampInclusive(double x, double min, double max) {
            if (min > max) {
                throw new ArgumentException("Min can't be bigger than max");
            }

            double delta = Math.Abs(x - Clamp(x, min, max));
            double range = max - min;

            if (range == 0) {
                return max;
            } else if (x > max) {
                return min + (((delta - 1) % range) + 1);
            } else if (x < min) {
                return max - (((delta - 1) % range) + 1);
            } else {
                return x;
            }
        }

        public static int CyclicalClamp(int x, int min, int max) => FloorToInt(CyclicalClamp((double) x, min, max));
        public static double CyclicalClamp(double x, double min, double max) {
            if (min == max)
                return min;
            if (min > max)
                throw new ArgumentException("Min can't be bigger than max");
            double range = max - min;
            return (((x - min) % range) + range) % range + min;
        }
        #endregion

        public static int RoundToInt(double value) => (int) Math.Round(value);
        public static int FloorToInt(double value) => (int) Math.Floor(value);
        public static int CeilToInt(double value) => (int) Math.Ceiling(value);
        public static double Percentage(double value, double min, double max) => (value - min) / (max - min);
        public static double ToRange(double value, double low, double high, double newLow, double newHigh) => newLow + (newHigh - newLow) * Percentage(value, low, high);

        #region trigonometry
        /// <summary>Converts degrees to radians.</summary>
        /// <param name="d">Angle in degrees</param>
        public static double Radians(double d) => Math.PI / 180 * d;

        /// <summary>Converts radians to degrees.</summary>
        /// <param name="r">Angle in radians</param>
        public static double Degrees(double r) => 180 / Math.PI * r;
        #endregion
    }
}
