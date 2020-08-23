#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System;
using System.Linq;
using System.Collections.Generic;

#endregion

namespace Osu_BackgroundPurge {
    public static class EnumerableExtensions {

        /// <summary>Converts to enum.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Value">The value.</param>
        /// <param name="Default">The default.</param>
        /// <param name="IgnoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns><see cref="T"/></returns>
        public static T ConvertToEnum<T>(this string Value, T Default = default, bool IgnoreCase = true) where T : struct => Enum.TryParse(Value, IgnoreCase, out T Result) ? Result : Default;

        /// <summary>Converts to enum.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Value">The value.</param>
        /// <param name="Default">The default.</param>
        /// <returns><see cref="T"/></returns>
        public static T ConvertToEnum<T>(this int Value, T Default = default) where T : struct => Value.ToString().ConvertToEnum(Default, true);

        /// <summary> Tries to add the given <paramref name="Value"/>, returning true if successful. </summary>
        /// <returns><see cref="System.Boolean"/></returns>
        public static bool TryAdd<T>(this HashSet<T> HashSet, T Value) {
            if (!HashSet.Contains(Value)) {
                HashSet.Add(Value);
                return true;
            }

            return false;
        }

        /// <summary>Gets a random element from the given <paramref name="Array"/>.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Array">The array.</param>
        /// <param name="R">The randomiser to use. If set to <c>null</c> [new Random()].</param>
        /// <returns><see cref="T"/></returns>
        public static T GetRandom<T>(this T[] Array, Random R = null) => Array[(R ?? new Random()).Next(0, Array.Length)];

        /// <summary>Gets a random element from the given <paramref name="Enum"/>.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Enum">The array.</param>
        /// <param name="R">The randomiser to use.</param>
        /// <returns><see cref="T"/></returns>
        public static T GetRandom<T>(this IEnumerable<T> Enum, Random R = null) => Enum.ToArray().GetRandom(R);

        /// <summary>Gets a random element from the given <paramref name="List"/>.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="List">The array.</param>
        /// <param name="R">The randomiser to use.</param>
        /// <returns><see cref="T"/></returns>
        public static T GetRandom<T>(this List<T> List, Random R = null) => List[(R ?? new Random()).Next(0, List.Count)];

        /// <summary>Returns the count of values in the given enum type.</summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <returns><see cref="int"/></returns>
        public static int Length<T>() where T : struct, Enum => Length(default(T));

#pragma warning disable IDE0060 // Remove unused parameter // Parameter is kept to ensure method is extension
        /// <summary>Returns the count of values in the given enum type.</summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <returns><see cref="int"/></returns>
        public static int Length<T>(this T E) where T : struct, Enum => Enum.GetNames(typeof(T)).Length;
#pragma warning restore IDE0060 // Remove unused parameter

        /// <summary>Returns the summary of active flags.</summary>
        /// <param name="Value">The flagged enum.</param>
        /// <param name="Delimiter">The delimiter to separate flags.</param>
        /// <returns><see cref="System.String"/></returns>
        public static string GetFlagsSummary(this Enum Value, string Delimiter = ", ") => string.Join(Delimiter, Value.GetFlags());

        /// <summary>Returns the active flags.</summary>
        /// <param name="Input">The flagged enum.</param>
        /// <returns><see cref="IEnumerable{Enum}"/></returns>
        public static IEnumerable<Enum> GetFlags(this Enum Input) => Enum.GetValues(Input.GetType()).Cast<Enum>().Where(Input.HasFlag);

        /// <summary>Returns the enumerable with the smallest value within the enum.</summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="Value">The value.</param>
        /// <returns><typeparamref name="T"/></returns>
        public static T Min<T>(this T Value) where T : struct, Enum => Enum.GetValues(Value.GetType()).Cast<T>().Min();

        /// <summary>Returns the smallest value within the enum.</summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="Value">The value.</param>
        /// <returns><see cref="int"/></returns>
        public static int MinValue<T>(this T Value) where T : struct, Enum => Convert.ToInt32(Value.Min());

        /// <summary>Returns the enumerable with the largest value within the enum.</summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="Value">The value.</param>
        /// <returns><typeparamref name="T"/></returns>
        public static T Max<T>(this T Value) where T : struct, Enum => Enum.GetValues(Value.GetType()).Cast<T>().Max();

        /// <summary>Returns the largest value within the enum.</summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="Value">The value.</param>
        /// <returns><see cref="int"/></returns>
        public static int MaxValue<T>(this T Value) where T : struct, Enum => Convert.ToInt32(Value.Max());
    }
}
