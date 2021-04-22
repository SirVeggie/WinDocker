using System;

namespace Apprentice.Tools {

    public delegate double Curve(double t);

    public static class Curves {
        private const double c1 = 1.70158;
        private const double c2 = c1 * 1.525;
        private const double c3 = c1 + 1;
        private const double c4 = 2 * Math.PI / 3;
        private const double n1 = 7.5625;
        private const double d1 = 2.75;

        /// <summary>Straight curve</summary>
        public static double Linear(double t) => t;

        /// <summary>A t^2 curve</summary>
        public static double Quad(double t) => Math.Pow(t, 2);
        /// <summary>A flipped t^2 curve</summary>
        public static double FQuad(double t) => 1 - Math.Pow(1 - t, 2);
        /// <summary>A two way t^2 curve</summary>
        public static double BiQuad(double t) => t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;

        /// <summary>A t^3 curve</summary>
        public static double Cubic(double t) => Math.Pow(t, 3);
        /// <summary>A flipped t^3 curve</summary>
        public static double FCubic(double t) => 1 - Math.Pow(1 - t, 3);
        /// <summary>A two way t^3 curve</summary>
        public static double BiCubic(double t) => t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;

        /// <summary>A t^4 curve</summary>
        public static double Quart(double t) => Math.Pow(t, 4);
        /// <summary>A flipped t^4 curve</summary>
        public static double FQuart(double t) => 1 - Math.Pow(1 - t, 4);
        /// <summary>A two way t^4 curve</summary>
        public static double BiQuart(double t) => t < 0.5 ? 8 * t * t * t * t : 1 - Math.Pow(-2 * t + 2, 4) / 2;

        public static double Expo(double t) => t == 0 ? 0 : Math.Pow(2, 10 * t - 10);
        public static double FExpo(double t) => t == 1 ? 1 : 1 - Math.Pow(2, -10 * t);
        public static double BiExpo(double t) => t == 0 ? 0 : t == 1 ? 1 : t < 0.5 ? Math.Pow(2, 20 * t - 10) / 2 : (2 - Math.Pow(2, -20 * t + 10)) / 2;

        public static double Circ(double t) => 1 - Math.Sqrt(1 - Math.Pow(t, 2));
        public static double FCirc(double t) => Math.Sqrt(1 - Math.Pow(t - 1, 2));
        public static double BiCirc(double t) => t < 0.5 ? (1 - Math.Sqrt(1 - Math.Pow(2 * t, 2))) / 2 : (Math.Sqrt(1 - Math.Pow(-2 * t + 2, 2)) + 1) / 2;

        public static double Elastic(double t) => t == 0 ? 0 : t == 1 ? 1 : -Math.Pow(2, 10 * t - 10) * Math.Sin((t * 10 - 10.75) * c4);
        public static double FElastic(double t) => t == 0 ? 0 : t == 1 ? 1 : Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * c4) + 1;

        public static double Back(double t) => c3 * t * t * t - c1 * t * t;
        public static double FBack(double t) => 1 + c3 * Math.Pow(t - 1, 3) + c1 * Math.Pow(t - 1, 2);
        public static double BiBack(double t) => t < 0.5 ? (Math.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2 : (Math.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;

        public static double BackSharp(double t) => 1 - (Math.Pow(t, -t) - t);
        public static double FBackSharp(double t) => Math.Pow(1 - t, -(1 - t)) - (1 - t);

        public static double Bounce(double t) {
            if (t < 1 / d1) {
                return n1 * t * t;
            } else if (t < 2 / d1) {
                return n1 * (t -= 1.5 / d1) * t + 0.75;
            } else if (t < 2.5 / d1) {
                return n1 * (t -= 2.25 / d1) * t + 0.9375;
            } else {
                return n1 * (t -= 2.625 / d1) * t + 0.984375;
            }
        }

        public static class Custom {
            /// <summary>Returns a curve that always gives a constant value</summary>
            public static Curve Constant(double returnValue) => t => returnValue;
            /// <summary>Returns a curve of the form t^power</summary>
            public static Curve Power(double power) => t => Math.Pow(t, power);
            /// <summary>Returns a flipped curve of the form t^power</summary>
            public static Curve FPower(double power) => t => 1 - Math.Pow(1 - t, power);
            public static Curve Average(Curve a, Curve b) => t => (a(t) + b(t)) / 2;
            public static Curve Lerp(Curve a, Curve b, double weight) => t => a(t) * (1 - weight) + b(t) * weight;
        }
    }
}
