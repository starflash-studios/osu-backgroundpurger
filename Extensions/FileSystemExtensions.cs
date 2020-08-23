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
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using Ookii.Dialogs.Wpf;

#endregion

namespace Osu_BackgroundPurge {
    public static class FileSystemExtensions {

        #region FileSystemInfo Constructors

        /// <summary>Tries to convert the given string to a valid <see cref="FileInfo"/>.</summary>
        /// <param name="FilePath">The file path.</param>
        /// <param name="CheckExists">if set to <c>true</c> [check exists].</param>
        /// <param name="Result">The result.</param>
        /// <returns><see cref="bool"/></returns>
        public static bool TryGetFileInfo(this string FilePath, bool CheckExists, out FileInfo Result) {
            if (!CheckExists || File.Exists(FilePath)) {
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
            } else {
                Debug.WriteLine($"File '{FilePath}' does not exist.");
            }

            Result = null;
            return false;
        }

        /// <summary>Tries to convert the given string to a valid <see cref="DirectoryInfo"/>.</summary>
        /// <param name="DirectoryPath">The directory path.</param>
        /// <param name="CheckExists">if set to <c>true</c> [check exists].</param>
        /// <param name="Result">The result.</param>
        /// <returns><see cref="System.Boolean"/></returns>
        public static bool TryGetDirectoryInfo(this string DirectoryPath, bool CheckExists, out DirectoryInfo Result) {
            if (!CheckExists || Directory.Exists(DirectoryPath)) {
                try {
                    Result = new DirectoryInfo(DirectoryPath);
                    return Result != null;
                } catch (ArgumentNullException ANE) {
                    Debug.WriteLine("ERROR", $"'{DirectoryPath}' is null. {ANE}");
                } catch (SecurityException SE) {
                    Debug.WriteLine("ERROR", $"The caller does not have the required permissions to access '{DirectoryPath}'. {SE}");
                } catch (ArgumentException AE) {
                    Debug.WriteLine("ERROR", $"The path '{DirectoryPath}' is empty, contains only white spaces, or contains invalid characters such as \", <, >, or |. {AE}");
                } catch (PathTooLongException PTLE) {
                    Debug.WriteLine("ERROR", $"The specified path, {DirectoryPath}, exceeds the system-defined maximum length. {PTLE}");
                }
            } else {
                Debug.WriteLine($"Directory '{DirectoryPath}' does not exist.");
            }

            Result = null;
            return false;
        }

        #endregion

        #region FileSystemInfo Selectors

        /// <summary>Starts a new instance of explorer.exe, specifying the file/folder to highlight.</summary>
        /// <param name="Info">The file/folder to select.</param>
        /// <returns><see cref="Process"/></returns>
        internal static Process SelectInternal(FileSystemInfo Info) {
            Process ExplorerProcess = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{Info.FullName}\""
                }
            };

            ExplorerProcess.Start();
            return ExplorerProcess;
        }

        /// <summary>Creates a new File Explorer instance, highlighting the specified <paramref name="Info"/>. If the path doesn't physically exist and <paramref name="Exact"/> is set to <c>false</c>, the closest found relative is highlighted instead.</summary>
        /// <param name="Info">The file/folder to select.</param>
        /// <param name="Exact">Whether or not only the exact path can be highlighted.</param>
        /// <returns><see cref="Process"/></returns>
        public static Process Select(this FileSystemInfo Info, bool Exact = false) {
            if (Info.Exists) {
                return SelectInternal(Info);
            }

            if (!Exact) {
                throw new FileNotFoundException($"The given path '{Info.FullName}' could not be found.", Info.FullName);
            }

            FileSystemInfo ClosestRelative = Info.GetClosestRelative();
            if (ClosestRelative?.Exists ?? false) {
                return SelectInternal(ClosestRelative);
            }

            return null;
        }

        #endregion

        #region File Browsers

        /// <summary>Opens a File Browser dialog with the given parameters, returning the selected file, if any.</summary>
        /// <param name="Title">The title.</param>
        /// <param name="Filters">The filters.</param>
        /// <param name="FilterIndex">Index of the default filter.</param>
        /// <param name="InitialFolder">The initial folder to start at.</param>
        /// <returns><see cref="FileInfo"/></returns>
        public static FileInfo OpenFileBrowser(string Title = null, string Filters = null, int FilterIndex = 0, string InitialFolder = null) => OpenFileBrowser(false, Title, Filters, FilterIndex, InitialFolder).FirstOrDefault();

        /// <summary>Opens a File Browser dialog with the given parameters, returning the selected files, if any.</summary>
        /// <param name="Multiselect">Whether or not the dialog can return multiple files.</param>
        /// <param name="Title">The title.</param>
        /// <param name="Filters">The filters.</param>
        /// <param name="FilterIndex">Index of the default filter.</param>
        /// <param name="InitialFolder">The initial folder to start at.</param>
        /// <returns><see cref="FileInfo"/></returns>
        public static IEnumerable<FileInfo> OpenFileBrowser(bool Multiselect, string Title = null, string Filters = null, int FilterIndex = 0, string InitialFolder = null) {
            VistaOpenFileDialog OpenFileDialog = new VistaOpenFileDialog {
                CheckFileExists = true,
                CheckPathExists = true,
                ValidateNames = true,
                Filter = Filters ?? $"Any file{(Multiselect ? "(s)" : "")} (*.*)|*.*",
                FilterIndex = FilterIndex,
                Title = Title ?? $"Select {(Multiselect ? "any file(s)" : "a file")}.",
                Multiselect = Multiselect,
                InitialDirectory = InitialFolder ?? Desktop.FullName
            };

            if (OpenFileDialog.ShowDialog() == true) {
                foreach (string Path in OpenFileDialog.FileNames) {
                    if (Path.TryGetFileInfo(false, out FileInfo Found)) {
                        yield return Found;
                    }
                }
            }
        }

        #endregion

        #region Backup 

        /// <summary>Sets the given file as a 'backup' by pre-pending the extension with <c>'.bkp'</c>.</summary>
        /// <param name="File">The file.</param>
        /// <param name="Overwrite">If set to <c>true</c> [overwrite any old backups].</param>
        public static void SetAsBackup(this FileInfo File, bool Overwrite = false) {
            if (!File.IsBackup()) {
                FileInfo Destination = new FileInfo(File.PreviewBackupDestination());

                if (!Destination.Exists) {
                    File.CopyTo(Destination, Overwrite);
                    return;
                }
            }

            Debug.WriteLine($"File '{File.FullName}' is already a backup. Setting previously-backed-up files as a backup is not recommended. Perform a File.IsBackup() check before calling this method.", "WARNING");
        }

        /// <summary>Sets the given backup as a master file by removing the pre-pended <c>'.bkp'</c> extension.</summary>
        /// <param name="File">The file.</param>
        /// <param name="Overwrite">If set to <c>true</c> [overwrite the master file if found].</param>
        public static void SetAsMaster(this FileInfo File, bool Overwrite = false) => RevertBackup(File, Overwrite);

        /// <summary>Sets the given backup as a master file by removing the pre-pended <c>'.bkp'</c> extension.</summary>
        /// <param name="File">The file.</param>
        /// <param name="Overwrite">If set to <c>true</c> [overwrite the master file if found].</param>
        public static void SetAsOriginal(this FileInfo File, bool Overwrite = false) => RevertBackup(File, Overwrite);

        /// <summary>Reverts the given backup to a master file by removing the pre-pended <c>'.bkp'</c> extension.</summary>
        /// <param name="File">The file.</param>
        /// <param name="Overwrite">If set to <c>true</c> [overwrite the original file if found].</param>
        public static void RevertBackup(this FileInfo File, bool Overwrite = false) {
            if (File.IsBackup()) {
                FileInfo Destination = new FileInfo(File.PreviewOriginalDestination());
                bool Exists = Destination.Exists;

                if (Exists && Overwrite) {
                    Destination.Delete();
                }

                if (!Exists) {
                    File.CopyTo(Destination);
                    return;
                }
            }

            Debug.WriteLine($"File '{File.FullName}' is not a backup. Reverting non-backed-up files is not recommended. Perform a File.IsBackup() check before calling this method.", "WARNING");
        }

        /// <summary>Determines whether this <paramref name="File"/> is backup.</summary>
        /// <param name="File">The file.</param>
        /// <returns><c>True</c> if the specified file is backup; otherwise, <c>false</c>.</returns>
        public static bool IsBackup(this FileInfo File) => File.NameWithoutExtension().ToLowerInvariant().EndsWith(".bkp");

        /// <summary>Returns the estimated backup destination.</summary>
        /// <param name="File">The file.</param>
        /// <returns><see cref="string"/></returns>
        public static string PreviewBackupDestination(this FileInfo File) => $"{File.DirectoryName}\\{File.NameWithoutExtension()}.bkp{File.Extension}";

        /// <summary>Returns the estimated master destination.</summary>
        /// <param name="File">The file.</param>
        /// <returns><see cref="string"/></returns>
        public static string PreviewOriginalDestination(this FileInfo File) => $"{File.DirectoryName}\\{File.NameWithoutExtension().TrimEnd(".bkp")}{File.Extension}";

        #endregion

        #region Copy / Move

        /// <summary>Copies the <paramref name="File"/> to the given <paramref name="Destination"/>, Overwriting any files if specified.</summary>
        /// <param name="File">The file.</param>
        /// <param name="Destination">The destination.</param>
        /// <param name="Overwrite">If set to <c>true</c> [overwrite].</param>
        public static void CopyTo(this FileInfo File, FileInfo Destination, bool Overwrite = false) => File.CopyTo(Destination.FullName, Overwrite);


        /// <summary>Copies the <paramref name="File"/> to the given <paramref name="Location"/>, Overwriting any files if specified.</summary>
        /// <param name="File">The file.</param>
        /// <param name="Location">The location.</param>
        /// <param name="Overwrite">If set to <c>true</c> [overwrite].</param>
        public static void CopyTo(this FileInfo File, DirectoryInfo Location, bool Overwrite = false) => File.CopyTo($"{Location.FullName}\\{File.Name}", Overwrite);

        /// <summary>Moves the <paramref name="File"/> to the given <paramref name="Destination"/>, Overwriting any files if specified.</summary>
        /// <param name="File">The file.</param>
        /// <param name="Destination">The destination.</param>
        /// <param name="Overwrite">If set to <c>true</c> [overwrite].</param>
        public static void MoveTo(this FileInfo File, FileInfo Destination, bool Overwrite = false) {
            if (Overwrite && Destination.Exists) { Destination.Delete(); }
            File.MoveTo(Destination.FullName);
        }


        /// <summary>Moves the <paramref name="File"/> to the given <paramref name="Location"/>, Overwriting any files if specified.</summary>
        /// <param name="File">The file.</param>
        /// <param name="Location">The location.</param>
        /// <param name="Overwrite">If set to <c>true</c> [overwrite].</param>
        public static void MoveTo(this FileInfo File, DirectoryInfo Location, bool Overwrite = false) => File.MoveTo(new FileInfo($"{Location.FullName}\\{File.Name}"), Overwrite);

        #endregion

        #region Get Files

        /// <summary>Returns all files within the directory, matching any <paramref name="Wildcards"/> if given. If <paramref name="SearchOption"/> is given, the relevant search method is utilised.</summary>
        /// <param name="Directory">The directory.</param>
        /// <param name="SearchOption">The search option.</param>
        /// <param name="Wildcards">The wildcards.</param>
        /// <returns><see cref="IEnumerable{FileInfo}"/></returns>
        public static IEnumerable<FileInfo> GetFiles(this DirectoryInfo Directory, SearchOption SearchOption, params string[] Wildcards) => Wildcards.SelectMany(Wildcard => Directory.GetFiles(Wildcard, SearchOption));

        /// <summary>Returns all files within the directory, matching any <paramref name="Wildcards"/> if given.</summary>
        /// <param name="Directory">The directory.</param>
        /// <param name="Wildcards">The wildcards.</param>
        /// <returns><see cref="IEnumerable{FileInfo}"/></returns>
        public static IEnumerable<FileInfo> GetFiles(this DirectoryInfo Directory, params string[] Wildcards) => Wildcards.SelectMany(Wildcard => Directory.GetFiles(Wildcard, SearchOption.TopDirectoryOnly));

        #endregion

        /// <summary>Returns the <paramref name="File"/>'s name without its extension.</summary>
        /// <param name="File">The file.</param>
        /// <returns><see cref="System.String"/></returns>
        public static string NameWithoutExtension(this FileInfo File) => Path.GetFileNameWithoutExtension(File.Name);

        /// <summary>Returns the string as a file name with all invalid windows characters removed.</summary>
        /// <param name="String">The string.</param>
        /// <returns>A <see cref="string"/> representing a valid filename.</returns>
        public static string ToSafeFileName(this string String) => new Regex($"[{Regex.Escape($"{new string(Path.GetInvalidFileNameChars())}{new string(Path.GetInvalidPathChars())}")}]").Replace(String, "");

        #region Closest Relative

        /// <summary>Returns the 'closest relative' to the given <paramref name="Info"/>. If <see cref="FileInfo"/>, returns <c>FileInfo.Directory</c>. If <see cref="DirectoryInfo"/>, returns <c>DirectoryInfo.Parent</c>.</summary>
        /// <param name="Info">The information.</param>
        /// <returns><see cref="FileSystemInfo"/></returns>
        public static FileSystemInfo GetClosestRelative(this FileSystemInfo Info) {
            DirectoryInfo ClosestRelative = null;
            switch (Info) {
                case FileInfo File:
                    if (File.Exists) { return File; }
                    ClosestRelative = File.Directory;
                    break;
                case DirectoryInfo Dir:
                    if (Dir.Exists) { return Dir; }
                    ClosestRelative = Dir.Parent;
                    break;
            }

            while (ClosestRelative != null) {
                if (ClosestRelative.Exists) { return ClosestRelative; }
                ClosestRelative = ClosestRelative.Parent;
            }

            return Path.GetPathRoot(Environment.SystemDirectory).TryGetDirectoryInfo(true, out DirectoryInfo Default) ? Default : null;
        }

        #endregion

        #region Exact Locations

        /// <summary>Returns the location of the system desktop.</summary>
        /// <value>The desktop.</value>
        public static DirectoryInfo Desktop => new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

        /// <summary>Returns the location of the system's 'roaming' application data folder.</summary>
        /// <value>Essentially the equivalent to %APPDATA%.</value>
        public static DirectoryInfo RoamingAppData => new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        /// <summary>Returns the location of the system's 'local' application data folder.</summary>
        /// <value>Essentially the equivalent to %LOCALAPPDATA%.</value>
        public static DirectoryInfo LocalAppData => new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

        /// <summary>Returns the location of the system's 'Program Files' folder.</summary>
        /// <value>Essentially the equivalent to %PROGRAMFILES%</value>
        public static DirectoryInfo ProgramFiles => new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

        /// <summary>Returns the location of the system's 'Program Files (x86)' folder.</summary>
        /// <value>Essentially the equivalent to %PROGRAMFILES(x86)%</value>
        public static DirectoryInfo ProgramFiles86 => new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));

        #endregion

        #region FileInfo Read/Write

        /// <summary>Returns all text within the given <paramref name="FileInfo"/>.</summary>
        /// <param name="FileInfo">The file to read.</param>
        /// <returns><see cref="string"/></returns>
        public static string ReadAllText(this FileInfo FileInfo) => File.ReadAllText(FileInfo.FullName);

        /// <summary>Returns all lines of text within the given <paramref name="FileInfo"/>.</summary>
        /// <param name="FileInfo">The file to read.</param>
        /// <returns><see cref="Array.string"/></returns>
        public static string[] ReadAllLines(this FileInfo FileInfo) => File.ReadAllLines(FileInfo.FullName);

        /// <summary>Returns all bytes within the given <paramref name="FileInfo"/>.</summary>
        /// <param name="FileInfo">The file to read.</param>
        /// <returns><see cref="Array.byte"/></returns>
        public static byte[] ReadAllBytes(this FileInfo FileInfo) => File.ReadAllBytes(FileInfo.FullName);

        /// <summary><inheritdoc cref="File.WriteAllText(string, string)"/></summary>
        /// <param name="FileInfo"><inheritdoc cref="File.WriteAllText(string, string)" path="path"/></param>
        /// <param name="Contents"><inheritdoc cref="File.WriteAllText(string, string)" path="contents"/></param>
        public static void WriteAllText(this FileInfo FileInfo, string Contents) => File.WriteAllText(FileInfo.FullName, Contents);

        /// <summary><inheritdoc cref="File.WriteAllLines(string, IEnumerable{string})"/></summary>
        /// <param name="FileInfo"><inheritdoc cref="File.WriteAllLines(string, IEnumerable{string})" path="path"/></param>
        /// <param name="Lines"><inheritdoc cref="File.WriteAllLines(string, IEnumerable{string})" path="contents"/></param>
        public static void WriteAllLines(this FileInfo FileInfo, IEnumerable<string> Lines) => File.WriteAllLines(FileInfo.FullName, Lines);

        /// <summary><inheritdoc cref="File.WriteAllBytes(string, byte[])"/></summary>
        /// <param name="FileInfo"><inheritdoc cref="File.WriteAllBytes(string, byte[])" path="path"/></param>
        /// <param name="Bytes"><inheritdoc cref="File.WriteAllBytes(string, byte[])" path="bytes"/></param>
        public static void WriteAllBytes(this FileInfo FileInfo, byte[] Bytes) => File.WriteAllBytes(FileInfo.FullName, Bytes);

        /// <summary><inheritdoc cref="File.AppendAllText(string, string)"/></summary>
        /// <param name="FileInfo"><inheritdoc cref="File.AppendAllText(string, string)" path="path"/></param>
        /// <param name="Contents"><inheritdoc cref="File.AppendAllText(string, string)" path="contents"/></param>
        public static void AppendAllText(this FileInfo FileInfo, string Contents) => File.AppendAllText(FileInfo.FullName, Contents);

        /// <summary><inheritdoc cref="File.AppendAllLines(string, IEnumerable{string})"/></summary>
        /// <param name="FileInfo"><inheritdoc cref="File.AppendAllLines(string, IEnumerable{string})" path="path"/></param>
        /// <param name="Lines"><inheritdoc cref="File.AppendAllLines(string, IEnumerable{string})" path="contents"/></param>
        public static void AppendAllLines(this FileInfo FileInfo, IEnumerable<string> Lines) => File.AppendAllLines(FileInfo.FullName, Lines);

        #endregion

        /// <summary><inheritdoc cref="File.Exists(string)"/></summary>
        /// <param name="FileInfo"><inheritdoc cref="File.Exists(string)" path="path"/></param>
        /// <returns>If <paramref name="FileInfo"/> is null, <c>False</c>, otherwise <inheritdoc cref="File.Exists(string)" path="returns"/></returns>
        public static bool Exists(this FileInfo FileInfo) => FileInfo != null && File.Exists(FileInfo.FullName);

        /// <summary><inheritdoc cref="Directory.Exists(string)"/></summary>
        /// <param name="DirectoryInfo"><inheritdoc cref="Directory.Exists(string)" path="path"/></param>
        /// <returns>If <paramref name="DirectoryInfo"/> is null, <c>False</c>, otherwise <inheritdoc cref="Directory.Exists(string)" path="returns"/></returns>
        public static bool Exists(this DirectoryInfo DirectoryInfo) => DirectoryInfo != null && Directory.Exists(DirectoryInfo.FullName);

    }
}