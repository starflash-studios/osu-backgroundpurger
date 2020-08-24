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
    /// <summary>A <see cref="Window"/> handling debug-logging for this application.</summary>
    /// <seealso cref="Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class LogWindow {
        /// <summary>Initialises a new instance of the <see cref="LogWindow"/> class.</summary>
        public LogWindow() {
            InitializeComponent();
            LogEvent += LogWindow_OnLog;
        }


        /// <summary>Called when a new debug message is logged.</summary>
        /// <param name="Message">The message.</param>
        /// <param name="Category">The category.</param>
        /// <param name="SendTime">The send time.</param>
        public delegate void OnLogDelegate(string Message, string Category = "INFO", DateTime SendTime = default);

        /// <summary>Occurs when a new debug message is logged.</summary>
        public static event OnLogDelegate LogEvent;

        /// <summary>Gets a value indicating whether or not to catch debug statements in the window.
        /// <para/>This WILL slow down the application, and is only intended for debugging purposes.</summary>
        /// <value><c>True</c> if catch debug statements; otherwise, <c>false</c>.</value>
        public static bool CatchLogs { get; private set; } = true;

        /// <summary>Whether or not to display the full ToolBar button names.</summary>
        public bool DisplayToolBarInfo = true;

        /// <summary>If <see cref="CatchLogs"/> is <c>True</c>, method logs all new messages into the <see cref="TextBlock"/>.</summary>
        /// <param name="Message">The message.</param>
        /// <param name="Category">The category.</param>
        /// <param name="SendTime">The send time.</param>
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

        /// <summary>Handles the <c>Click</c> event of the <see cref="TglCatchLogs"/> control.
        /// <para/>Updates the related <see cref="CatchLogs"/> variable.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void TglCatchLogs_Click(object Sender, RoutedEventArgs E) {
            if (Sender is ToggleButton TB) {
                CatchLogs = TB.IsChecked ?? false;
            }
        }

        /// <summary>Handles the <c>Click</c> event of the <see cref="TglToolBarInfo"/> control.
        /// <para/>Updates the <see cref="Visibility"/> of related ToolBar labels.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void TglToolBarInfo_Click(object Sender, RoutedEventArgs E) {
            if (Sender is ToggleButton TB) {
                DisplayToolBarInfo = TB.IsChecked ?? false;

                UpdateLabelVisibility(LblCatchLogs);
                UpdateLabelVisibility(LblClearAll);
                UpdateLabelVisibility(LblOpen);
                UpdateLabelVisibility(LblSave);
            }
        }

        /// <summary>Handles the <c>Click</c> event of the <see cref="BtnClearAll"/> control.
        /// <para/>Clears the currently-displayed messages in <see cref="TextBlock"/>.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void BtnClearAll_Click(object Sender, RoutedEventArgs E) {
            TextBlock.Text = string.Empty;

            BtnClearAll.IsEnabled = false;
            BtnSave.IsEnabled = false;
        }

        /// <summary>The location of the most recently-saved log file. Ensure file exists before accessing it, as the user may have deleted it.</summary>
        FileInfo _Saved = null;

        /// <summary>Handles the <c>Click</c> event of the <see cref="BtnSave"/> control.
        /// <para/>Saves the currently-displayed messages to a file on the desktop.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void BtnSave_Click(object Sender, RoutedEventArgs E) {
            string T = TextBlock.Text;
            if (T.IsNullOrEmpty()) { return; }

            //If no welcome (version) message is present, prepend it to the log before saving.
            T += $"{_WelcomeMessage}\n\n";

            _Saved = new FileInfo(Path.Combine(StringToFileSystemInfoTypeConverter.Desktop().FullName, $"Debug {DateTime.Now.ToString("g").ToSafeFileName()}.log"));
            _Saved.WriteAllText(T);

            BtnOpen.IsEnabled = _Saved.Exists();
        }

        /// <summary>Handles the <c>Click</c> event of the <see cref="BtnOpen"/> control.
        /// <para/>Opens the most recently saved log file in the default system Text Editor.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void BtnOpen_Click(object Sender, RoutedEventArgs E) {
            if (_Saved.Exists()) {
                Process.Start(_Saved.FullName); //Opens the log file with the default text editor
            } else { //If the open button was pressed, and the log file somehow doesn't exist, create the log and open it again.
                BtnSave_Click(Sender, E);
                // ReSharper disable once TailRecursiveCall
                BtnOpen_Click(Sender, E);
            }
        }

        /// <summary>Updates the visibility of the specified <paramref name="Lbl"/>, relative to <see cref="DisplayToolBarInfo"/>.</summary>
        /// <param name="Lbl">The label.</param>
        public void UpdateLabelVisibility(Label Lbl) => Lbl.Visibility = DisplayToolBarInfo ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>Handles the <c>Closed</c> event of this instance.
        /// <para/>Removes the assigned <see cref="LogWindow_OnLog(string, string, DateTime)"/> delegate method from <see cref="LogEvent"/>.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="EventArgs"/> instance containing the event data.</param>
        void MetroWindow_Closed(object Sender, EventArgs E) => LogEvent -= LogWindow_OnLog;

        /// <summary>Logs the specified message.</summary>
        /// <param name="Message">The message.</param>
        /// <param name="Category">The category.</param>
        /// <param name="SendTime">The send time.</param>
        public static void Log(string Message, string Category = "INFO", DateTime? SendTime = null) {
            LogEvent?.Invoke(Message, Category, SendTime ?? DateTime.Now);

            #if (DEBUG)
            Debug.WriteLine(Message, Category);
            #endif
        }

        /// <summary>The welcome message.</summary>
        static readonly string _WelcomeMessage = $">> Running Osu!BackgroundPurge v{Assembly.GetEntryAssembly().GetName().Version} <<";

        /// <summary>Handles the <c>IsVisibleChanged</c> event of this instance.
        /// <para/>Logs the <see cref="_WelcomeMessage"/>, showing the currently-installed version.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        void MetroWindow_IsVisibleChanged(object Sender, DependencyPropertyChangedEventArgs E) {
            if (TextBlock.Text.IsNullOrEmpty()) {
                Log(_WelcomeMessage, "INIT");
            }
        }
    }
}
