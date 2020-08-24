#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;

#endregion

namespace Osu_BackgroundPurge {
    /// <summary>A <see cref="System.ComponentModel.TypeConverter"/> for converting <see cref="string"/> to <see cref="FileSystemInfo"/>, and vice versa.</summary>
    /// <seealso cref="System.ComponentModel.TypeConverter" />
    public class StringToFileSystemInfoTypeConverter : TypeConverter {
        /// <summary>Determines whether this instance [can convert from] the specified context.</summary>
        /// <param name="Context">The context.</param>
        /// <param name="SourceType">Type of the source.</param>
        /// <returns><c>True</c> if this instance [can convert from] the specified context; otherwise, <c>false</c>.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext Context, Type SourceType) => SourceType == typeof(FileInfo) || SourceType == typeof(DirectoryInfo) || SourceType == typeof(FileSystemInfo) || SourceType == typeof(string);

        /// <summary>Determines whether this instance [can convert to] the specified context.</summary>
        /// <param name="Context">The context.</param>
        /// <param name="DestinationType">Type of the destination.</param>
        /// <returns><c>True</c> if this instance [can convert to] the specified context; otherwise, <c>false</c>.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext Context, Type DestinationType) => DestinationType == typeof(FileInfo) || DestinationType == typeof(DirectoryInfo) || DestinationType == typeof(FileSystemInfo);

        /// <summary>Converts the given <paramref name="Value"/> to a conversion-friendly <see cref="object"/>.</summary>
        /// <param name="Context">The context.</param>
        /// <param name="Culture">The culture.</param>
        /// <param name="Value">The value.</param>
        /// <returns><see cref="object"/></returns>
        public override object ConvertFrom(ITypeDescriptorContext Context, CultureInfo Culture, object Value) =>
            Value switch {
                FileSystemInfo FSI => FSI,
                string Path when TryGetFileSystemInfo(Path, out FileSystemInfo Result) => Result,
                _ => null
            };

        /// <summary>Converts the given <paramref name="Value"/> to the requested <paramref name="DestinationType"/>.</summary>
        /// <param name="Context">The context.</param>
        /// <param name="Culture">The culture.</param>
        /// <param name="Value">The value.</param>
        /// <param name="DestinationType">The final return type.</param>
        /// <returns><see cref="object"/></returns>
        public override object ConvertTo(ITypeDescriptorContext Context, CultureInfo Culture, object Value, Type DestinationType) {
            string Path = Value.ToString();

            if (DestinationType == typeof(FileSystemInfo) && TryGetFileSystemInfo(Path, out FileSystemInfo Result)) {
                return Result;
            }

            if (DestinationType == typeof(FileInfo) && TryGetFileInfo(Path, out FileInfo File)) {
                return File;
            }

            if (DestinationType == typeof(DirectoryInfo) && TryGetDirectoryInfo(Path, out DirectoryInfo Dir)) {
                return Dir;
            }

            return base.ConvertTo(Context, Culture, Value, DestinationType);
        }

        /// <summary>Tries to convert the given <paramref name="Path"/> to a <see cref="FileInfo"/> / <see cref="DirectoryInfo"/>.</summary>
        /// <param name="Path">The path.</param>
        /// <param name="Result">The result.</param>
        /// <returns><see cref="bool"/></returns>
        public static bool TryGetFileSystemInfo(string Path, out FileSystemInfo Result) {
            FileAttributes Attr;
            try {
                Attr = File.GetAttributes(Path.Trim(' ').ToLowerInvariant());
                #pragma warning disable CA1031 // Do not catch general exception types
            } catch { //Called when given path is not valid
                #pragma warning restore CA1031 // Do not catch general exception types
                Result = null;
                return false;
            }

            switch (Attr) {
                case FileAttributes.Directory: //Path is (assumed) DirectoryInfo
                    if (TryGetDirectoryInfo(Path, out DirectoryInfo DirectoryResult)) {
                        Result = DirectoryResult;
                        return true;
                    }
                    break;
                default: //Path is (assumed) FileInfo
                    if (TryGetFileInfo(Path, out FileInfo FileResult)) {
                        Result = FileResult;
                        return true;
                    }
                    break;
            }

            Result = null;
            return false;
        }

        /// <summary>Tries to convert the given <paramref name="Path"/> to a <see cref="DirectoryInfo"/>.</summary>
        /// <param name="Path">The path.</param>
        /// <param name="Result">The result.</param>
        /// <returns><see cref="bool"/></returns>
        public static bool TryGetDirectoryInfo(string Path, out DirectoryInfo Result) {
            try {
                Result = new DirectoryInfo(Path);
                return Result != null;
                #pragma warning disable CA1031 // Do not catch general exception types
            } catch {
                #pragma warning restore CA1031 // Do not catch general exception types
                Result = null;
                return false;
            }
        }

        /// <summary>Tries to convert the given <paramref name="Path"/> to a <see cref="FileInfo"/>.</summary>
        /// <param name="Path">The path.</param>
        /// <param name="Result">The result.</param>
        /// <returns><see cref="bool"/></returns>
        public static bool TryGetFileInfo(string Path, out FileInfo Result) {
            try {
                Result = new FileInfo(Path);
                return Result != null;
                #pragma warning disable CA1031 // Do not catch general exception types
            } catch {
                #pragma warning restore CA1031 // Do not catch general exception types
                Result = null;
                return false;
            }
        }

        /// <summary>Returns the containing executable folder of the currently running <see cref="System.Windows.Application"/>.</summary>
        /// <returns><see cref="DirectoryInfo"/></returns>
        public static DirectoryInfo ExecutingLocation() => ExecutingApplication().Directory;

        /// <summary>Returns the executable of the currently running <see cref="System.Windows.Application"/>.</summary>
        /// <returns><see cref="FileInfo"/></returns>
        public static FileInfo ExecutingApplication() => new FileInfo(Assembly.GetExecutingAssembly().Location);

        /// <summary>Returns the location of the system desktop.</summary>
        /// <value>The desktop.</value>
        public static DirectoryInfo Desktop() => new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
    }
}