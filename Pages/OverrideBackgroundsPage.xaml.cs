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
    /// <summary>A <see cref="System.Windows.Controls.Page"/> responsible for allowing the user to specify which beatmaps they would like to override the backgrounds of; being able to manage them too.</summary>
    /// <seealso cref="System.Windows.Controls.Page" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class OverrideBackgroundsPage {
        /// <summary>The current <see cref="BackgroundResourcesPage"/> instance.</summary>
        public static BackgroundResourcesPage BRP;

        /// <summary>Initialises a new instance of the <see cref="OverrideBackgroundsPage"/> class.</summary>
        public OverrideBackgroundsPage() {
            InitializeComponent();
            IndivFolderBrowser.DirectoryPathChanged += IndivFolderBrowser_DirectoryPathChanged;

            BRP = new BackgroundResourcesPage();
            BackgroundMethodRandomResFrame.Navigate(BRP);
        }

        /// <summary>Handles the <c>DirectoryPathChanged</c> event of the <see cref="IndivFolderBrowser"/> control.
        /// <para/>Calls <see cref="IndivListView"/>.<see cref="AppendableListView.Set(List{object})"/>, showing the newly-selected folders.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void IndivFolderBrowser_DirectoryPathChanged(object Sender, RoutedEventArgs E) {
            ReadOnlyCollection<DirectoryInfo> Folders = ((FolderMultiBrowser)Sender).SelectedPaths;
            if (Folders != null) {
                LogWindow.Log("Folder-space Updated.");

                List<object> F = Folders.Cast<object>().ToList();
                //F.Sort();

                //IndivListView.Clear();
                IndivListView.Set(F);
                //IndivListView.Items.Clear();
                StartButton.IsEnabled = F.Count > 0;
            }
        }

        /// <summary>Handles the <c>DirectoryPathChanged</c> event of the <see cref="AutoFolderBrowser"/> control.
        /// <para/>Searches through the newly-selected path for all available .osu beatmap files, calling <see cref="IndivListView"/>.<see cref="AppendableListView.Set(List{object})"/> with the relevant parent folders.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
                IndivListView.Set(F);
                StartButton.IsEnabled = F.Count > 0;
            }
        }

        /// <summary>Handles the <c>Changed</c> event of the <see cref="BackgroundMethodSpecificRadio"/> control.
        /// <para/>Updates the <see cref="Visibility"/> of the relevant <see cref="BackgroundMethodSpecificPanel"/> and <see cref="BackgroundMethodRandomPanel"/> controls.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void BackgroundMethod_Changed(object Sender, RoutedEventArgs E) {
            bool Specific = BackgroundMethodSpecificRadio.IsChecked ?? false;

            BackgroundMethodSpecificPanel.SetVisibility(Specific ? Visibility.Visible : Visibility.Collapsed);
            BackgroundMethodRandomPanel.SetVisibility(Specific ? Visibility.Collapsed : Visibility.Visible);
        }

        /// <summary>Manages the given <see cref="Beatmap"/>, overriding all relevant <see cref="Background"/> event files.</summary>
        /// <param name="Beatmap">The beatmap.</param>
        /// <param name="Location">The location.</param>
        /// <param name="Backgrounds">The backgrounds.</param>
        /// <param name="ManagedMedia">The managed media.</param>
        /// <param name="Resize">If set to <c>true</c> resizes the new backgrounds to match the original resolution, using the <see cref="WPFExtensions.ResizeToFill(Bitmap, int, int, PixelFormat, System.Drawing.Drawing2D.CompositingMode, System.Drawing.Drawing2D.CompositingQuality, System.Drawing.Drawing2D.InterpolationMode, System.Drawing.Drawing2D.SmoothingMode, System.Drawing.Drawing2D.PixelOffsetMode, System.Drawing.Drawing2D.WrapMode)"/> method.</param>
        /// <param name="BackupBackground">If set to <c>true</c>, calls <see cref="FileSystemExtensions.SetAsBackup(FileInfo, bool)"/> on the original background files before overwriting them.</param>
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

        /// <summary>Handles the <c>Click</c> event of the <see cref="StartButton"/> control.
        /// <para/>Prepares the managing <see cref="BackgroundWorker"/> and related UI, asynchronously starting it once ready.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>Handles the <c>ProgressChanged</c> event of the managing <see cref="BackgroundWorker"/>.
        /// <para/>Updates <see cref="BWProgress"/>.<see cref="RangeBase.Value"/> to the current <see cref="ProgressChangedEventArgs.ProgressPercentage"/>.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="ProgressChangedEventArgs"/> instance containing the event data.</param>
        void BW_ProgressChanged(object Sender, ProgressChangedEventArgs E) => Dispatcher.Invoke(() => {
            BWProgress.Value = E.ProgressPercentage;
        }, DispatcherPriority.Normal);

        /// <summary>Handles the <c>RunWorkerCompleted</c> event of the managing <see cref="BackgroundWorker"/>.
        /// <para/>Displays a <see cref="MessageBox"/>, summarising the processed beatmaps.
        /// <para/>Resets all relevant UI controls, allowing more beatmaps to be processed afterwards.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>Does the work in the managing <see cref="BackgroundWorker"/>.
        /// <para/>Iterates through each selected <see cref="Beatmap"/> and manages them, using <see cref="ManageBeatmap(Beatmap, DirectoryInfo, Bitmap[], ref HashSet{string}, bool, bool)"/> and returning the current progress between maps.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
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

        /// <summary>Results struct utilised for returning the final results of the managing <see cref="BackgroundWorker"/>.</summary>
        [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Type is only relevant to this specific class")]
        public readonly struct WorkerResults : IEquatable<WorkerResults> {
            /// <summary>The amount of processed sets.</summary>
            public readonly int Sets;
            /// <summary>The amount of processed maps.</summary>
            public readonly long Maps;

            /// <summary>Initialises a new instance of the <see cref="WorkerResults"/> struct.</summary>
            /// <param name="Sets">The sets.</param>
            /// <param name="Maps">The maps.</param>
            public WorkerResults(int Sets = -1, long Maps = -1) {
                this.Sets = Sets;
                this.Maps = Maps;
            }

            /// <summary>Determines whether the specified <see cref="object" />, is equal to this instance.</summary>
            /// <param name="Obj">The <see cref="object" /> to compare with this instance.</param>
            /// <returns><c>True</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
            public override bool Equals(object Obj) => Obj is WorkerResults Results && Equals(Results);

            /// <summary>Returns a hash code for this instance.</summary>
            /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. </returns>
            public override int GetHashCode() {
                unchecked { return(Sets * 397) ^ Maps.GetHashCode(); }
            }

            /// <summary>Determines whether the specified <see cref="WorkerResults" />, is equal to this instance.</summary>
            /// <param name="Other">The <see cref="WorkerResults" /> to compare with this instance.</param>
            /// <returns><c>True</c> if the specified <see cref="WorkerResults" /> is equal to this instance; otherwise, <c>false</c>.</returns>
            public bool Equals(WorkerResults Other) =>
                Sets == Other.Sets &&
                Maps == Other.Maps;

            /// <summary>Implements the operator ==.</summary>
            /// <param name="Left">The left.</param>
            /// <param name="Right">The right.</param>
            /// <returns>The result of the operator.</returns>
            public static bool operator ==(WorkerResults Left, WorkerResults Right) => Left.Equals(Right);

            /// <summary>Implements the operator !=.</summary>
            /// <param name="Left">The left.</param>
            /// <param name="Right">The right.</param>
            /// <returns>The result of the operator.</returns>
            public static bool operator !=(WorkerResults Left, WorkerResults Right) => !(Left == Right);

            /// <summary>Converts to string.</summary>
            /// <returns>A <see cref="string" /> that represents this instance.</returns>
            public override string ToString() => $"{Sets} Sets ({Maps} Maps).";
        }

        #endregion

        /// <summary>Saves the specified BMP.</summary>
        /// <param name="BMP">The BMP.</param>
        /// <param name="Destination">The destination.</param>
        /// <param name="Format">The format.</param>
        public static void Save(Bitmap BMP, FileInfo Destination, ImageFormat Format) => BMP.Save(Destination.FullName, Format);

        //public static void Save(Bitmap BMP, FileInfo Destination) => Save(BMP, Destination, GetFormat(Destination, out _));

        /// <summary>Gets the <see cref="ImageFormat"/> of the specified <paramref name="File"/>, if applicable.</summary>
        /// <param name="File">The file.</param>
        /// <param name="ParsedFormat">The parsed format.</param>
        /// <returns><see cref="ImageFormat"/></returns>
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

        /// <summary>Handles the <c>Click</c> event of the <see cref="BackgroundResize"/> control.
        /// <para/>Enables/Disables the <see cref="BtnResizeSettings"/> control, relative to whether or not <see cref="BackgroundResize"/>.<see cref="ToggleButton.IsChecked"/>.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void BackgroundResize_Click(object Sender, RoutedEventArgs E) => BtnResizeSettings.IsEnabled = BackgroundResize.IsChecked ?? false;

        /// <summary>Handles the <c>Click</c> event of the <see cref="BtnResizeSettings"/> control.
        /// <para/>Creates and shows a new instance of <see cref="GlobalImageResizerWindow"/>, allowing the user to modify the resizer parameters.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void BtnResizeSettings_Click(object Sender, RoutedEventArgs E) => new GlobalImageResizerWindow().Show();

        /// <summary>Handles the <c>Click</c> event of the <see cref="BackgroundMethodToggle"/> control.
        /// <para/>Updates the display icon of the button to match the <see cref="BackgroundMethodToggle"/>.<see cref="ToggleButton.IsChecked"/> status.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void BackgroundMethodToggle_Click(object Sender, RoutedEventArgs E) {
            bool UseResources = ((ToggleButton)Sender).IsChecked ?? false;

            BackgroundMethodToggleIcon.Kind = UseResources ? PackIconModernKind.ArrowDown : PackIconModernKind.ArrowUp;
        }
    }
}