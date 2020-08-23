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

#endregion

namespace Osu_BackgroundPurge.OsuParser {

    #region RelativeFileSystemInfo<T>

    public abstract class RelativeFileSystemInfo : IEquatable<RelativeFileSystemInfo> {
        internal readonly string BasePath;
        public string RelativePath => BasePath;

        protected RelativeFileSystemInfo(string BasePath) => this.BasePath = BasePath.Trim(' ').Replace("\"", "");

        public override string ToString() => $".\\{RelativePath}";

        #region Equality Members

        public override bool Equals(object Obj) => Equals(Obj as RelativeFileSystemInfo);

        public override int GetHashCode() => BasePath != null ? BasePath.GetHashCode() : 0;

        public bool Equals(RelativeFileSystemInfo Other) =>
            Other != null &&
            BasePath == Other.BasePath &&
            RelativePath == Other.RelativePath;

        public static bool operator ==(RelativeFileSystemInfo Left, RelativeFileSystemInfo Right) => EqualityComparer<RelativeFileSystemInfo>.Default.Equals(Left, Right);

        public static bool operator !=(RelativeFileSystemInfo Left, RelativeFileSystemInfo Right) => !(Left == Right);

        #endregion
    }

    #endregion

    #region RelativeFileInfo : RelativeFileSystemInfo<FileInfo>

    #endregion

    #region RelativeDirectoryInfo : RelativeFileSystemInfo<DirectoryInfo>

    //public class RelativeDirectoryInfo : RelativeFileSystemInfo, IEquatable<RelativeDirectoryInfo> {

    //    public RelativeDirectoryInfo(string BaseFile) : base(BaseFile) { }

    //    public bool TryGetAbsoluteDirectory(DirectoryInfo BaseDirectory, out DirectoryInfo Result) => TryGetDirectoryInfo($"{BaseDirectory.FullName}\\{RelativePath}", out Result);

    //    public static bool TryGetRelativeFile(DirectoryInfo OriginalPath, DirectoryInfo RelativePath, out RelativeDirectoryInfo Result) {
    //        string NewPath = OriginalPath.FullName;
    //        string Root = RelativePath.FullName;
    //        if (NewPath.StartsWith(Root, StringComparison.InvariantCultureIgnoreCase)) {
    //            if (TryGetDirectoryInfo(NewPath.Substring(Root.Length), out DirectoryInfo FinalPath)) {
    //                Result = new RelativeDirectoryInfo(BaseFile: FinalPath);
    //                return true;
    //            }

    //            Debug.WriteLine("WARNING", $"An error occurred when parsing filepath '{NewPath.Substring(Root.Length)}'.");
    //            Result = default;
    //            return false;
    //        }

    //        throw new NotImplementedException($"Path '{NewPath}' is not relative to root '{Root}'");
    //    }

    //    public static bool TryGetDirectoryInfo(string DirectoryPath, out DirectoryInfo Result) {
    //        //if (Directory.Exists(FilePath)) {
    //            try {
    //                Result = new DirectoryInfo(DirectoryPath);
    //                return Result != null;
    //            } catch (ArgumentNullException ANE) {
    //                Debug.WriteLine("ERROR", $"'{DirectoryPath}' is null. {ANE}");
    //            } catch (SecurityException SE) {
    //                Debug.WriteLine("ERROR", $"The caller does not have the required permissions to access '{DirectoryPath}'. {SE}");
    //            } catch (ArgumentException AE) {
    //                Debug.WriteLine("ERROR", $"The path '{DirectoryPath}' is empty, contains only white spaces, or contains invalid characters such as \", <, >, or |. {AE}");
    //            } catch (PathTooLongException PTLE) {
    //                Debug.WriteLine("ERROR", $"The specified path, {DirectoryPath}, exceeds the system-defined maximum length. {PTLE}");
    //            }
    //        //} else {
    //        //    Debug.WriteLine($"Directory '{FilePath}' does not exist.");
    //        //}

    //        Result = null;
    //        return false;
    //    }

    //    public override string ToString() => $"*\\{RelativePath.FullName}";

    //    #region Equality Members

    //    public override bool Equals(object Obj) => Obj is RelativeDirectoryInfo Info && Equals(Info);

    //    public override int GetHashCode() => base.GetHashCode();

    //    public bool Equals(RelativeDirectoryInfo Other) => EqualityComparer<DirectoryInfo>.Default.Equals(BasePath, Other.BasePath);

    //    public static bool operator ==(RelativeDirectoryInfo Left, RelativeDirectoryInfo Right) => Left.Equals(Right);

    //    public static bool operator !=(RelativeDirectoryInfo Left, RelativeDirectoryInfo Right) => !(Left == Right);

    //    #endregion
    //}

    #endregion

}