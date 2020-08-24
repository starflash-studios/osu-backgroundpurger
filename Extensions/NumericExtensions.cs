#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System;

#endregion

namespace Osu_BackgroundPurge {
    /// <summary>A collection of numerous extensions for numeral-based variables.</summary>
    public static class NumericExtensions {

        #region Double / Long

        /// <summary><inheritdoc cref="Math.Ceiling(double)"/></summary>
        /// <param name="D"><inheritdoc cref="Math.Ceiling(double)" path="a"/></param>
        /// <returns><inheritdoc cref="Math.Ceiling(double)" path="returns"/></returns>
        public static double Ceil(this double D) => Math.Ceiling(D);

        /// <summary><inheritdoc cref="Math.Floor(double)"/></summary>
        /// <param name="D"><inheritdoc cref="Math.Floor(double)" path="d"/></param>
        /// <returns><inheritdoc cref="Math.Floor(double)" path="returns"/></returns>
        public static double Floor(this double D) => Math.Floor(D);

        /// <summary><inheritdoc cref="Math.Round(double, int)"/></summary>
        /// <param name="D"><inheritdoc cref="Math.Round(double, int)" path="value"/></param>
        /// <param name="Digits"><inheritdoc cref="Math.Round(double, int)" path="digits"/></param>
        /// <returns><inheritdoc cref="Math.Round(double, int)" path="returns"/></returns>
        public static double Round(this double D, int Digits = 0) => Math.Round(D, Digits);

        /// <summary><inheritdoc cref="Math.Ceiling(double)"/> Value is then returned in the form <see cref="long"/>.</summary>
        /// <param name="D"><inheritdoc cref="Math.Ceiling(double)" path="a"/></param>
        /// <returns><inheritdoc cref="Math.Ceiling(double)" path="returns"/></returns>
        public static long CeilToWhole(this double D) => (long)Math.Ceiling(D);

        /// <summary><inheritdoc cref="Math.Floor(double)"/> Value is then returned in the form <see cref="long"/>.</summary>
        /// <param name="D"><inheritdoc cref="Math.Floor(double)" path="d"/></param>
        /// <returns><inheritdoc cref="Math.Floor(double)" path="returns"/></returns>
        public static long FloorToWhole(this double D) => (long)Math.Floor(D);

        /// <summary><inheritdoc cref="Math.Round(double, int)"/> Value is then returned in the form <see cref="long"/>.</summary>
        /// <param name="D"><inheritdoc cref="Math.Round(double, int)" path="value"/></param>
        /// <param name="Digits"><inheritdoc cref="Math.Round(double, int)" path="digits"/></param>
        /// <returns><inheritdoc cref="Math.Round(double, int)" path="returns"/></returns>
        public static long RoundToWhole(this double D, int Digits = 0) => (long)Math.Round(D, Digits);

        #endregion

        #region Float / Int
        
        /// <summary>Returns the smallest integral value that is greater than or equal to the specified floating-point number.</summary>
        /// <param name="D">A floating-point number.</param>
        /// <returns>The smallest integral value that is greater than or equal to <paramref name="D" />. If <paramref name="D" /> is equal to <see cref="F:System.Int32.NaN" />, <see cref="F:System.Int32.NegativeInfinity" />, or <see cref="F:System.Int32.PositiveInfinity" />, that value is returned. Note that this method returns a <see cref="T:System.Int32" /> instead of an integral type.</returns>
        public static float Ceil(this float D) => (float)Math.Ceiling(D);

        /// <summary>Returns the largest integral value less than or equal to the specified floating-point number.</summary>
        /// <param name="D">A floating-point number.</param>
        /// <returns>The largest integral value less than or equal to <paramref name="D" />. If <paramref name="D" /> is equal to <see cref="F:System.Int32.NaN" />, <see cref="F:System.Int32.NegativeInfinity" />, or <see cref="F:System.Int32.PositiveInfinity" />, that value is returned.</returns>
        public static float Floor(this float D) => (float)Math.Floor(D);

        /// <summary>Rounds a floating-point value to a specified number of fractional digits, and rounds midpoint values to the nearest even number.</summary>
        /// <param name="D">A floating-point number to be rounded.</param>
        /// <param name="Digits">The number of fractional digits in the return value.</param>
        /// <returns>The number nearest to <paramref name="D" /> that contains a number of fractional digits equal to <paramref name="Digits" />.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="Digits" /> is less than 0 or greater than 15.</exception>
        public static float Round(this float D, int Digits = 0) => (float)Math.Round(D, Digits);

        /// <summary><inheritdoc cref="Ceil(float)"/> Value is then returned in the form <see cref="int"/>.</summary>
        /// <param name="D"><inheritdoc cref="Ceil(float)" path="D"/></param>
        /// <returns><inheritdoc cref="Ceil(float)" path="returns"/></returns>
        public static int CeilToWhole(this float D) => (int)Math.Ceiling(D);

        /// <summary><inheritdoc cref="Floor(float)"/> Value is then returned in the form <see cref="int"/>.</summary>
        /// <param name="D"><inheritdoc cref="Floor(float)" path="D"/></param>
        /// <returns><inheritdoc cref="Floor(float)" path="returns"/></returns>
        public static int FloorToWhole(this float D) => (int)Math.Floor(D);

        /// <summary><inheritdoc cref="Round(float, int)"/> Value is then returned in the form <see cref="int"/>.</summary>
        /// <param name="D"><inheritdoc cref="Round(float, int)" path="D"/></param>
        /// <param name="Digits"><inheritdoc cref="Round(float, int)" path="Digits"/></param>
        /// <returns><inheritdoc cref="Round(float, int)" path="returns"/></returns>
        public static int RoundToWhole(this float D, int Digits = 0) => (int)Math.Round(D, Digits);

        /// <summary>Returns the absolute (positive) value of a floating-point number.</summary>
        /// <param name="A">A number that is greater than or equal to <see cref="float.MinValue" />, but less than or equal to <see cref="float.MaxValue" />.</param>
        /// <returns>A floating-point number, 𝑥, such that 0 ≤ 𝑥 ≤ <see cref="float.MaxValue" />.</returns>
        public static float Positive(this float A) => Math.Abs(A);

        /// <summary>Returns the negative absolute value of a floating-point number.</summary>
        /// <param name="A">A number that is greater than or equal to <see cref="float.MinValue" />, but less than or equal to <see cref="float.MaxValue" />.</param>
        /// <returns>A floating-point number, 𝑥, such that <see cref="float.MinValue"/> ≤ 𝑥 ≤ 0.</returns>
        public static float Negative(this float A) => -Math.Abs(A);

        /// <summary>Returns the absolute (positive) value of a 32-bit signed integer.</summary>
        /// <param name="A">A number that is greater than or equal to <see cref="int.MinValue" />, but less than or equal to <see cref="int.MaxValue" />.</param>
        /// <returns>A 32-bit signed integer, 𝑥, such that 0 ≤ 𝑥 ≤ <see cref="int.MaxValue" />.</returns>
        public static int Positive(this int A) => Math.Abs(A);

        /// <summary>Returns the negative absolute value of a 32-bit signed integer.</summary>
        /// <param name="A">A number that is greater than or equal to <see cref="int.MinValue" />, but less than or equal to <see cref="int.MaxValue" />.</param>
        /// <returns>A 32-bit signed integer, 𝑥, such that <see cref="int.MinValue"/> ≤ 𝑥 ≤ 0.</returns>
        public static int Negative(this int A) => -Math.Abs(A);

        #endregion

    }
}
