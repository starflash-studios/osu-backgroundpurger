//This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program. If not, see<https://www.gnu.org/licenses/>.

//Starflash Studios, hereby disclaims all copyright interest in the program 'Osu!BackgroundPurger' (which is an automated osu!beatmap background remover) written by Cody Bock.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Microsoft.WindowsAPICodePack.Dialogs;

using SuperfastBlur;

namespace OsuBackgroundPurger {
    public partial class MainWindow {
        public static bool UpdateUI = true;
        public static MainWindow instance;

        public static int CurrentProcess;
        public static int CurrentProcessMax;

        // ReSharper disable once PossibleInvalidOperationException
        public static int BlurAmount => (int)instance.BlurProcessInput.Value;

        public enum Mode {
            Disable,
            Enable,
            Delete,
            Blur
        }

        public Mode GetMode() => (Mode)ModeControl.SelectedIndex;

        public MainWindow() {
            InitializeComponent();
            instance = this;

            UpdateChecker.Create(Dispatcher, this);
            Task.Run(UIThreadAsync);
            //Dispatcher.Invoke(UIThreadAsync);
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
            Debug.WriteLine("New File: " + newFile.Name);
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
            Debug.WriteLine("Moving file: " + file.FullName + " to: " + nFn);
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
        /// <summary>
        /// Cycles through each folder and presumes that each child folder is a beatmap
        /// </summary>
        /// <param name="folders"></param>
        public void BulkRemove(DirectoryInfo[] folders) {
            Mode mode = GetMode();
            CurrentProcessMax = folders.Length;
            CurrentProcess = 0;
            Progress.IsIndeterminate = true;
            foreach (DirectoryInfo f in folders) {
                _ = ManageBeatmap(f, mode);
            }
            Progress.IsIndeterminate = false;
            Debug.WriteLine("<<COMPLETED>>");
        }

        /// <summary>
        /// Searches the specified beatmap directory for .osu files and manages them if found
        /// </summary>
        /// <param name="beatmap"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static Task ManageBeatmap(DirectoryInfo beatmap, Mode mode) {
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
                        try {
                            EditSong(file, mode);
                            Debug.WriteLine("Managed: '" + file.Name + "' Successfully");
#pragma warning disable CA1031 // Do not catch general exception types
                        } catch (Exception ex) {
                            ExceptionWindow.Append(new Exception(ex.InnerException.Source + ": " + file.Name, ex));
                            // ReSharper disable once RedundantJumpStatement
                            continue;
                        }
#pragma warning restore CA1031 // Do not catch general exception types
                        break;
                }
            }
            Debug.WriteLine("<MANAGED>");
            CurrentProcess++;
            return null;
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
        public static void EditSong(FileInfo osuFile, Mode mode = Mode.Disable) {
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
                            if (mode != Mode.Delete) {
                                Debug.WriteLine("Media not found");
                                ExceptionWindow.Append(new Exception("FileNotFound: " + osuFile.Name, new NullReferenceException(m.Name)));
                                commentLine = true;
                            }
                        } else {
                            switch (mode) {
                                case Mode.Disable:
                                    Disable(media, true);
                                    commentLine = true;
                                    break;
                                case Mode.Enable:
                                    Enable(media, true);
                                    break;
                                case Mode.Delete:
                                    media.Delete();
                                    commentLine = true;
                                    break;
                                case Mode.Blur:
                                    Debug.WriteLine("Blurring Media: " + media.FullName);
                                    switch (media.Extension.ToLowerInvariant().TrimStart('.')) {
                                        case"bmp":
                                        case"jpg":
                                        case"png":
                                            Blur(media, BlurAmount);
                                            break;
                                    }

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                    
                }

                if (commentLine) {
                    newLines.Add(l.TrimStart("/"[0]));
                } else {
                    newLines.Add((commentLine ? "//" : "") + l);
                }
            }

            if (File.ReadAllLines(osuFile.FullName) == newLines.ToArray()) {
                Debug.WriteLine("<<NO CHANGES NECESSARY>>");
            } else {
                string fN = osuFile.FullName; //Preserve filename as invalidation changes it
                Disable(osuFile);
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

        #region UI

        #region Application
        /// <summary>
        /// Thread to handle all global UI events
        /// </summary>
        public void UIThreadAsync() {
            UpdateUI = true;

            try {
                Dispatcher.Invoke(() => {
                    while (UpdateUI) {
                        //Debug.WriteLine("----------------------------------------------: " + CurrentProcess + " / " + CurrentProcessMax);
                        SetProgress(CurrentProcess, CurrentProcessMax);
                        ExecuteWait(() => Thread.Sleep(300));
                    }
                });
#pragma warning disable CA1031 // Do not catch general exception types
            } catch {
                Debug.WriteLine("Left loop");
                UpdateUI = false;
                Task.Delay(1000).ContinueWith(t => UIThreadAsync());
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        /// <summary>
        /// Awaits the current dispatcher thread
        /// </summary>
        /// <param name="action"></param>
        public static void ExecuteWait(Action action) {
            DispatcherFrame waitFrame = new DispatcherFrame();
            IAsyncResult op = action.BeginInvoke(dummy => waitFrame.Continue = false, null);
            Dispatcher.PushFrame(waitFrame);
            action.EndInvoke(op);
        }

        /// <summary>
        /// Sets the progressbar's value to the given amount (and maximum value if given)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        public void SetProgress(int value, int max = -1) {
            Progress.Minimum = 0;
            Progress.Maximum = max < 0 ? Progress.Maximum : max == Progress.Minimum ? Progress.Minimum + 1 : max;
            Progress.Value = value > Progress.Maximum ? Progress.Maximum : value;
            ProgressLabel.Content = value + " / " + Progress.Maximum;
        }

        /// <summary>
        /// Increments the current progressbar's value by one
        /// </summary>
        public void IncrementProgress() => SetProgress((int)Progress.Value + 1, -1);

        #endregion

        #region XAML Handlers
        void ButtonSelect_Click(object sender, RoutedEventArgs e) {
            CurrentProcessMax = 0;
            DirectoryInfo[] dirs = GetFolder(true).ToArray();
            if (dirs == null || dirs.Length < 1) { Debug.Write("Cancelled"); return; }
            BulkRemove(dirs);
        }

        void ButtonFolder_Click(object sender, RoutedEventArgs e) {
            CurrentProcessMax = 0;
            DirectoryInfo dirs = GetFolder(false).FirstOrDefault();
            if (dirs == null || !dirs.Exists) { Debug.Write("Cancelled"); return; }
            DirectoryInfo[] subDirs = dirs.GetDirectories().Where(d => d.Exists).ToArray();
            BulkRemove(subDirs);
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => UpdateUI = false;

        void MetroWindow_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == System.Windows.Input.Key.F1) { UpdateChecker.GotoPage(); }
        }
        #endregion

        #endregion
    }
}
