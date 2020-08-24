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
    /// <summary>A <see cref="Window"/> handling exception-catching for this application.</summary>
    /// <seealso cref="Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class ExceptionWindow {
        /// <summary>The current instance.</summary>
        public static ExceptionWindow Instance;

        /// <summary>A list of all caught exceptions.</summary>
        static List<CaughtException> _CaughtExceptions = new List<CaughtException>();
        /// <summary>Whether or not any exceptions have been caught.</summary>
        static bool _CaughtAny = false;

        /// <summary>Gets or sets the list caught exceptions.</summary>
        /// <value>The caught exceptions <see cref="List{CaughtException}"/>.</value>
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

        /// <summary>Initialises a new instance of the <see cref="ExceptionWindow"/> class.</summary>
        public ExceptionWindow() {
            if (Instance != null) {
                Instance.Show();
            } else {
                Instance = this;

                InitializeComponent();
                PrepareEmergencyLog();
            }
        }

        /// <summary>Initialises a new instance of the <see cref="ExceptionWindow"/> class; passing the given <see cref="Exception"/>, <paramref name="E"/>, to <see cref="CaughtExceptions"/>.</summary>
        /// <param name="E">The <see cref="Exception"/> to catch.</param>
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

        /// <summary>Initialises a new instance of the <see cref="ExceptionWindow"/> class; passing the given <see cref="Exception"/>, <paramref name="E"/>, to <see cref="List{Exception}"/>.</summary>
        /// <param name="E">All <see cref="Exception"/>s to catch.</param>
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

        /// <summary>The emergency exception 'drop-off' location.
        /// <para/>The drop-off file is responsible for storing exceptions to a .log file; allowing the user to find the reason for crash, in the case the exception caused the application to be unrecoverable.</summary>
        static FileInfo _EmergencyDropoff;

        /// <summary>Prepares the <see cref="_EmergencyDropoff"/> .log file.</summary>
        static void PrepareEmergencyLog() {
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

        /// <summary>Adds the specified <see cref="Exception"/>, <paramref name="E"/>.</summary>
        /// <param name="E">The <see cref="Exception"/>.</param>
        protected static void Add(Exception E) {
            List<CaughtException> Old = CaughtExceptions;
            Old.Add(new CaughtException(E));
            CaughtExceptions = Old;
        }

        /// <summary>Adds the specified <see cref="Exception"/>s, <paramref name="Ex"/>.</summary>
        /// <param name="Ex">The <see cref="Exception"/>s to add.</param>
        protected static void AddRange(IEnumerable<Exception> Ex) {
            List<CaughtException> Old = CaughtExceptions;
            Old.AddRange(Ex.Select(E => new CaughtException(E)));
            CaughtExceptions = Old;
        }

        /// <summary>Catches the specified <see cref="Exception"/>, <paramref name="Exception"/>.
        /// <para/>Also stores the exception in the <see cref="_EmergencyDropoff"/> .log file in the case the application is unrecoverable.</summary>
        /// <param name="Exception">The <see cref="Exception"/>.</param>
        public static void Catch(Exception Exception) {
            _CaughtAny = true;
            _EmergencyDropoff.AppendAllLines(new[] { 
                "",
                "ーーーーーー",
                $"[{DateTime.Now:G}]：{Exception.HResult}　・　{Exception.HelpLink}　・　{Exception.Source}　・　{Exception.TargetSite}：",
                $"\t{Exception.Message.Replace("\n", "\n\t")}",
                $"\t{Exception.StackTrace.Replace("\n", "\n\t")}",
                ""
            });
            //Forcefully eject caught exceptions into file in case application is unrecoverable.

            Add(Exception);
        }

        /// <summary>Catches the specified <see cref="Exception"/>s, <paramref name="Exceptions"/>.
        /// <para/>Also stores the caught exceptions in the <see cref="_EmergencyDropoff"/> .log file in the case the application is unrecoverable.</summary>
        /// <param name="Exceptions">The <see cref="Exception"/>s.</param>
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

        /// <summary>Updates the <see cref="ExceptionsList"/>.</summary>
        public void UpdateList() {
            Debug.WriteLine("Updating list...");
            List<object> Exceptions = CaughtExceptions.Cast<object>().ToList();
            Debug.WriteLine($"\t=>Set to: {Exceptions}");
            ExceptionsList.Set(Exceptions);
        }

        /// <summary>Handles the <c>SelectionChanged</c> event of the <see cref="ExceptionsList"/> control.
        /// <para/>Calls <see cref="PreviewException(CaughtException)"/> on the newly-selected exception.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="AppendableSelectionChangedEventArgs"/> instance containing the event data.</param>
        void ExceptionsList_SelectionChanged(object Sender, AppendableSelectionChangedEventArgs E) {
            if (E.SelectedItem != null) {
                PreviewException((CaughtException)E.SelectedItem);
            }
        }

        /// <summary>Previews the exception.</summary>
        /// <param name="E">The <see cref="Exception"/>.</param>
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

        /// <summary>The most recently-saved HelpLink.</summary>
        string _SavedLink = string.Empty;

        /// <summary>Handles the <c>Click</c> event of the <see cref="PreviewHelpLink"/> control, opening the related help URL in the default system browser.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void PreviewHelpLink_Click(object Sender, RoutedEventArgs E) {
            if (!_SavedLink.IsNullOrEmpty()) {
                Process.Start(_SavedLink); //Opens the help link in the default browser.
            }
        }

        /// <summary>Handles the <c>Closed</c> event of the <see cref="MainWindow"/>.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="EventArgs"/> instance containing the event data.</param>
        static void MW_Closed(object Sender, EventArgs E) {
            if (_CaughtAny && _EmergencyDropoff.Exists()) {
                Process.Start(_EmergencyDropoff.FullName);
            }
        }
    }

    /// <summary>A struct representing a caught <see cref="Exception"/>.</summary>
    /// <seealso cref="System.IEquatable{CaughtException}" />
    public readonly struct CaughtException : IEquatable<CaughtException> {
        /// <summary>The caught <see cref="Exception"/>.</summary>
        public readonly Exception E;

        /// <summary>Initialises a new instance of the <see cref="CaughtException"/> struct.</summary>
        /// <param name="E">The <see cref="Exception"/>.</param>
        public CaughtException(Exception E = null) => this.E = E;

        /// <summary>Gets the HRESULT, a coded numerical value assigned to a specific exception, of the caught exception.</summary>
        /// <value><see cref="E"/>.<see cref="Exception.HResult"/></value>
        public int HResult => E?.HResult ?? -1;

        /// <summary>Gets a link to the help file associated with the caught exception.</summary>
        /// <value><see cref="E"/>.<see cref="Exception.HelpLink"/>.</value>
        public string HelpLink => E?.HelpLink ?? string.Empty;

        /// <summary>Gets the name of the application or the object that causes the error.</summary>
        /// <value><see cref="E"/>.<see cref="Exception.Source"/>.</value>
        public string Source => E?.Source ?? string.Empty;

        /// <summary>Gets the method that throws the current exception.</summary>
        /// <value><see cref="E"/>.<see cref="Exception.TargetSite"/>.</value>
        public MethodBase TargetSite => E?.TargetSite;

        /// <summary>Gets a string representation of the immediate frames on the call stack.</summary>
        /// <value><see cref="E"/>.<see cref="Exception.StackTrace"/>.</value>
        public string StackTrace => E?.StackTrace ?? string.Empty;

        /// <summary>Gets a message that describes the current exception.</summary>
        /// <value><see cref="E"/>.<see cref="Exception.Message"/>.</value>
        public string Message => E?.Message ?? string.Empty;

        /// <summary>Converts to string.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() => $"{nameof(CaughtException)}: {E}";

        #region Equality Members

        /// <summary>Determines whether the specified <see cref="System.Object" />, is equal to this instance.</summary>
        /// <param name="Obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns><c>True</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object Obj) => Obj is CaughtException Exception && Equals(Exception);

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. </returns>
        public override int GetHashCode() => E != null ? E.GetHashCode() : 0;

        /// <summary>Determines whether the specified <see cref="CaughtException" />, is equal to this instance.</summary>
        /// <param name="Other">The <see cref="CaughtException" /> to compare with this instance.</param>
        /// <returns><c>True</c> if the specified <see cref="CaughtException" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(CaughtException Other) => EqualityComparer<Exception>.Default.Equals(E, Other.E);

        /// <summary>Implements the operator ==.</summary>
        /// <param name="Left">The left.</param>
        /// <param name="Right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(CaughtException Left, CaughtException Right) => Left.Equals(Right);

        /// <summary>Implements the operator !=.</summary>
        /// <param name="Left">The left.</param>
        /// <param name="Right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(CaughtException Left, CaughtException Right) => !(Left == Right);

        #endregion
    }
}
