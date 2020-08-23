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

using Osu_BackgroundPurge.Pages;

#endregion

namespace Osu_BackgroundPurge.Windows {
    public partial class MainWindow {
        //TODO: Add beatmap folder function to OsuFileSystem.cs
        //TODO: Add skins   folder function to OsuFileSystem.cs
        //TODO: Implement systems into Osu!ModeManager as well.

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

        static void Current_DispatcherUnhandledException(object Sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs E) => ExceptionWindow.Catch(E.Exception);

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

        static OverrideBackgroundsPage _RBPage;
        void TabControl_SelectionChanged(object Sender, SelectionChangedEventArgs E) {
            if (Sender == null || !(Sender is TabControl TC)) { return; }

            Tab T = TC.SelectedIndex.ConvertToEnum(Tab.Unknown);
            SwapTab(T);
        }

        public void SwapTab(Tab T) {
            switch (T) {
                case Tab.RemoveBackgrounds: //'Remove Backgrounds' tab
                    Debug.WriteLine("Changed to Remove Backgrounds tab");
                    if (_RBPage == null) { _RBPage = new OverrideBackgroundsPage(); }
                    //RemoveBackgroundsFrame.Navigate(new Uri("Windows/RemoveBackgroundsWindow.xaml", UriKind.Relative));
                    MainFrame.Navigate(_RBPage);
                    break;
                // ReSharper disable once RedundantCaseLabel
                case Tab.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public enum Tab {
            Unknown = 0,
            RemoveBackgrounds = 1
        }

        //If user presses the key-sequence 'CTRL + L', open the Log Viewer.
        //If user presses the key-sequence 'CTRL + E', open the Exception Viewer.
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

        void MetroWindow_Closed(object Sender, EventArgs E) {
            WindowCollection Windows = Application.Current.Windows;
            foreach (Window W in from Window W in Windows where W != this select W) {
                W.Close();
            }
        }

        // ReSharper disable once AsyncConverter.AsyncAwaitMayBeElidedHighlighting
        async void MetroWindow_Loaded(object Sender, RoutedEventArgs E) => await UpdateWindow.CreateAsync(this).ConfigureAwait(false);
    }
}
