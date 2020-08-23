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
using System.Diagnostics;

#endregion

namespace Osu_BackgroundPurge.OsuParser {
    public static class OsuBeatmapExtensions {

        #region Enum Flags

        public static IEnumerable<Enum> GetFlags(this Enum EnumFlags) => from Enum E in Enum.GetValues(EnumFlags.GetType()) where EnumFlags.HasFlag(E) select E;

        public static IEnumerable<string> GetFlagNames(this Enum EnumFlags) => from Enum E in Enum.GetValues(EnumFlags.GetType()) where EnumFlags.HasFlag(E) select E.ToString();

        public static string SummariseFlags(this Enum EnumFlags, string Delimeter = ", ") => EnumFlags.GetFlagNames().GetListedString(Delimeter);

        #endregion

        #region List Strings

        public static string GetListedString<T>(this IEnumerable<T> List, string Delimeter = ", ") => List != null ? string.Join(Delimeter, List) : string.Empty;

        #endregion

        #region String Conversions

        public static bool TryConvertToInt(this string Value, out int Result) => int.TryParse(Value, out Result);

        public static int ConvertToInt(this string Value, int Default = 0) => TryConvertToInt(Value, out int Result) ? Result : Default;

        public static bool TryConvertToDecimal(this string Value, out decimal Result) => decimal.TryParse(Value, out Result);

        public static decimal ConvertToDecimal(this string Value, decimal Default = 0M) => TryConvertToDecimal(Value, out decimal Result) ? Result : Default;

        public static bool ConvertToBool(this string Value, StringComparison StringComparison = StringComparison.InvariantCultureIgnoreCase) => Value.TryConvertToInt(out int IntValue) ? IntValue != 0 : Value.Equals("true", StringComparison);

        public static T ConvertToEnum<T>(this string Value, StringComparison ComparisonType) where T : Enum {
            Type EnumType = typeof(T);

            if (Value.TryConvertToInt(out int IntResult)) {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (int V in Enum.GetValues(EnumType)) {
                    if (V == IntResult) {
                        // https://stackoverflow.com/a/10387134/11519246
                        return (T)(object)IntResult;
                    }
                }
            } else {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach(string N in Enum.GetNames(EnumType)) {
                    if (N.Equals(Value, ComparisonType)) {
                        return (T)Enum.Parse(EnumType, Value);
                    }
                }
            }

            Debug.WriteLine("WARNING", $"Value '{Value}' was unable to be converted to enum of form {EnumType}.");
            return default;
        }

        public static T ConvertToEnum<T>(this string Value, T Default = default, bool IgnoreCase = true) where T : struct => Enum.TryParse(Value, IgnoreCase, out T Result) ? Result : Default;

        public static (int X, int Y) ConvertToPoint(this string Value, (int X, int Y) Default, StringSplitOptions Options = StringSplitOptions.RemoveEmptyEntries) {
            (int DX, int DY) = Default;
            int X = DX;
            int Y = DY;

            int I = 0;
            foreach(string Val in Value.SeparateColonList(Options)) {
                switch (I) {
                    case 0:
                        X = Val.ConvertToInt(DX);
                        break;
                    case 1:
                        Y = Val.ConvertToInt(DY);
                        break;
                    default:
                        Debug.WriteLine("WARNING", $"Point '{Value}' contained unexpected amount of values to consider. ");
                        I = -1;
                        break;
                }
                if (I < 0) { break; }
                I++;
            }

            return (X, Y);
        }

        public static RelativeFileInfo ConvertToRelativeFileInfo(this string Value) => new RelativeFileInfo(Value);

        #endregion

        #region Split List

        public static IEnumerable<string> Split(this string Value, StringSplitOptions Options, params char[] SplitChars) {
            switch (Options) {
                case StringSplitOptions.None:
                    foreach(string S in Value.Split(SplitChars)) {
                        yield return S;
                    }
                    break;
                case StringSplitOptions.RemoveEmptyEntries:
                    if (!string.IsNullOrEmpty(Value)) {
                        // ReSharper disable once LoopCanBePartlyConvertedToQuery
                        foreach(string T in Value.Split(SplitChars)) {
                            if (!string.IsNullOrEmpty(T)) {
                                yield return T;
                            }
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Options), Options, null);
            }
        }

        public static IEnumerable<string> SeparateCommaList(this string Value, StringSplitOptions Options = StringSplitOptions.RemoveEmptyEntries) => Value.Split(Options, ',').Select(Split => Split.Trim(' '));

        public static IEnumerable<string> SeparateSpaceList(this string Value, StringSplitOptions Options = StringSplitOptions.RemoveEmptyEntries) => Value.Split(Options, ' ');

        public static IEnumerable<string> SeparateColonList(this string Value, StringSplitOptions Options = StringSplitOptions.RemoveEmptyEntries) => Value.Split(Options, ':').Select(Split => Split.Trim(' '));

        public static IEnumerable<string> SeparatePipeList(this string Value, StringSplitOptions Options = StringSplitOptions.RemoveEmptyEntries) => Value.Split(Options, '|').Select(Split => Split.Trim(' '));

        #endregion
    }
}