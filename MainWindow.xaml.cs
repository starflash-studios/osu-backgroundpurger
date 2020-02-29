//This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program. If not, see<https://www.gnu.org/licenses/>.

//Starflash Studios, hereby disclaims all copyright interest in the program 'Osu!BackgroundPurger' (which is an automated osu!beatmap background remover) written by Cody Bock.

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using SuperfastBlur;

namespace OsuBackgroundPurger {
    public partial class MainWindow {
        public static MainWindow instance;

        // ReSharper disable once PossibleInvalidOperationException
        public static int BlurAmount => (int)instance.BlurProcessInput.Value;

        public Mode GetMode() => (Mode)ModeControl.SelectedIndex;

        public enum Mode {
            Disable,
            Enable,
            Delete,
            Blur
        }

        public MainWindow() {
            InitializeComponent();
            instance = this;

            UpdateChecker.Create(Dispatcher, this);
            ExceptionWindow.Create();
            ExceptionWindow.onChange += OnException;
        }

        void OnException(List<SafeException> exceptions) {
            Dispatcher.Invoke(() => {
                WarningPanel.Visibility = exceptions != null && exceptions.Count > 0 ? Visibility.Visible : Visibility.Hidden;
                WarningLabel.Content = "" + (exceptions?.Count ?? 0);
            });
        }

        #region File Management Functions
        /// <summary>
        /// Opens a Windows Explorer dialog allowing the user to select one/multiple directories for input
        /// </summary>
        /// <param name="multi"></param>
        /// <param name="startLocation"></param>
        /// <returns></returns>
        public static IEnumerable<DirectoryInfo> GetFolder(bool multi = false, DirectoryInfo startLocation = null) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog {
                InitialDirectory = startLocation?.FullName ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                IsFolderPicker = true,
                Multiselect = multi
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                foreach (DirectoryInfo d in dialog.FileNames.Select(f => new DirectoryInfo(f)).Where(d => d.Exists)) { yield return d; }
            }
        }

        /// <summary>
        /// Renames a file to the given name (w/ extension)
        /// </summary>
        /// <param name="file"></param>
        /// <param name="newName"></param>
        /// <param name="resolve"></param>
        public static void Rename(FileInfo file, string newName, bool resolve = false) {
            if (file == null || !file.Exists) { Debug.WriteLine("File is null or nonexistent; Returning."); return; }

            FileInfo newFile = new FileInfo(file.Directory.FullName + "//" + newName);
            if (string.Equals(file.FullName, newFile.FullName, StringComparison.InvariantCultureIgnoreCase)) { Debug.WriteLine("File is duplicate; Returning."); return; }
            string nFn = newFile.FullName;
            if (newFile.Exists) {
                if (resolve) {
                    newFile.Delete();
                    Task.Delay(30);
                    Debug.WriteLine("\t^ exists, Deleting");
                } else {
                    Debug.WriteLine("\t^ exists, Aborting");
                    return;
                }
            }
            file.MoveTo(nFn);
        }

        /// <summary>
        /// Validates the file if program is in restoration mode, otherwise it's invalidated
        /// </summary>
        /// <param name="file"></param>
        /// <param name="resolve"></param>
        public void AutoEnable(FileInfo file, bool resolve = false) {
            switch (GetMode()) {
                case Mode.Disable:
                    Disable(file, resolve);
                    break;
                case Mode.Enable:
                    Enable(file, resolve);
                    break;
                default:
                    Debug.WriteLine("Mode not valid");
                    break;
            }
        }

        /// <summary>
        /// Appends .bkp to the file extension to prevent usage
        /// </summary>
        /// <param name="file"></param>
        /// <param name="resolve"></param>
        public static void Disable(FileInfo file, bool resolve = false) { Rename(file, file.Name.TrimEnd(".bkp".ToCharArray()) + ".bkp", resolve); }

        /// <summary>
        /// Removes .bkp from the file extension to enable usage
        /// </summary>
        /// <param name="file"></param>
        /// <param name="resolve"></param>
        public static void Enable(FileInfo file, bool resolve = false) { Rename(file, file.Name.TrimEnd(".bkp".ToCharArray()), resolve); }

        /// <summary>
        /// Returns the current file's extension and whether or not it is considered invalidated
        /// </summary>
        /// <param name="file"></param>
        /// <param name="invalidated"></param>
        /// <returns></returns>
        public static string GetExtension(FileInfo file, out bool invalidated) {
            invalidated = file.Extension.ToLowerInvariant() == ".bkp";
            string fileName = (invalidated ? file.Name.Substring(0, file.Name.Length - 4) : file.Name).ToLowerInvariant();
            return fileName.Substring(fileName.LastIndexOf("."[0])).TrimStart("."[0]);
        }

        #endregion

        #region Beatmap Processing

        #region Worker
        /// <summary>
        /// Creates a BackgroundWorker to process beatmap folders
        /// </summary>
        /// <param name="folders"></param>
        /// <param name="startIndex"></param>
        public void CreateWorker(DirectoryInfo[] folders, int startIndex = 0) {
            ModeControl.IsEnabled = false;
            ButtonsPanel.IsEnabled = false;

            Progress.Minimum = 0;
            Progress.Value = 0;
            Progress.Maximum = 100;
            Progress.IsIndeterminate = true;

            BackgroundWorker bW = new BackgroundWorker {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false
            };

            bW.DoWork += BW_DoWork;
            bW.ProgressChanged += BW_ProgressChanged;
            bW.RunWorkerCompleted += BW_RunWorkerCompleted;

            bW.RunWorkerAsync(new Tuple<BackgroundWorker, DirectoryInfo[], Mode, bool, int>(bW, folders, GetMode(), BlurDeleteVideos.IsChecked == true, startIndex));
            Debug.WriteLine("Running worker...");
        }

        /// <summary>
        /// Iterates through each folder and updates the worker's progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void BW_DoWork(object sender, DoWorkEventArgs e) {
            Debug.WriteLine("Starting work...");
            (BackgroundWorker worker, DirectoryInfo[] folders, Mode mode, bool blurDelete, int startIndex) = e.Argument as Tuple<BackgroundWorker, DirectoryInfo[], Mode, bool, int>;

            Debug.WriteLine("Doing work...");
            worker.ReportProgress(0);
            Debug.WriteLine("<<START>>");
            for (int i = startIndex; i < folders.Length; i++) {
                try {
                    ManageBeatmap(folders[i], mode, blurDelete);
                    int progress = (int)(i * 100.0 / folders.Length);
                    Debug.WriteLine(progress + "% -------------- " + progress + "%");
                    worker.ReportProgress((int)(i * 100.0 / folders.Length));
#pragma warning disable CA1031 // Do not catch general exception types
                } catch {
                    e.Result = new Tuple<DirectoryInfo[], int>(folders, i);
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
            Debug.WriteLine("<<END>>");
            worker.ReportProgress(100);
            Debug.WriteLine("Work complete...");
        }

        /// <summary>
        /// Stops progress bar animation and continues in new worker after 3 seconds if failed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            ModeControl.IsEnabled = true;
            ButtonsPanel.IsEnabled = true;
            Progress.IsIndeterminate = false;

            ExceptionWindow.AutoShow();

            if (e.Error != null) {
                Debug.WriteLine("Error was caught, restarting in 3 seconds");
                (DirectoryInfo[] folders, int lastIndex) = e.Result as Tuple<DirectoryInfo[], int>;
                Thread.Sleep(3000);
                CreateWorker(folders, lastIndex + 1);
            }
        }

        /// <summary>
        /// Changes the progress bar value and text to reflect the worker's current progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BW_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            Dispatcher.Invoke(() => {
                Progress.Value = e.ProgressPercentage;
                ProgressLabel.Content = e.ProgressPercentage + "%";
            });
        }

        #endregion

        #region Beatmap Related Functions

        /// <summary>
        /// Searches the specified beatmap directory for .osu files and manages them if found
        /// </summary>
        /// <param name="beatmap"></param>
        /// <param name="mode"></param>
        /// <param name="blurDelete"></param>
        /// <returns></returns>
        public static void ManageBeatmap(DirectoryInfo beatmap, Mode mode, bool blurDelete = false) {
            Debug.WriteLine("Managing song: " + beatmap.Name + " [Exists: " + beatmap.Exists + "]");
            FileInfo[] files = beatmap.GetFiles("*.os*");
            foreach (FileInfo file in files) {
                string search = GetExtension(file, out bool invalid);
                if (file == null) { continue; }
                //Debug.WriteLine("Search: " + search + " Invalid: " + invalid);
                switch (search) {
                    case "osu":
                        if (invalid) {
                            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                            switch (mode) {
                                case Mode.Enable:
                                    Enable(file, true);
                                    continue;
                                case Mode.Delete:
                                    file.Delete();
                                    continue;
                            }
                        }

                        Debug.WriteLine("\tWill Edit Osu File: " + file.Name);
                        EditSong(file, mode, blurDelete);
                        Debug.WriteLine("Managed: '" + file.Name + "' Successfully");
                        break;
                }
            }
            Debug.WriteLine("<MANAGED>");
        }

        /// <summary>
        /// Returns whether or not the specified line of string is a considered a background event
        /// </summary>
        /// <param name="text"></param>
        /// <param name="smart"></param>
        /// <returns></returns>
        public static bool IsBackground(string text, bool smart = false) => IsBackground(text, smart, out _);

        /// <summary>
        /// Returns whether or not the specified line of string is a considered a background event along with the found media file
        /// </summary>
        /// <param name="text"></param>
        /// <param name="smart"></param>
        /// <param name="mediaFile"></param>
        /// <returns></returns>
        public static bool IsBackground(string text, bool smart, out FileInfo mediaFile) {
            mediaFile = null;
            string[] split = text.ToLowerInvariant().Split(","[0]);

            if (split == null || split.Length < 3) { return false; }

            string mediaType = split[0].TrimStart(smart ? "/"[0] : " "[0]);
            if (mediaType != "video" && !int.TryParse(mediaType, out _)) { return false; }
            if (!int.TryParse(split[1], out _)) { return false; }

            if (split[2].StartsWith("\"") && split[2].EndsWith("\"")) {
                mediaFile = new FileInfo(split[2].Trim('"'));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Edits the given .osu file by searching for the background events and processing file's relevant lines and linked files
        /// </summary>
        /// <param name="osuFile"></param>
        /// <param name="mode"></param>
        /// <param name="blurDelete"></param>
        public static void EditSong(FileInfo osuFile, Mode mode = Mode.Disable, bool blurDelete = false) {
            string[] lines = File.ReadAllLines(osuFile.FullName);
            List<string> newLines = new List<string>();

            string stage = "";
            foreach (string l in lines) {
                bool commentLine = false;
                if (l.ToLowerInvariant().StartsWith("[")) {
                    stage = l.ToLowerInvariant().TrimStart("["[0]).TrimEnd("]"[0]);
                }
                if (stage == "events") {
                    if (IsBackground(l, true, out FileInfo m)) {
                        FileInfo media = GetMediaFile(m, osuFile);
                        if (media == null || !media.Exists) {
                            if (mode != Mode.Delete && mode != Mode.Blur) {
                                SafeException exc = new SafeException("FileNotFound: " + osuFile.Name, "File: '" + m.Name + "' referenced in: '" + osuFile.Name + "' does not exist.");
                                Debug.WriteLine("\t\t" + exc);
                                ExceptionWindow.Append(exc, false);
                            }
                            commentLine = true;
                        } else {
                            switch (mode) {
                                case Mode.Disable:
                                    Disable(media, true);
                                    break;
                                case Mode.Enable:
                                    Enable(media, true);
                                    break;
                                case Mode.Delete:
                                    media.Delete();
                                    break;
                                case Mode.Blur:
                                    Debug.WriteLine("Blurring Media: " + media.FullName);
                                    switch (media.Extension.ToLowerInvariant().TrimStart('.')) {
                                        case "bmp":
                                        case "jpg":
                                        case "png":
                                            Blur(media, BlurAmount);
                                            break;
                                        default:
                                            if (blurDelete) { media.Delete(); }
                                            commentLine = true;
                                            break;
                                    }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        if (mode == Mode.Disable || mode == Mode.Delete) {
                            Debug.WriteLine("\tComment out: " + l);
                            commentLine = true;
                        }
                    }
                    
                }

                newLines.Add((commentLine ? "//" : "") + l.TrimStart('/'));
            }

            if (File.ReadAllLines(osuFile.FullName) == newLines.ToArray()) {
                Debug.WriteLine("<<NO CHANGES NECESSARY>>");
            } else {
                string fN = osuFile.FullName; //Preserve filename as invalidation changes it
                if (mode == Mode.Delete) { osuFile.Delete(); } else { Disable(osuFile); }
                File.WriteAllLines(fN, newLines);
            }
        }

        /// <summary>
        /// Returns a relative media file, making sure to check for invalidation
        /// </summary>
        /// <param name="media"></param>
        /// <param name="beatmap"></param>
        /// <returns></returns>
        public static FileInfo GetMediaFile(FileInfo media, FileInfo beatmap) {
            string check = beatmap.DirectoryName + "//" + media.Name;
            return File.Exists(check) ? new FileInfo(check) : File.Exists(check + ".bkp") ? new FileInfo(check + ".bkp") : null;
        }
        #endregion

        #endregion

        #region Media Filters
        public static void Blur(FileInfo file, int amount = 10) {
            if (file == null || !file.Exists) { return; }
            Debug.WriteLine("[B] Invalidating: " + file.FullName);
            File.WriteAllBytes(file.FullName + ".bkp", File.ReadAllBytes(file.FullName));
            ImageFormat format = null;
            Debug.WriteLine("[B] Iterating through formats");
            switch (file.Extension.ToLowerInvariant().TrimStart('.')) {
                case "bmp":
                    format = ImageFormat.Bmp;
                    break;
                case "jpg":
                    format = ImageFormat.Jpeg;
                    break;
                case "png":
                    format = ImageFormat.Png;
                    break;
            }
            if (format == null) { return; }
            FileInfo copy = file.CopyTo(file.DirectoryName + "//" + file.Name.Replace(".", "-temp."));
            using (Bitmap bmp = new Bitmap(copy.FullName)) {
                GaussianBlur blur = new GaussianBlur(bmp);
                Bitmap result = blur.Process(amount);
                using (FileStream fs = file.OpenWrite()) {
                    result.Save(fs, format);
                }
            }
            copy.Delete();
        }
        #endregion


        #region XAML Handlers
        void ButtonSelect_Click(object sender, RoutedEventArgs e) {
            DirectoryInfo[] dirs = GetFolder(true).ToArray();
            if (dirs == null || dirs.Length < 1) { Debug.Write("Cancelled"); return; }

            CreateWorker(dirs);
        }

        void ButtonFolder_Click(object sender, RoutedEventArgs e) {
            DirectoryInfo dirs = GetFolder(false).FirstOrDefault();
            if (dirs == null || !dirs.Exists) { Debug.Write("Cancelled"); return; }
            DirectoryInfo[] subDirs = dirs.GetDirectories().Where(d => d.Exists).ToArray();

            CreateWorker(subDirs);
        }

        void MetroWindow_PreviewKeyUp(object sender, KeyEventArgs e) {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (e.Key) {
                case Key.F1:
                    UpdateChecker.GotoPage();
                    break;
                case Key.F2:
                    Dispatcher.Invoke(() => ExceptionWindow.instance?.Show());
                    break;
            }
        }

        void MetroWindow_Closing(object sender, CancelEventArgs e) {
            ExceptionWindow.instance?.Close();
            UpdateChecker.instance?.Close();
        }

        void WarningPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { //Progress bar is 'indeterminate' when backgroundworker is processing beatmaps
            if (!Progress.IsIndeterminate) { ExceptionWindow.instance?.Show(); }
        }
        #endregion

        void WarningPanel_MouseEnter(object sender, MouseEventArgs e) => WarningPanel.Cursor = Progress.IsIndeterminate ? Cursors.Arrow : Cursors.Hand;
    }
}
