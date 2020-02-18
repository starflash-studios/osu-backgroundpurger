//This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program. If not, see<https://www.gnu.org/licenses/>.

//Starflash Studios, hereby disclaims all copyright interest in the program 'Osu!BackgroundPurger' (which is an automated osu!beatmap background remover) written by Cody Bock.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace OsuBackgroundPurger {
    public partial class MainWindow {
        public static bool ProcessFix;
        public static bool UpdateUI = true;
        public static MainWindow instance;

        public static int CurrentProcess;
        public static int CurrentProcessMax;

        public MainWindow() {
            InitializeComponent();
            instance = this;
            //Task.Run(CreateUpdater);
            Task.Run(UIThreadAsync);
            //Dispatcher.Invoke(UIThreadAsync);
        }

        public async void CreateUpdater() {
            Debug.WriteLine("Thread #2 - 1 / 3");
            Dispatcher.Invoke(() => {
                Debug.WriteLine("Thread #2 - 2 / 3");
                UpdateChecker.Create(null);
            });
            await Task.Delay(1000);
            Debug.WriteLine("Thread #2 - 3 / 3");
        }

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

        #region General Functions
        public static void Rename(FileInfo file, string newName, bool resolve = false) {
            if (file == null) { Debug.WriteLine("No file selected; Aborting."); return; }
            if (!file.Exists) { Debug.WriteLine("File: " + file.FullName + " Does not exist; Aborting."); return; }

            Debug.WriteLine("Passed Checks");
            FileInfo newFile = new FileInfo(file.Directory.FullName + "//" + newName);
            Debug.WriteLine("New File: " + newFile.Name);
            if (newFile.Exists) {
                if (resolve) {
                    newFile.Delete();
                    Task.Delay(500);
                    Debug.WriteLine("^ exists, Deleting");
                } else {
                    Debug.WriteLine("^ exists, Aborting");
                    return;
                }
            }
            file.MoveTo(newFile.FullName);
        }

        public static void SmartValidate(FileInfo file) {
            if (ProcessFix) {
                Debug.WriteLine("Validating: " + file.FullName);
                Validate(file);
            } else { Invalidate(file); }
        }

        public static void Invalidate(FileInfo file) { Rename(file, file.Name.TrimEnd(".bkp".ToCharArray()) + ".bkp"); }

        public static void Validate(FileInfo file) { Rename(file, file.Name.TrimEnd(".bkp".ToCharArray())); }

        public static string GetExtension(FileInfo file, out bool invalidated) {
            invalidated = file.Extension.ToLowerInvariant() == ".bkp";
            string fileName = (invalidated ? file.Name.Substring(0, file.Name.Length - 4) : file.Name).ToLowerInvariant();
            return fileName.Substring(fileName.LastIndexOf("."[0])).TrimStart("."[0]);
        }

        #endregion

        #region Management
        public static void BulkRemove(DirectoryInfo[] folders) {
            CurrentProcessMax = folders.Length;
            CurrentProcess = 0;
            foreach (DirectoryInfo f in folders) { _ = RemoveBackground(f); }
            Debug.WriteLine("<<COMPLETED>>");
        }

        public static Task RemoveBackground(DirectoryInfo song) {
            Debug.WriteLine("Managing song: " + song.Name + " [Exists: " + song.Exists + "]");
            foreach(FileInfo file in song.GetFiles()) {
                string search = GetExtension(file, out bool invalid);
                Debug.WriteLine("Search: " + search + " Invalid: " + invalid);
                switch (search) {
                    case "mp4":
                    case "avi":
                    case "mkv":
                        Debug.WriteLine("\tWill In/Validate Video: " + file.FullName);
                        SmartValidate(file);
                        //file.Delete();
                        break;
                    case "bmp":
                    case "gif":
                    case "jpg":
                    case "png":
                        Debug.WriteLine("\tWill In/Validate Image: " + file.FullName);
                        SmartValidate(file);
                        //file.Delete();
                        break;
                    case "ini":
                        if (file.Name.ToLowerInvariant() == "skin.ini") {
                            Debug.WriteLine("\tWill In/Validate Skin Ini: " + file.FullName);
                            SmartValidate(file);
                        }
                        break;
                    case "osu":
                        if (ProcessFix && invalid) { file.Delete(); break; }
                        Debug.WriteLine("\tWill Edit Osu File: " + file.Name);
                        EditSong(file);
                        break;
                }
            }
            Debug.WriteLine("<MANAGED>");
            CurrentProcess++;
            return null;
        }

        public static bool IsBackground(string text, bool smart = false) {
            int checks = 0;
            string[] split = text.ToLowerInvariant().Split(","[0]);
            if (split == null || split.Length < 3) { return false; }
            if (split[0] == null) { Debug.WriteLine("Huh??"); return false; }
            string first = split[0].TrimStart(smart ? "/"[0] : " "[0]);
            if (first == "video" || int.TryParse(first, out _)) {
                checks++;
            }
            if (int.TryParse(split[1], out _)) {
                checks++;
            }
            if (split[2].StartsWith("\"") && split[2].EndsWith("\"")) {
                checks++;
            }

            return checks == 3;
        }

        public static void EditSong(FileInfo osuFile) {
            string[] lines = File.ReadAllLines(osuFile.FullName);
            List<string> newLines = new List<string>();

            string stage = "";
            foreach (string l in lines) {
                bool skip = false;
                if (l.ToLowerInvariant().StartsWith("[")) {
                    stage = l.ToLowerInvariant().TrimStart("["[0]).TrimEnd("]"[0]);
                }
                if (stage == "events") {
                    //Debug.WriteLine("\t\tFound event: " + l);
                    if (IsBackground(l, ProcessFix)) {
                        Debug.WriteLine("\t\t\tFound background: " + l + " [Reversing: " + ProcessFix + "]");
                        skip = true;
                    }
                    
                }

                if (ProcessFix && skip) {
                    newLines.Add(l.TrimStart("/"[0]));
                } else {
                    newLines.Add((skip ? "//" : "") + l);
                }
            }

            string fileName = osuFile.FullName;
            Invalidate(osuFile);
            File.WriteAllLines(fileName, newLines);
        }
        #endregion

        #region Reversal

        #endregion

        #region UI
        public void UIThreadAsync() {
            UpdateUI = true;

            try {
                Dispatcher.Invoke(() => {
                    while (UpdateUI) {
                        Debug.WriteLine("----------------------------------------------: " + CurrentProcess + " / " + CurrentProcessMax);
                        SetProgress(CurrentProcess, CurrentProcessMax);
                        ExecuteWait(() => Thread.Sleep(300));
                    }
                });
            } catch {
                Debug.WriteLine("Left loop");
                UpdateUI = false;
                Task.Delay(1000).ContinueWith(t => UIThreadAsync());
            }
        }

        public static void ExecuteWait(Action action) {
            DispatcherFrame waitFrame = new DispatcherFrame();
            IAsyncResult op = action.BeginInvoke(dummy => waitFrame.Continue = false, null);
            Dispatcher.PushFrame(waitFrame);
            action.EndInvoke(op);
        }

        public void SetProgress(int value, int max = -1) {
            Progress.Minimum = 0;
            Progress.Maximum = max < 0 ? Progress.Maximum : max == Progress.Minimum ? Progress.Minimum + 1 : max;
            Progress.Value = value > Progress.Maximum ? Progress.Maximum : value;
            ProgressLabel.Content = value + " / " + Progress.Maximum;
        }

        public void IncrementProgress() => SetProgress((int)Progress.Value + 1, -1);

        void ButtonSingle_Click(object sender, RoutedEventArgs e) {
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

        void ToggleFix_Click(object sender, RoutedEventArgs e) {
            ProcessFix = !ProcessFix;
            ToggleFix.IsChecked = ProcessFix;
            ToggleLabel.Content = ProcessFix ? ">>" : "<<";
        }

        void ToggleLabel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ToggleFix_Click(sender, null);
        
        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => UpdateUI = false;
        #endregion
    }
}
