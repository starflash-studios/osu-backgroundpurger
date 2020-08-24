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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using Osu_BackgroundPurge.Pages;

#endregion

namespace Osu_BackgroundPurge.Windows {
    /// <summary>The main window for the program. This is the first thing the user sees.</summary>
    /// <seealso cref="MahApps.Metro.Controls.MetroWindow" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MainWindow {
        //TODO: Add beatmap folder function to OsuFileSystem.cs
        //TODO: Add skins   folder function to OsuFileSystem.cs
        //TODO: Implement systems into Osu!ModeManager as well.

        /// <summary>Initialises a new instance of the <see cref="MainWindow"/> class.</summary>
        public MainWindow() {
            InitializeComponent();

            MainTabs.SelectedIndex = 1;

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            _ = new ExceptionWindow();

            // ReSharper disable once CommentTypo
            //try {
            //    _ = new System.IO.FileInfo("\\:;:\\//:::dasdsa\\//???\\");
            //} catch (Exception E) {
            //    ExceptionWindow.Catch(E);
            //}

            //TestDrop.SelectionChanged += TestDrop_SelectionChanged;
            //if (OsuFileSystem.TryGetInstall(OsuFileSystem.OsuVersion.Classic, out FileInfo Classic)) {
            //    Debug.WriteLine($"Found 'Classic' install: '{Classic.FullName}'.");
            //    //Classic.Select();
            //}

            //if (OsuFileSystem.TryGetInstall(OsuFileSystem.OsuVersion.Lazer, out FileInfo Lazer)) {
            //    Debug.WriteLine($"Found 'Lazer' install: '{Lazer.FullName}'.");
            //    //Lazer.Select();
            //    foreach((Version Version, DirectoryInfo Path) in OsuFileSystem.GetLazerVersions(Lazer.Directory)) {
            //        Debug.WriteLine($"\t=> Found Lazer {Version}: {Path.FullName}.");
            //    }

            //    Debug.WriteLine($"Found 'Lazer' rulesets: '{(OsuFileSystem.TryGetLazerRulesetsPath(out DirectoryInfo Result) ? Result.FullName : string.Empty)}'.");
            //}

            ////_ = FileSystemExtensions.OpenFileBrowser();
            //foreach (FileInfo File in FileSystemExtensions.OpenFileBrowser(true)) {
            //    Debug.WriteLine($"\t=> Found : {File.FullName}.");
            //    foreach(string Line in System.IO.File.ReadAllLines(File.FullName)) {
            //        Debug.WriteLine($"\t\t{Line}");
            //    }
            //}

            ////Test();
        }

        /// <summary>Handles the <c>DispatcherUnhandledException</c> event of this <see cref="Window"/>.
        /// <para/>Calls <see cref="ExceptionWindow.Catch(Exception)"/>.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        static void Current_DispatcherUnhandledException(object Sender, DispatcherUnhandledExceptionEventArgs E) => ExceptionWindow.Catch(E.Exception);

        // ReSharper disable once FunctionRecursiveOnAllPaths
        //public void Test() {
        //    OpenFileDialog Temp = new OpenFileDialog {
        //        Filter = "osu! Beatmap (*.osu)|*.osu|Any File (*.*)|*.*",
        //        FilterIndex = 0,
        //        Title = "Select an osu! beatmap to analyse",
        //        CheckFileExists = true,
        //        CheckPathExists = true,
        //        ReadOnlyChecked = true,
        //        ShowReadOnly = true,
        //        Multiselect = false,
        //        ValidateNames = true
        //    };

        //    if (Temp.ShowDialog() == true && RelativeFileInfo.TryGetFileInfo(Temp.FileName, out FileInfo Result)) {
        //        Debug.WriteLine($"//―――――――「{Result.Name}」――――――――//");

        //        //Debug.WriteLine("Raw Text:");
        //        string[] Lines = File.ReadAllLines(Result.FullName);
        //        //foreach(string Line in Lines) {
        //        //    Debug.WriteLine($"\t{Line}");
        //        //}

        //        //Debug.WriteLine("\nBeatmap:");
        //        OsuBeatmap Analysed = OsuBeatmap.GetBeatmap(Lines);
        //        //Debug.WriteLine("-----------");
        //        Debug.WriteLine(Analysed.ToString());

        //        Debug.WriteLine($"//――――――――{string.Concat(Enumerable.Repeat("―", Result.Name.Length))}―――――――――//");
        //    }

        //    //Debug.WriteLine("---------------------------------");
        //    // ReSharper disable once TailRecursiveCall
        //    Test();
        //}

        /// <summary>The currently open <see cref="OverrideBackgroundsPage"/>.</summary>
        static OverrideBackgroundsPage _OBPage;

        /// <summary>Handles the <c>SelectionChanged</c> event of the <see cref="TabControl"/> control.
        /// <para/>Calls <see cref="SwapTab(Tab)"/> with the currently active <see cref="Tab"/>.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        void TabControl_SelectionChanged(object Sender, SelectionChangedEventArgs E) {
            if (Sender == null || !(Sender is TabControl TC)) { return; }

            Tab T = TC.SelectedIndex.ConvertToEnum(Tab.Unknown);
            SwapTab(T);
        }

        /// <summary>Swaps the tab to the given <paramref name="T"/>.
        /// <para/>Calls <see cref="Frame.Navigate(object)"/> on the <see cref="MainFrame"/>, navigating to the relevant page.</summary>
        /// <param name="T">The <see cref="Tab"/> to swap to.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SwapTab(Tab T) {
            switch (T) {
                case Tab.RemoveBackgrounds: //'Remove Backgrounds' tab
                    Debug.WriteLine("Changed to Remove Backgrounds tab");
                    if (_OBPage == null) { _OBPage = new OverrideBackgroundsPage(); }
                    //RemoveBackgroundsFrame.Navigate(new Uri("Windows/RemoveBackgroundsWindow.xaml", UriKind.Relative));
                    MainFrame.Navigate(_OBPage);
                    break;
                // ReSharper disable once RedundantCaseLabel
                case Tab.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>The known tabs supporting navigation.</summary>
        public enum Tab {
            /// <summary>Represents an unknown tab.</summary>
            Unknown = 0,
            /// <summary>Represents the 'Remove Backgrounds' tab.</summary>
            RemoveBackgrounds = 1
        }

        /// <summary>Handles the <c>PreviewKeyDown</c> event of this (<see cref="MahApps.Metro.Controls.MetroWindow"/>) control.
        /// <para/>If the user presses the key-sequence 'CTRL + L', the Log Viewer is opened.
        /// <para/>Alternatively, if the user presses the key-sequence 'CTRL + E', the Exception Viewer is opened.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        void MetroWindow_PreviewKeyDown(object Sender, KeyEventArgs E) {
            bool Control = Keyboard.IsKeyDown(Key.LeftCtrl);
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (E.Key) {
                case Key.L when Control:
                    (Application.Current.TryGetWindow(out LogWindow FoundLW) ? FoundLW : new LogWindow()).Show();
                    E.Handled = true;
                    return;
                case Key.E when Control:
                    (Application.Current.TryGetWindow(out ExceptionWindow FoundEW) ? FoundEW : new ExceptionWindow()).Show();
                    E.Handled = true;
                    return;
                default:
                    E.Handled = false;
                    return;
            }
        }

        /// <summary>Handles the <c>Closed</c> event of this (<see cref="MahApps.Metro.Controls.MetroWindow"/>) control.
        /// <para/>Closes all active <see cref="Window"/>s within the current <see cref="Application"/>.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="EventArgs"/> instance containing the event data.</param>
        void MetroWindow_Closed(object Sender, EventArgs E) {
            WindowCollection Windows = Application.Current.Windows;
            foreach (Window W in from Window W in Windows where W != this select W) {
                W.Close();
            }
        }

        // ReSharper disable once AsyncConverter.AsyncAwaitMayBeElidedHighlighting        
        /// <summary>Asynchronously handles the <c>Loaded</c> event of this (<see cref="MahApps.Metro.Controls.MetroWindow"/>) control.
        /// <para/>Calls <see cref="UpdateWindow"/>.<see cref="UpdateWindow.CreateAsync(Window)"/> to ensure the application checks for any available updates, and notifies the user if one is available.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        async void MetroWindow_Loaded(object Sender, RoutedEventArgs E) => await UpdateWindow.CreateAsync(this).ConfigureAwait(false);
    }
}
