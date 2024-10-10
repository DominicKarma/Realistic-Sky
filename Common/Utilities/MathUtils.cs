using System;
using Terraria;

namespace RealisticSky.Common.Utilities
{
    public static class MathUtils
    {
        /// <summary>
        /// Clamps a given number between 0 and 1.
        /// </summary>
        /// <param name="x">The number to clamp.</param>
        public static float Saturate(float x)
        {
            if (x > 1f)
                return 1f;
            if (x < 0f)
                return 0f;
            return x;
        }

        /// <summary>
        ///     Performs a linear bump across a spectrum of two in/out values.
        /// </summary>
        /// <param name="start1">The value at which the output should rise from 0 to 1.</param>
        /// <param name="start2">The value at which the output start bumping at 1.</param>
        /// <param name="end1">The value at which the output cease bumping at 1.</param>
        /// <param name="end2">The value at which the output should descent from 1 to 0.</param>
        /// <param name="x">The input interpolant.</param>
        /// <returns>
        ///     0 when <paramref name="x"/> is less than or equal to <paramref name="start1"/>.
        ///     <br></br>
        ///     Anywhere between 0 and 1, ascending, when <paramref name="x"/> is greater than <paramref name="start1"/> but less than <paramref name="start2"/>.
        ///     <br></br>
        ///     1 when <paramref name="x"/> is between <paramref name="start2"/> and <paramref name="end1"/>.
        ///     <br></br>
        ///     Anywhere between 0 and 1, descending, when <paramref name="x"/> is greater than <paramref name="end1"/> but less than <paramref name="end2"/>.
        ///     <br></br>
        ///     1 when <paramref name="x"/> is greater than or equal to <paramref name="end2"/>.
        /// </returns>
        public static float InverseLerpBump(float start1, float start2, float end1, float end2, float x)
        {
            return Utils.GetLerpValue(start1, start2, x, true) * Utils.GetLerpValue(end2, end1, x, true);
        }

        /// <summary>
        /// Gives the <b>real</b> modulo of a divided by a divisor.
        /// This method is necessary because the % operator in C# keeps the sign of the dividend.
        /// </summary>
        public static float Modulo(this float dividend, float divisor)
        {
            return dividend - (float)Math.Floor(dividend / divisor) * divisor;
        }
    }
}
