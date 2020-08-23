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
using System.Reflection;
using System.Windows;
using Osu_BackgroundPurge.UserControls;

#endregion

namespace Osu_BackgroundPurge.Windows {
    public partial class ExceptionWindow {
        public static ExceptionWindow Instance;

        static List<CaughtException> _CaughtExceptions = new List<CaughtException>();
        static bool _CaughtAny = false;

        public static List<CaughtException> CaughtExceptions {
            get => _CaughtExceptions;
            set {
                _CaughtExceptions = value ?? new List<CaughtException>();
                Debug.WriteLine($"Exceptions set to: '{string.Join(", ", _CaughtExceptions)}'");
                Debug.WriteLine("AAA");
                if (_CaughtExceptions.Count > 0) {
                    Application.Current.Dispatcher.Invoke(() => {
                        if (Instance == null) { Instance = new ExceptionWindow(); }

                        Debug.WriteLine("\t=> Updating List...");
                        Instance.UpdateList();
                        Instance.Show();
                    });
                }
            }
        }

        public ExceptionWindow() {
            if (Instance != null) {
                Instance.Show();
            } else {
                Instance = this;

                InitializeComponent();
                PrepareEmergencyLog();
            }
        }

        public ExceptionWindow(Exception E) {
            CaughtExceptions.Add(new CaughtException(E));

            if (Instance != null) {
                Instance.Show();
            } else {
                Instance = this;

                InitializeComponent();
                PrepareEmergencyLog();
            }
        }

        public ExceptionWindow(IEnumerable<Exception> E) {
            foreach (Exception Ex in E) {
                CaughtExceptions.Add(new CaughtException(Ex));
            }

            if (Instance != null) {
                Instance.Show();
            } else {
                Instance = this;

                InitializeComponent();
                PrepareEmergencyLog();
            }
        }

        static FileInfo _EmergencyDropoff;
        public void PrepareEmergencyLog() {
            _EmergencyDropoff = new FileInfo($"{new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName}\\Crash.log");

            if (!_EmergencyDropoff.Exists) {
                _EmergencyDropoff.WriteAllLines(new [] {
                    "This is an emergency crash log file.",
                    "Whenever an exception occurs, and it was unable to be resolved, the exception is logged here.",
                    "",
                    "Please post any issues to: https://github.com/starflash-studios/osu-backgroundpurger/issues/",
                    "",
                    "Exceptions are logged as follows: ",
                    "[DateTime]：HRESULT　・　HelpLink　・　Source　・　TargetSite：",
                    "\tMessage",
                    "\tStackTrace",
                    "",
                    "//―――――――「ＬＯＧ　ＳＴＡＲＴ」――――――――//"});
            }

            

            if (Application.Current.TryGetWindow(out MainWindow MW)) {
                MW.Closed += MW_Closed;
            } else {
                Debug.WriteLine("MainWindow not found.");
            }

        }

        protected static void Add(Exception E) {
            List<CaughtException> Old = CaughtExceptions;
            Old.Add(new CaughtException(E));
            CaughtExceptions = Old;
        }

        protected static void AddRange(IEnumerable<Exception> Ex) {
            List<CaughtException> Old = CaughtExceptions;
            Old.AddRange(Ex.Select(E => new CaughtException(E)));
            CaughtExceptions = Old;
        }

        //Forcefully eject caught exceptions into file in case application is unrecoverable.
        public static void Catch(Exception E) {
            _CaughtAny = true;
            _EmergencyDropoff.AppendAllLines(new[] { 
                "",
                "ーーーーーー",
                $"[{DateTime.Now:G}]：{E.HResult}　・　{E.HelpLink}　・　{E.Source}　・　{E.TargetSite}：",
                $"\t{E.Message.Replace("\n", "\n\t")}",
                $"\t{E.StackTrace.Replace("\n", "\n\t")}",
                ""
            });
            //Forcefully eject caught exceptions into file in case application is unrecoverable.

            Add(E);
        }

        public static void Catch(List<Exception> Exceptions) {
            _CaughtAny = true;
            //Forcefully eject caught exceptions into file in case application is unrecoverable.
            foreach (Exception E in Exceptions) {
                _EmergencyDropoff.AppendAllLines(new[] {
                    "",
                    "ーーーーーー",
                    $"[{DateTime.Now:G}]：{E.HResult}　・　{E.HelpLink}　・　{E.Source}　・　{E.TargetSite}：",
                    $"\t{E.Message.Replace("\n", "\n\t")}",
                    $"\t{E.StackTrace.Replace("\n", "\n\t")}",
                    ""
                });
            }
            
            AddRange(Exceptions);
        }

        public void UpdateList() {
            Debug.WriteLine("Updating list...");
            List<object> Exceptions = CaughtExceptions.Cast<object>().ToList();
            Debug.WriteLine($"\t=>Set to: {Exceptions}");
            ExceptionsList.SetList(Exceptions);
        }

        void ExceptionsList_SelectionChanged(object Sender, AppendableSelectionChangedEventArgs E) {
            if (E.SelectedItem != null) {
                PreviewException((CaughtException)E.SelectedItem);
            }
        }

        public void PreviewException(CaughtException E) {
            Preview.IsEnabled = E != null;
            if (E != null) {
                PreviewHResult.Value = E.HResult;

                PreviewHelpLink.Content = E.HelpLink;
                _SavedLink = E.HelpLink;

                PreviewSource.Text = E.Source;

                PreviewTargetSite.Text = E.TargetSite.ToString();

                PreviewStackTrace.Text = E.StackTrace;

                PreviewMessage.Text = E.Message;
            }
        }

        string _SavedLink = string.Empty;

        void PreviewHelpLink_Click(object Sender, RoutedEventArgs E) {
            if (!_SavedLink.IsNullOrEmpty()) {
                Process.Start(_SavedLink); //Opens the help link in the default browser.
            }
        }

        void MW_Closed(object Sender, EventArgs E) {
            if (_CaughtAny && _EmergencyDropoff.Exists()) {
                Process.Start(_EmergencyDropoff.FullName);
            }
        }
    }

    public readonly struct CaughtException : IEquatable<CaughtException> {
        public readonly Exception E;

        public CaughtException(Exception E = null) => this.E = E;

        public int HResult => E?.HResult ?? -1;
        public string HelpLink => E?.HelpLink ?? string.Empty;
        public string Source => E?.Source ?? string.Empty;
        public MethodBase TargetSite => E?.TargetSite;
        public string StackTrace => E?.StackTrace ?? string.Empty;
        public string Message => E?.Message ?? string.Empty;

        public override string ToString() => $"{nameof(CaughtException)}: {E}";

        public override bool Equals(object Obj) => Obj is CaughtException Exception && Equals(Exception);

        public override int GetHashCode() => E != null ? E.GetHashCode() : 0;

        public bool Equals(CaughtException Other) => EqualityComparer<Exception>.Default.Equals(E, Other.E);

        public static bool operator ==(CaughtException Left, CaughtException Right) => Left.Equals(Right);

        public static bool operator !=(CaughtException Left, CaughtException Right) => !(Left == Right);
    }
}
