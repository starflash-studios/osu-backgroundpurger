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
        public static List<Exception> exceptions = new List<Exception>();

        public ExceptionWindow() {
            InitializeComponent();
        }

        #region Global Functions
        /// <summary>
        /// Creates an ExceptionWindow
        /// </summary>
        public static void Create() {
            if (instance != null) { throw new NotImplementedException("Multiple ExceptionWindows"); }
            ExceptionWindow window = new ExceptionWindow();
            instance = window;
        }

        /// <summary>
        /// Appends an exception to the list and shows the window if specified
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="show"></param>
        public static void Append(Exception ex, bool show = true) {
            if (instance == null) { Create(); }
            if (show && !instance.IsVisible) { instance.Show(); }
            Debug.WriteLine("Caught: " + ex);
            exceptions.Add(ex);
            instance.Redraw();
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
        public static void Log(Exception ex) {
            if (ex == null) { return; }
            SaveFileDialog sfd = CreateLog(ex.Message + ".log");
            if (sfd.ShowDialog() == true) {
                File.WriteAllText(sfd.FileName, ex.ToString());
            }
        }


        public static void LogAll(List<Exception> exceptions) {
            if (exceptions == null || exceptions.Count <= 0) { return; }
            SaveFileDialog sfd = CreateLog("[" + PluralCount(exceptions.Count, " Exception") + "] " + DateTime.Now.ToString(CultureInfo.InvariantCulture).Replace('/', '-').Replace(':', '-') + ".log");
            if (sfd.ShowDialog() == true) {
                List<string> lines = new List<string>();
                foreach (Exception ex in exceptions) {
                    lines.Add("[[" + ex.Message + "]]");
                    lines.Add("\t" + ex.ToString().Replace("\n", "\n\t"));
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
                exceptions = new List<Exception>();
            }

            if (exceptions.Count > 0) {
                Title = PluralCount(exceptions.Count, "!exception");
                ActionsBulk.IsEnabled = true;

                ExceptionList.Items.Clear();
                foreach (Exception ex in exceptions) {
                    ExceptionList.Items.Add(ex.Message);
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
        public void View(Exception ex) {
            ExceptionSingle.Text = ex != null ? ex.ToString() : "";
        }

        /// <summary>
        /// Returns the selected exception from the list (or null if none is selected)
        /// </summary>
        /// <param name="found"></param>
        /// <returns></returns>
        public bool GetSelected(out Exception found) {
            int sel = ExceptionList.SelectedIndex;
            if (sel >= 0 && exceptions.Count > sel) {
                found = exceptions[sel];
                return found != null;
            }

            found = null;
            return false;
        }
        #endregion

        #region UI Handlers
        void ActionsSingleLog_Click(object sender, RoutedEventArgs e) {
            if (GetSelected(out Exception ex)) {
                Log(ex);
                Redraw();
            }
        }

        void ActionsSingleClear_Click(object sender, RoutedEventArgs e) {
            if (GetSelected(out Exception ex)) {
                exceptions.Remove(ex);
                Redraw();
            }
        }

        void ActionsBulkLog_Click(object sender, RoutedEventArgs e) {
            LogAll(exceptions);
        }

        void ActionsBulkClear_Click(object sender, RoutedEventArgs e) {
            exceptions = new List<Exception>();
            View(null);
            Redraw();
        }

        void ButtonExit_Click(object sender, RoutedEventArgs e) {
            instance = null;
            Close();
        }

        void ExceptionList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (GetSelected(out Exception ex)) {
                ActionsSingle.IsEnabled = true;
                View(ex);
            } else {
                ActionsSingle.IsEnabled = false;
            }

            ActionsBulk.IsEnabled = exceptions != null && exceptions.Count > 0;
        }
        #endregion
    }
}
