using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace OsuBackgroundPurger {
    public partial class ExceptionWindow {
        public static ExceptionWindow instance;
        public static List<SafeException> exceptions = new List<SafeException>();

        public delegate void OnCaught(SafeException exception);
        public static OnCaught onCaught;

        public delegate void OnChange(List<SafeException> exceptions);
        public static OnChange onChange;

        public ExceptionWindow() {
            InitializeComponent();
        }

        #region Global Functions
        /// <summary>
        /// Creates an ExceptionWindow
        /// </summary>
        public static void Create() {
            instance?.Close();
            ExceptionWindow window = new ExceptionWindow();
            instance = window;
        }

        /// <summary>
        /// Appends an exception to the list and shows the window if specified
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="autoShow"></param>
        public static void Append(SafeException ex, bool autoShow = true) {
            //Debug.WriteLine("Appending: " + ex + " and showing? " + autoShow);
            if (instance == null) { Create(); }
            Debug.WriteLine("Caught: " + ex);
            exceptions.Add(ex);
            onCaught?.Invoke(ex);
            onChange?.Invoke(exceptions);

            if (instance.IsVisible) {
                instance.Redraw();
            } else if (autoShow) {
                AutoShow();
            }
        }

        /// <summary>
        /// Shows the window if there are any exceptions present within the list
        /// </summary>
        public static void AutoShow() {
            if (instance == null) { Create(); return; }
            if (!instance.IsVisible) {
                if (exceptions != null && exceptions.Count > 0) {
                    instance.Show();
                    instance.Redraw();
                }
            } else {
                instance.Redraw();
            }
        }
        #endregion

        /// <summary>
        /// Returns a string containing the value and it's word with or without an 's' at the end
        /// </summary>
        /// <param name="value"></param>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string PluralCount(int value, string word) => value + word + (value == 1 ? "" : "s");

        #region Exception Logging
        /// <summary>
        /// Creates a Log file save dialog
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static SaveFileDialog CreateLog(string fileName) => new SaveFileDialog {
            Title = "Save '" + fileName + "'",
            ValidateNames = true,
            FileName = fileName,
            Filter = "Log File (*.log)|*.log|Text File (*.txt)|*.txt|Any File (*.*)|*.*",
            DefaultExt = "log",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        /// <summary>
        /// Logs a single exception and prompts the user to save the file
        /// </summary>
        /// <param name="ex"></param>
        public static void Log(SafeException ex) {
            SaveFileDialog sfd = CreateLog(ex.Name + ".log");
            if (sfd.ShowDialog() == true) {
                File.WriteAllText(sfd.FileName, ex.ToString());
            }
        }


        public static void LogAll(List<SafeException> exceptions) {
            if (exceptions == null || exceptions.Count <= 0) { return; }
            SaveFileDialog sfd = CreateLog("[" + PluralCount(exceptions.Count, " Exception") + "] " + DateTime.Now.ToString(CultureInfo.InvariantCulture).Replace('/', '-').Replace(':', '-') + ".log");
            if (sfd.ShowDialog() == true) {
                List<string> lines = new List<string>();
                foreach (SafeException ex in exceptions) {
                    lines.Add("[[" + ex.Name + "]]");
                    lines.Add("\t" + ex.Display.Replace("\n", "\n\t"));
                    lines.Add("");
                }
                if (lines.Count > 0) {
                    File.WriteAllLines(sfd.FileName, lines);
                }
            }
        }
        #endregion

        #region UI Functions
        /// <summary>
        /// Redraws Application UI
        /// </summary>
        public void Redraw() {
            if (exceptions == null) {
                exceptions = new List<SafeException>();
                onChange?.Invoke(exceptions);
            }

            if (exceptions.Count > 0) {
                Title = PluralCount(exceptions.Count, "!exception");
                ActionsBulk.IsEnabled = true;

                ExceptionList.Items.Clear();
                foreach (SafeException ex in exceptions) {
                    ExceptionList.Items.Add(ex.Name);
                }
            } else {
                Title = "some!exceptions";
                ActionsBulk.IsEnabled = false;
                ExceptionList.Items.Clear();
            }
        }

        /// <summary>
        /// Views the specified exception within the 'ExceptionSingle' textbox
        /// </summary>
        /// <param name="ex"></param>
        public void View(SafeException? ex) {
            ExceptionSingle.Text = ex != null ? ex.Value.Display : "";
        }

        /// <summary>
        /// Returns the selected exception from the list (or null if none is selected)
        /// </summary>
        /// <param name="found"></param>
        /// <returns></returns>
        public bool GetSelected(out SafeException found) {
            int sel = ExceptionList.SelectedIndex;
            if (sel >= 0 && exceptions.Count > sel) {
                found = exceptions[sel];
                return true;
            }

            found = default;
            return false;
        }
        #endregion

        #region UI Handlers
        void ActionsSingleLog_Click(object sender, RoutedEventArgs e) {
            if (GetSelected(out SafeException ex)) {
                Log(ex);
                Redraw();
            }
        }

        void ActionsSingleClear_Click(object sender, RoutedEventArgs e) {
            if (GetSelected(out SafeException ex)) {
                exceptions.Remove(ex);
                onChange?.Invoke(exceptions);
                Redraw();
            }
        }

        void ActionsBulkLog_Click(object sender, RoutedEventArgs e) {
            LogAll(exceptions);
        }

        void ActionsBulkClear_Click(object sender, RoutedEventArgs e) {
            exceptions = new List<SafeException>();
            onChange?.Invoke(exceptions);
            View(null);
            Redraw();
        }

        void ButtonExit_Click(object sender, RoutedEventArgs e) {
            instance = null;
            Close();
        }

        void ExceptionList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (GetSelected(out SafeException ex)) {
                ActionsSingle.IsEnabled = true;
                View(ex);
            } else {
                ActionsSingle.IsEnabled = false;
            }

            ActionsBulk.IsEnabled = exceptions != null && exceptions.Count > 0;
        }

        void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) => instance = null;
        #endregion
    }

    public struct SafeException {
        public readonly string Name;
        public readonly string Display;

        public SafeException(string name, string description) {
            Name = name;
            Display = description;
        }

        public SafeException(string name, Exception ex) {
            Name = name;
            Display = ex.ToString();
        }

        public SafeException(Exception ex) {
            Name = ex.Source;
            Display = ex.ToString();
        }

        public override string ToString() => Name;

        public override bool Equals(object obj) {
            if (obj is SafeException sE) {
                return Name == sE.Name && Display == sE.Display;
            }
            return false;
        }

        public override int GetHashCode() => (int)(Math.Pow(2, Name.GetHashCode()) * Math.Pow(3, Display.GetHashCode()));

        public static bool operator ==(SafeException left, SafeException right) => left.Equals(right);

        public static bool operator !=(SafeException left, SafeException right) => !(left == right);
    }
}
