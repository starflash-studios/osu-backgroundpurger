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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Win32;

#endregion

namespace Osu_BackgroundPurge {
    public static class OsuFileSystem {

        public enum OsuVersion { Classic, Lazer }

        #region Installation Detection

        public static bool TryGetInstall(OsuVersion V, out FileInfo Found) {
#if (DEBUG)
            Stopwatch Watch = new Stopwatch();
            Watch.Start();
#endif
            if (TryGetPhysicalInstall(V, out Found)) {
#if (DEBUG)
                Watch.Stop();
                Debug.WriteLine($"Found {V} through *file-system* search in {Watch.ElapsedMilliseconds:N2}ms.");
#endif
                return true;
            }

            if (TryGetRegistryInstall(V, out Found)) {
#if (DEBUG)
                Watch.Stop();
                Debug.WriteLine($"Found {V} through *registry* search in {Watch.ElapsedMilliseconds:N2}ms.");
#endif
                return true;
            }

            Debug.WriteLine($"Unable to find {V}.");
            Found = null;
            return false;
        }

        public static bool TryGetInstallLocation(OsuVersion V, out DirectoryInfo Found) {
            if (TryGetInstall(V, out FileInfo FoundFile)) {
                Found = FoundFile.Directory;
                return true;
            }

            Found = null;
            return false;
        }

        #region Explorer Method

        //Classic:
        //  New: C:\Users\{USER}\AppData\Local\osu!\
        //  Old: C:\Program Files\osu!\
        //    OR C:\Program Files(x86)\osu!\
        //Lazer:
        //       C:\Users\{USER}\AppData\Local\osulazer\
        public static bool TryGetPhysicalInstall(OsuVersion V, out FileInfo Found) {
            switch (V) {
                case OsuVersion.Classic:
                    if ($"{FileSystemExtensions.LocalAppData.FullName}\\osu!\\osu!.exe".TryGetFileInfo(true, out Found)) {
                        return true; // C:\Users\{USER}\AppData\Local\osu!\
                    } else if ($"{FileSystemExtensions.ProgramFiles.FullName}\\osu!\\osu!.exe".TryGetFileInfo(true, out Found)) {
                        return true; // C:\Program Files\osu!\
                    } else if ($"{FileSystemExtensions.ProgramFiles86.FullName}\\osu!\\osu!.exe".TryGetFileInfo(true, out Found)) {
                        return true; // C:\Program Files(x86)\osu!\
                    }
                    break;
                case OsuVersion.Lazer:
                    if ($"{FileSystemExtensions.LocalAppData.FullName}\\osulazer\\osu!.exe".TryGetFileInfo(true, out Found)) {
                        return true; // C:\Users\{USER}\AppData\Local\osulazer\
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(V), V, null);
            }

            Found = null;
            return false;
        }

        #endregion

        #region Registry Method

        public static bool TryGetRegistryInstall(OsuVersion V, out FileInfo Found) {
            foreach(RegistryKey Key in GetOsuInstallationKeys(V)) {
                if (TryGetValue(Key, "UninstallString", out object Value) && Value is string UninstallCommand) {
                    if (GetPath(UninstallCommand).TryGetFileInfo(true, out Found)) {
                        //Debug.WriteLine($"/////Found: {Found}/////");
                        return Found != null;
                    }
                }
            }

            Found = null;
            return false;
        }

        public static string GetPath(string RawString) => RawString.Split(RawString.Contains("\"") ? new[] { '"' } : new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

        [SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "<Pending>")]
        public static IEnumerable<RegistryKey> GetOsuInstallationKeys(OsuVersion V) {
            //Computer\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\
            using (RegistryKey UninstallNode = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\")) {
                foreach (string LocalNodeName in UninstallNode.GetSubKeyNames()) {
                    using (RegistryKey LocalNode = UninstallNode.OpenSubKey(LocalNodeName)) {
                        if (TryGetValue(LocalNode, "DisplayName", out object Value) && Value is string LocalAppName) {
                            switch (V) {
                                case OsuVersion.Classic:
                                    if (LocalAppName.Equals("osu!", StringComparison.InvariantCultureIgnoreCase)) {
                                        yield return LocalNode;
                                    }
                                    break;
                                case OsuVersion.Lazer:
                                    if (LocalAppName.Equals("osu!lazer", StringComparison.InvariantCultureIgnoreCase)) {
                                        yield return LocalNode;
                                    }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(V), V, null);
                            }
                        }
                    }
                }
            }
        }

        public static bool TryGetValue(RegistryKey Key, string ValueName, out object Value) {
            if (Key != null) {
                Value = Key.GetValue(ValueName);
                return Value != null;
            }

            Value = null;
            return false;
        }

        #endregion

        #endregion

        #region Osu!Lazer Functions

        // C:/Users/{USER}/AppData/Roaming/osu/rulesets/
        //                       %AppData%/osu/rulesets/
        // ReSharper disable once StringLiteralTypo
        public static bool TryGetLazerRulesetsPath(out DirectoryInfo Result) => $"{FileSystemExtensions.RoamingAppData.FullName}\\osu\\rulesets\\".TryGetDirectoryInfo(true, out Result);

        public static IEnumerable<DirectoryInfo> GetLazerVersionLocations(DirectoryInfo LazerInstallPath) => LazerInstallPath.GetDirectories("app-*", SearchOption.TopDirectoryOnly);

        public static IEnumerable<(Version Version, DirectoryInfo Path)> GetLazerVersions(DirectoryInfo LazerInstallPath) => from VersionPath in GetLazerVersionLocations(LazerInstallPath) let Name = VersionPath.Name select (new Version($"{(Name.StartsWith("app-", StringComparison.InvariantCultureIgnoreCase) ? Name.Substring(4) : Name)}.0"), VersionPath);

        #endregion
    }

}
