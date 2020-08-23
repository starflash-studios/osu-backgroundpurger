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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using MahApps.Metro.IconPacks;
using Osu_BackgroundPurge.UserControls;
using Osu_BackgroundPurge.Windows;
using OsuParser;
using OsuParser.Events;

#endregion

namespace Osu_BackgroundPurge.Pages {

    public partial class OverrideBackgroundsPage {
        public static BackgroundResourcesPage BRP;
        public static DirectoryInfo Temp;

        public OverrideBackgroundsPage() {
            InitializeComponent();
            IndivFolderBrowser.DirectoryPathChanged += IndivFolderBrowser_DirectoryPathChanged;

            BRP = new BackgroundResourcesPage();
            BackgroundMethodRandomResFrame.Navigate(BRP);
        }

        void IndivFolderBrowser_DirectoryPathChanged(object Sender, RoutedEventArgs E) {
            ReadOnlyCollection<DirectoryInfo> Folders = ((FolderMultiBrowser)Sender).SelectedPaths;
            if (Folders != null) {
                LogWindow.Log("Folder-space Updated.");

                List<object> F = Folders.Cast<object>().ToList();
                //F.Sort();

                //IndivListView.Clear();
                IndivListView.SetList(F);
                //IndivListView.Items.Clear();
                StartButton.IsEnabled = F.Count > 0;
            }
        }

        void AutoFolderBrowser_DirectoryPathChanged(object Sender, RoutedEventArgs E) {
            DirectoryInfo Folder = ((FolderBrowser)Sender).SelectedPath;
            if (Folder != null) {
                List<object> F = new List<object>();
                HashSet<string> Fn = new HashSet<string>();
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach(FileInfo Beatmap in Folder.GetFiles("*.osu", SearchOption.AllDirectories)) {
                    DirectoryInfo Dir = Beatmap.Directory;
                    if (Dir.Parent.FullName.Equals(Folder.FullName, StringComparison.InvariantCultureIgnoreCase)) {
                        string N = Dir.FullName.ToLowerInvariant();
                        if (!Fn.Contains(N)) {
                            Fn.Add(N);
                            F.Add(Beatmap.Directory);
                        }
                    }
                }

                //F.Sort();
                IndivListView.SetList(F);
                StartButton.IsEnabled = F.Count > 0;
            }
        }

        void BackgroundMethod_Changed(object Sender, RoutedEventArgs E) {
            bool Specific = BackgroundMethodSpecificRadio.IsChecked ?? false;

            BackgroundMethodSpecificPanel.SetVisibility(Specific ? Visibility.Visible : Visibility.Collapsed);
            BackgroundMethodRandomPanel.SetVisibility(Specific ? Visibility.Collapsed : Visibility.Visible);
        }

        public static void ManageBeatmap(Beatmap Beatmap, DirectoryInfo Location, Bitmap[] Backgrounds, ref HashSet<string> ManagedMedia, bool Resize, bool BackupBackground) {

            foreach (OsuEvent Event in Beatmap.Events) {
                LogWindow.Log($"\tChecking Event: {Event}");
                switch (Event) {
                    case Background BG:
                        LogWindow.Log($"\t\t=> Got BG Event: {BG}");
                        if (BG.FileName.TryGetAbsoluteFile(Location, out FileInfo RelativeFile) && !RelativeFile.Extension.IsNullOrEmpty()) {
                            string RelativeFileName = RelativeFile.FullName.ToLowerInvariant();
                            if (!ManagedMedia.Contains(RelativeFileName)) {
                                ManagedMedia.Add(RelativeFileName);
                                LogWindow.Log($"\t\t\tFound Media: '{RelativeFile.FullName}'.");

                                ImageFormat Final = GetFormat(RelativeFile, out _);
                                if (Final == null) { return; }

                                Bitmap NewBackground = Backgrounds.GetRandom();

                                if (RelativeFile.Exists) {

                                    if (BackupBackground) {
                                        RelativeFile.SetAsBackup();
                                    }

                                    if (Resize) {
                                        LogWindow.Log("Attempting resize...");
                                        int W = -1;
                                        int H = -1;

                                        try {
                                            using (Bitmap Original = new Bitmap(RelativeFile.FullName)) {
                                                W = Original.Width;
                                                H = Original.Height;
                                            }
                                        } catch { } //Unable to access / import bitmap; Skipped in next check.

                                        if (W > 0 && H > 0) {
                                            LogWindow.Log($"\tWill resize to: ({W}, {H}) ;; Chosen BG: ({NewBackground.Width}, {NewBackground.Height})");
                                            NewBackground = NewBackground.ResizeToFill(W, H, PixelFormat.Format24bppRgb, GlobalImageResizerWindow.CompositingMode, GlobalImageResizerWindow.CompositingQuality, GlobalImageResizerWindow.InterpolationMode, GlobalImageResizerWindow.SmoothingMode, GlobalImageResizerWindow.PixelOffsetMode, GlobalImageResizerWindow.WrapMode);
                                        } else {
                                            LogWindow.Log("\tFailed retrieving original media size.");
                                        }
                                    }

                                    RelativeFile.Delete();
                                }

                                Save(NewBackground, RelativeFile, Final);
                            } else { LogWindow.Log($"\t\t\tMedia: '{RelativeFile.FullName}' was already managed."); }
                        }

                        break;
                    //case OsuEvent.OsuEventType.Video: //TODO: Video event support?
                    //    break;
                }
            }
        }

        void StartButton_Click(object Sender, RoutedEventArgs E) {
            StartButton.IsEnabled = false;

            BWProgress.Value = 0;
            BWProgress.IsIndeterminate = true;
            BWProgress.Visibility = Visibility.Visible;

            bool Specific = BackgroundMethodSpecificRadio.IsChecked ?? false;
            bool Resize = BackgroundResize.IsChecked ?? false;
            bool Backup = BackgroundKeepOld.IsChecked ?? false;
            bool UseResources = BackgroundMethodToggle.IsChecked ?? false;

            FileInfo SingleBackgroundPath = BackgroundMethodSpecificBrowser.SelectedPath;
            DirectoryInfo BackgroundsFolderPath = BackgroundMethodRandomBrowser.SelectedPath;


            BackgroundWorker BW = new BackgroundWorker {
                WorkerReportsProgress = true
            };
            BW.DoWork += BW_DoWork;
            BW.ProgressChanged += BW_ProgressChanged;
            BW.RunWorkerCompleted += BW_RunWorkerCompleted;

            BW.RunWorkerAsync((Specific, Resize, Backup, UseResources, SingleBackgroundPath, BackgroundsFolderPath));
        }

        void BW_ProgressChanged(object Sender, ProgressChangedEventArgs E) => Dispatcher.Invoke(() => {
            BWProgress.Value = E.ProgressPercentage;
        }, DispatcherPriority.Normal);

        void BW_RunWorkerCompleted(object Sender, RunWorkerCompletedEventArgs E) {
            Dispatcher.Invoke(() => {
                BWProgress.Visibility = Visibility.Hidden;
                StartButton.IsEnabled = true;
            });

            WorkerResults Result = (WorkerResults)E.Result;
            if (E.Cancelled) {
                MessageBox.Show($"Managed {Result.Sets:N0} Beatmap sets ({Result.Maps:N0} Beatmap difficulties in total) before being cancelled.", $"Cancelled - {Title}", MessageBoxButton.OK, MessageBoxImage.Warning);
            } else {
                MessageBox.Show($"Managed {Result.Sets:N0} Beatmap sets ({Result.Maps:N0} Beatmap difficulties in total)!", $"Complete! - {Title}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        void BW_DoWork(object Sender, DoWorkEventArgs E) {
            BackgroundWorker Worker = (BackgroundWorker)Sender;

            (bool Specific, bool Resize, bool Backup, bool UseResources, FileInfo SingleBackgroundPath, DirectoryInfo BackgroundsFolderPath) = ((bool, bool, bool, bool, FileInfo, DirectoryInfo))E.Argument;

            Bitmap[] Backgrounds = Array.Empty<Bitmap>();

            if (Specific) {
                if (SingleBackgroundPath != null && SingleBackgroundPath.Exists) {
                    Backgrounds = new[] { new Bitmap(SingleBackgroundPath.FullName) };
                }
            } else {

                if (!UseResources && BackgroundsFolderPath != null) {
                    Backgrounds = BackgroundsFolderPath.GetFiles("*.png", "*.jpg", "*.jpeg", "*.bmp").Select(File => new Bitmap(File.FullName, true)).ToArray();
                    LogWindow.Log($"Found: {Backgrounds.Length} Files");
                    //return;
                }
            }

            List<object> Dirs = new List<object>();
            Dispatcher.Invoke(() => {
                if (Backgrounds.Length == 0) {
                    Backgrounds = BRP.GetActiveBackgrounds().ToArray();
                }

                BWProgress.IsIndeterminate = false;
                Dirs = IndivListView.ListProperty;
            }, DispatcherPriority.Normal);

            HashSet<string> ManagedMedia = new HashSet<string>();
            float DirCount = Dirs.Count;
            int FinishedDirs = 0;
            int FinishedSets = 0;
            long FinishedMaps = 0;
            foreach (object DirObject in Dirs) {
                DirectoryInfo Dir = (DirectoryInfo)DirObject;
                //LogWindow.Log($"Managing {Dir.FullName}");
                try {
                    foreach (Beatmap Beatmap in Dir.GetFiles("*.osu", SearchOption.TopDirectoryOnly).Select(FoundBeatmap => Beatmap.GetBeatmap(FoundBeatmap.ReadAllLines(),
                        false,
                        false,
                        false,
                        false,
                        true, //We only care about background events, since those are the only things that will be affected. Note: To ensure rankings are still valid, ONLY EDIT EXTERNAL FILES; DO NOT EDIT THE .osu BEATMAP!!!
                        false,
                        false))) {
                        //LogWindow.Log($"\tGot Beatmap: {Beatmap}");

                        try { ManageBeatmap(Beatmap, Dir, Backgrounds, ref ManagedMedia, Resize, Backup); } catch (Exception Ex) {
                            Dispatcher.Invoke(() => ExceptionWindow.Catch(Ex), DispatcherPriority.Normal);
                        }

                        FinishedMaps++;
                        if (E.Cancel) {
                            E.Result = new WorkerResults(FinishedSets, FinishedMaps);
                            return;
                        }
                    }

                    FinishedSets++;
                    if (E.Cancel) {
                        E.Result = new WorkerResults(FinishedSets, FinishedMaps);
                        return;
                    }
                } catch (Exception Ex) {
                    Dispatcher.Invoke(() => ExceptionWindow.Catch(Ex), DispatcherPriority.Normal);
                }

                Worker.ReportProgress((FinishedDirs / DirCount * 100.0f).CeilToWhole());
                FinishedDirs++;
            }

            Worker.ReportProgress(100);
            E.Result = new WorkerResults(FinishedSets, FinishedMaps);
        }

        #region WorkerResults Struct

        [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Type is only relevant to this specific class")]
        public readonly struct WorkerResults : IEquatable<WorkerResults> {
            public readonly int Sets;
            public readonly long Maps;

            public WorkerResults(int Sets = -1, long Maps = -1) {
                this.Sets = Sets;
                this.Maps = Maps;
            }

            public override bool Equals(object Obj) => Obj is WorkerResults Results && Equals(Results);

            public override int GetHashCode() {
                unchecked { return(Sets * 397) ^ Maps.GetHashCode(); }
            }

            public bool Equals(WorkerResults Other) =>
                Sets == Other.Sets &&
                Maps == Other.Maps;

            public static bool operator ==(WorkerResults Left, WorkerResults Right) => Left.Equals(Right);

            public static bool operator !=(WorkerResults Left, WorkerResults Right) => !(Left == Right);

            public override string ToString() => $"{Sets} Sets ({Maps} Maps).";
        }

        #endregion

        Dictionary<string, FileInfo> _GeneratedBitmaps;
        public FileInfo GetReferenceBackground(Bitmap BMP, FileInfo Destination) {
            if (Temp == null || !Temp.Exists) {
                Temp = new DirectoryInfo($"{Path.GetTempPath().TrimEnd("\\")}\\{Path.GetRandomFileName().TrimEnd(".tmp")}\\");
                Temp.Create();
            }

            if (_GeneratedBitmaps == null) { _GeneratedBitmaps = new Dictionary<string, FileInfo>(); }

            ImageFormat F = GetFormat(Destination, out string E);

            if (F != null) {
                if (_GeneratedBitmaps.ContainsKey(E)) {
                    return _GeneratedBitmaps[E];
                }

                FileInfo Tmp = new FileInfo(Path.Combine(Temp.FullName, $"{Path.GetTempFileName()}.{E}"));
                BMP.Save(Tmp.FullName, F);
                _GeneratedBitmaps.Add(E, Tmp);
            }

            return _GeneratedBitmaps.FirstOrDefault().Value;
        }

        public static void Save(Bitmap BMP, FileInfo Destination, ImageFormat Format) => BMP.Save(Destination.FullName, Format);

        public static void Save(Bitmap BMP, FileInfo Destination) => Save(BMP, Destination, GetFormat(Destination, out _));

        public static ImageFormat GetFormat(FileInfo File, out string ParsedFormat) {
            string E = File.Extension.TrimStart('.').ToLowerInvariant();
            switch (E) {
                case "png":
                    ParsedFormat = "png";
                    return ImageFormat.Png;
                case "jpg":
                case "jpeg":
                    ParsedFormat = "jpg";
                    return ImageFormat.Jpeg;
                case "bmp":
                    ParsedFormat = "bmp";
                    return ImageFormat.Bmp;
                case "gif":
                    ParsedFormat = "gif";
                    return ImageFormat.Gif;
                default:
                    LogWindow.Log($"Unknown extension '{E}'.", "WARNING");
                    ParsedFormat = string.Empty;
                    return null;
            }
        }

        void BackgroundResize_Click(object Sender, RoutedEventArgs E) => BtnResizeSettings.IsEnabled = BackgroundResize.IsChecked ?? false;

        void BtnResizeSettings_Click(object Sender, RoutedEventArgs E) => new GlobalImageResizerWindow().Show();

        void BackgroundMethodToggle_Click(object Sender, RoutedEventArgs E) {
            bool UseResources = ((ToggleButton)Sender).IsChecked ?? false;

            BackgroundMethodToggleIcon.Kind = UseResources ? PackIconModernKind.ArrowDown : PackIconModernKind.ArrowUp;
        }
    }
}