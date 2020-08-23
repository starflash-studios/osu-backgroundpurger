#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

#endregion

namespace Osu_BackgroundPurge.Windows {
    public partial class LogWindow {
        public LogWindow() {
            InitializeComponent();
            LogEvent += LogWindow_OnLog;
        }

        public delegate void OnLogDelegate(string Message, string Category = "INFO", DateTime SendTime = default);
        public static event OnLogDelegate LogEvent;

        public static bool CatchLogs { get; private set; } = true;

        public bool DisplayToolBarInfo = true;

        void LogWindow_OnLog(string Message, string Category, DateTime SendTime) {
            Dispatcher.Invoke(() => {
                Recent.IsEnabled = true;
                RecentTime.Content = SendTime.ToString("T");
                RecentMsg.Text = $"[{Category}] {SendTime:T}：{Message}";

                if (!CatchLogs) { return; }
                TextBlock.Text += $"\n[{Category}] {SendTime:T}：{Message}";
                TextBlockScroller.ScrollToEnd();
                TextBlockScroller.ScrollToLeftEnd();

                BtnClearAll.IsEnabled = true;
                BtnSave.IsEnabled = true;
            }, DispatcherPriority.Background);
        }

        void TglCatchLogs_Click(object Sender, RoutedEventArgs E) {
            if (Sender is ToggleButton TB) {
                CatchLogs = TB.IsChecked ?? false;
            }
        }

        void TglToolBarInfo_Click(object Sender, RoutedEventArgs E) {
            if (Sender is ToggleButton TB) {
                DisplayToolBarInfo = TB.IsChecked ?? false;

                UpdateLabelVisibility(LblCatchLogs);
                UpdateLabelVisibility(LblClearAll);
                UpdateLabelVisibility(LblOpen);
                UpdateLabelVisibility(LblSave);
            }
        }

        void BtnClearAll_Click(object Sender, RoutedEventArgs E) {
            TextBlock.Text = string.Empty;

            BtnClearAll.IsEnabled = false;
            BtnSave.IsEnabled = false;
        }

        FileInfo _Saved = null;
        void BtnSave_Click(object Sender, RoutedEventArgs E) {
            string T = TextBlock.Text;
            if (T.IsNullOrEmpty()) { return; }

            //If no welcome (version) message is present, prepend it to the log before saving.
            T += $"{_WelcomeMessage}\n\n";

            _Saved = new FileInfo(Path.Combine(StringToFileSystemInfoTypeConverter.Desktop().FullName, $"Debug {DateTime.Now.ToString("g").ToSafeFileName()}.log"));
            _Saved.WriteAllText(T);

            BtnOpen.IsEnabled = _Saved.Exists();
        }

        void BtnOpen_Click(object Sender, RoutedEventArgs E) {
            if (_Saved.Exists()) {
                Process.Start(_Saved.FullName); //Opens the log file with the default text editor
            } else { //If the open button was pressed, and the log file somehow doesn't exist, create the log and open it again.
                BtnSave_Click(Sender, E);
                // ReSharper disable once TailRecursiveCall
                BtnOpen_Click(Sender, E);
            }
        }

        public void UpdateLabelVisibility(Label Lbl) => Lbl.Visibility = DisplayToolBarInfo ? Visibility.Visible : Visibility.Collapsed;

        void MetroWindow_Closed(object Sender, EventArgs E) => LogEvent -= LogWindow_OnLog;

        public static void Log(string Message, string Category = "INFO", DateTime? SendTime = null) {
            LogEvent?.Invoke(Message, Category, SendTime ?? DateTime.Now);

            #if (DEBUG)
            Debug.WriteLine(Message, Category);
            #endif
        }

        static readonly string _WelcomeMessage = $">> Running Osu!BackgroundPurge v{Assembly.GetEntryAssembly().GetName().Version} <<";
        void MetroWindow_IsVisibleChanged(object Sender, DependencyPropertyChangedEventArgs E) {
            if (TextBlock.Text.IsNullOrEmpty()) {
                Log(_WelcomeMessage, "INIT");
            }
        }
    }
}
