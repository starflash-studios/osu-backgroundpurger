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
    public static class StringExtensions {
        /// <summary><inheritdoc cref="string.IsNullOrEmpty(string)"/></summary>
        /// <param name="String"><inheritdoc cref="string.IsNullOrEmpty(string)" path="value"/></param>
        /// <returns><inheritdoc cref="string.IsNullOrEmpty(string)" path="returns"/></returns>
        public static bool IsNullOrEmpty(this string String) => string.IsNullOrEmpty(String);

        /// <summary><inheritdoc cref="string.IsNullOrWhiteSpace(string)"/></summary>
        /// <param name="String"><inheritdoc cref="string.IsNullOrWhiteSpace(string)" path="value"/></param>
        /// <returns><inheritdoc cref="string.IsNullOrWhiteSpace(string)" path="returns"/></returns>
        public static bool IsNullOrWhiteSpace(this string String) => string.IsNullOrWhiteSpace(String);

        /// <summary>Removes all trailing occurrences of the given phrase from the current <see cref="string"/>.</summary>
        /// <param name="String">The string to trim.</param>
        /// <param name="Trim">The phrase to remove.</param>
        /// <param name="ComparisonType">Specifies how the phrases will be detected and trimmed.</param>
        /// <returns>The string that remains after all occurrences of the phrases specified in the <paramref name="Trim" /> parameter are removed from the end of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
        public static string TrimEnd(this string String, string Trim, StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase) => String.EndsWith(Trim, ComparisonType) ? String.Substring(0, String.Length - Trim.Length) : String;

        /// <summary>Removes all leading occurrences of the given phrase from the current <see cref="string"/>.</summary>
        /// <param name="String"><inheritdoc cref="TrimEnd(string, string, StringComparison)" path="String"/></param>
        /// <param name="Trim"><inheritdoc cref="TrimEnd(string, string, StringComparison)" path="Trim"/></param>
        /// <param name="ComparisonType"><inheritdoc cref="TrimEnd(string, string, StringComparison)" path="ComparisonType"/></param>
        /// <returns>The string that remains after all occurrences of the phrases specified in the <paramref name="Trim" /> parameter are removed from the start of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
        public static string TrimStart(this string String, string Trim, StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase) => String.StartsWith(Trim, ComparisonType) ? String.Substring(Trim.Length) : String;

        /// <summary>Removes all trailing and leading occurrences of the given phrase from the current <see cref="string"/>.</summary>
        /// <param name="String"><inheritdoc cref="TrimEnd(string, string, StringComparison)" path="String"/></param>
        /// <param name="Trim"><inheritdoc cref="TrimEnd(string, string, StringComparison)" path="Trim"/></param>
        /// <param name="ComparisonType"><inheritdoc cref="TrimEnd(string, string, StringComparison)" path="ComparisonType"/></param>
        /// <returns>The string that remains after all occurrences of the phrases specified in the <paramref name="Trim" /> parameter are removed from the start and end of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
        public static string Trim(this string String, string Trim, StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase) => String.TrimStart(Trim, ComparisonType).TrimEnd(Trim, ComparisonType);
    }
}
