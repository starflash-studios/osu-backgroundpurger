#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;

#endregion

namespace Osu_BackgroundPurge.OsuParser {
    public class RelativeFileInfo : RelativeFileSystemInfo, IEquatable<RelativeFileInfo> {

        public RelativeFileInfo(string BaseFile) : base(BaseFile) { }

        public bool TryGetAbsoluteFile(DirectoryInfo BaseDirectory, out FileInfo Result) => TryGetFileInfo($"{BaseDirectory.FullName}\\{RelativePath}", out Result);

        public static RelativeFileInfo GetRelativeFile(FileInfo File, DirectoryInfo HoldingDirectory) => new RelativeFileInfo(GetRelativePath(HoldingDirectory, File));

        public static RelativeFileInfo GetRelativeFile(string FileName) => new RelativeFileInfo(FileName);

        public static string GetPath(FileSystemInfo Fsi) => !(Fsi is DirectoryInfo D) ? Fsi.FullName : $"{D.FullName.TrimEnd(Path.DirectorySeparatorChar)}{Path.DirectorySeparatorChar}";

        public static string GetRelativePath(FileSystemInfo From, FileSystemInfo To) {
            //See: https://gist.github.com/mattjcowan/ae21ca6d9835c9630b362dd17141f863
            string FromPath = GetPath(From);
            string ToPath = GetPath(To);

            Uri FromUri = new Uri(FromPath);
            Uri ToUri = new Uri(ToPath);

            Uri RelativeUri = FromUri.MakeRelativeUri(ToUri);
            string RelativePath = Uri.UnescapeDataString(RelativeUri.ToString());

            return RelativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        public static bool TryGetFileInfo(string FilePath, out FileInfo Result) {
            //if (File.Exists(FilePath)) {
            try {
                Result = new FileInfo(FilePath);
                return Result != null;
            } catch (ArgumentNullException ANE) {
                Debug.WriteLine("ERROR", $"'{FilePath}' is null. {ANE}");
            } catch (SecurityException SE) {
                Debug.WriteLine("ERROR", $"The caller does not have the required permissions to access '{FilePath}'. {SE}");
            } catch (ArgumentException AE) {
                Debug.WriteLine("ERROR", $"The filename '{FilePath}' is empty, contains only white spaces, or contains invalid characters. {AE}");
            } catch (UnauthorizedAccessException UAE) {
                Debug.WriteLine("ERROR", $"Access to {FilePath} is denied. {UAE}");
            } catch (PathTooLongException PTLE) {
                Debug.WriteLine("ERROR", $"The specified filename, {FilePath}, exceeds the system-defined maximum length. {PTLE}");
            } catch (NotSupportedException NSE) {
                Debug.WriteLine("ERROR", $"{FilePath} contains a colon (:) in the middle of the string. {NSE}");
            }
            //} else {
            //    Debug.WriteLine($"File '{FilePath}' does not exist.");
            //}

            Result = null;
            return false;
        }

        public override string ToString() => $".\\{RelativePath}";

        #region Equality Members

        public override bool Equals(object Obj) => Equals(Obj as RelativeFileInfo);

        public override int GetHashCode() => base.GetHashCode();

        public bool Equals(RelativeFileInfo Other) =>
            Other != null &&
            base.Equals(Other) &&
            BasePath == Other.BasePath;

        public static bool operator ==(RelativeFileInfo Left, RelativeFileInfo Right) => EqualityComparer<RelativeFileInfo>.Default.Equals(Left, Right);

        public static bool operator !=(RelativeFileInfo Left, RelativeFileInfo Right) => !(Left == Right);

        #endregion
    }
}