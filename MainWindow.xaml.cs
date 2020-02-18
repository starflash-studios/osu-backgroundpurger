using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.WindowsAPICodePack.Dialogs;

namespace OsuBackgroundPurger {
    public partial class MainWindow {
        public static bool ProcessFix;
        public static MainWindow instance;

        public MainWindow() {
            InitializeComponent();
            instance = this;
            _ = new UpdateChecker(this);
        }

        #region UI
        void ButtonSingle_Click(object sender, RoutedEventArgs e) {
            DirectoryInfo[] dirs = GetFolder(true).ToArray();
            if (dirs == null || dirs.Length < 1) { Debug.Write("Cancelled"); return; }
            BulkRemove(dirs);
        }

        void ButtonFolder_Click(object sender, RoutedEventArgs e) {
            DirectoryInfo dirs = GetFolder(false).FirstOrDefault();
            if (dirs == null || !dirs.Exists) { Debug.Write("Cancelled"); return; }
            DirectoryInfo[] subDirs = dirs.GetDirectories().Where(d => d.Exists).ToArray();
            BulkRemove(subDirs);
        }
        #endregion

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
        public static void Rename(FileInfo file, string newName, bool resolve = true) {
            string gen = file.Directory.FullName + "//" + newName;
            if (File.Exists(gen)) { if (resolve) { File.Delete(gen); } else { return; } }
            file.MoveTo(file.Directory.FullName + "//" + newName);
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

        public static void SetProgress(int value, int max = -1) {
            instance.Progress.Minimum = 0;
            instance.Progress.Maximum = max < 0 ? instance.Progress.Maximum : max;
            instance.Progress.Value = value;
        }

        #endregion

        #region Management
        public static void BulkRemove(DirectoryInfo[] folders) {
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

        void ToggleFix_Click(object sender, RoutedEventArgs e) {
            ProcessFix = !ProcessFix;
            ToggleFix.IsChecked = ProcessFix;
            ToggleLabel.Content = ProcessFix ? ">>" : "<<";
        }

        void ToggleLabel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ToggleFix_Click(sender, null);
    }
}
