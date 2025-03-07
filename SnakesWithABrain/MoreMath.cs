using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakesWithABrain
{
    /// <summary>
    /// Other Math function i decided i needed.
    /// </summary>
    public static class MoreMath
    {
        public static double Hypotenuse(int x, int y)
        {
            double returnMe = 0;
            x *= x;
            y *= y;
            returnMe = Math.Sqrt(x + y);
            return returnMe;
        }

        public static double Hypotenuse(float x, float y)
        {
            double returnMe = 0;
            x *= x;
            y *= y;
            returnMe = Math.Sqrt(x + y);
            return returnMe;
        }

        public static double Hypotenuse(double x, double y)
        {
            double returnMe = 0;
            x *= x;
            y *= y;
            returnMe = Math.Sqrt(x + y);
            return returnMe;
        }

        /// <summary>
        /// Applies Sigmoid activation to a 1 dimensional array.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double[] MatrixSigmoid(double[] m)
        {
            double[] returnMe = new double[m.Length];
            for (int x = 0; x < m.Length; x++)
            {
                returnMe[x] = SigmoidActivation(m[x]);
            }

            return returnMe;
        }

        /// <summary>
        /// Applies Tanh activation to a 1 dimensional array.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double[] MatrixTanh(double[] m)
        {
            double[] returnMe = new double[m.Length];
            for (int x = 0; x < m.Length; x++)
            {
                returnMe[x] = TanhActivation(m[x]);
            }

            return returnMe;
        }
        public static double SigmoidActivation(double value)
        {
            if (value < -10)
            {
                return 0f;
            }
            else if (value > 10)
            {
                return 1f;
            }
            else
            {
                return 1.0f / (1.0f + (double)(System.Math.Exp(-value)));
            }
        }

        public static double TanhActivation(double x)
        {
            if (x < -10.0)
            {
                return -1.0f;
            }
            else if (x > 10.0)
            {
                return 1.0f;
            }

            return (double)(Math.Tanh(x));
        }

        /// <summary>
        /// Multiplies a 2 dimensional array by a 1 dimensional array
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double[] MatrixMultiply(double[,] a, double[] b)
        {
            double[] returnMe = new double[a.GetLongLength(0)];

            int lastX = 0;
            int lastY = 0;
            for (int x = 0; x < a.GetLongLength(0); x++) //will cycle 1-3
            {
                lastX = x;
                for (int y = 0; y < a.GetLongLength(1); y++)//will cycle 1-5
                {
                    lastY = y;
                    returnMe[x] += a[x, y] * b[y];
                }

            }

            return returnMe;
        }

        /// <summary>
        /// Adds 2 1 dimensional arrays together.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double[] MatrixAdd(double[] a, double[] b)
        {
            double[] returnMe = new double[a.Length];
            for (int x = 0; x < a.Length; x++)
            {
                returnMe[x] = a[x] + b[x];
            }

            return returnMe;
        }

        /// <summary>
        /// Mashes 2 1 dimensional arrays into a single 1 dimensional array
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double[] MatrixConcat(double[] a, double[] b)
        {
            double[] returnMe = new double[a.Length + b.Length];

            a.CopyTo(returnMe, 0);
            b.CopyTo(returnMe, b.Length);
            return returnMe;
        }

        /// <summary>
        /// Performs Hadamard product on 2 1 dimensional arrays. (Element wise multiplication)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double[] MatrixHadamard(double[] a, double[] b)
        {
            double[] returnMe = new double[a.Length];

            for (int x = 0; x < a.Length; x++)
            {
                returnMe[x] = a[x] * b[x];
            }

            return returnMe;
        }

    }
}
