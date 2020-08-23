#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;

#endregion

namespace Osu_BackgroundPurge.UserControls {
    public partial class FileOpenBrowser {

        public FileOpenBrowser() {
            InitializeComponent();
        }

        void BrowseButton_Click(object Sender, RoutedEventArgs E) {
            VistaOpenFileDialog OpenDialog = new VistaOpenFileDialog {
                AddExtension = true,
                Filter = Filter ?? "Any file (*.*)|*.*",
                FilterIndex = FilterIndex,
                InitialDirectory = (InitialDirectory ?? StringToFileSystemInfoTypeConverter.Desktop()).FullName,
                Title = PopupTitle ?? "Pick a file",
                ValidateNames = true
            };

            //Select previous path if not null
            if (SelectedPath != null) { OpenDialog.FileName = SelectedPath.FullName; } 

            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (OpenDialog.ShowDialog()) {
                case true when TryGetFileInfo(OpenDialog.FileName, out FileInfo Result):
                    SelectedPath = Result;
                    break;
            }
        }

        #region Routed Events

        public static readonly RoutedEvent FilePathChangedEvent = EventManager.RegisterRoutedEvent(
       nameof(FilePathChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FileOpenBrowser));

        public event RoutedEventHandler FilePathChanged {
            add => AddHandler(FilePathChangedEvent, value);
            remove => RemoveHandler(FilePathChangedEvent, value);
        }

        #endregion

        #region Dependency Properties

        #region Selected Path

        public FileInfo SelectedPath {
            get => (FileInfo)GetValue(SelectedPathproperty);
            set {
                SetValue(SelectedPathproperty, value);
                PathTextBox.Text = value.FullName;
            }
        }

        public static readonly DependencyProperty SelectedPathproperty =
            DependencyProperty.Register(
                nameof(SelectedPath),
                typeof(FileInfo),
                typeof(FileOpenBrowser),
                new FrameworkPropertyMetadata(SelectedPathChanged) {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        static void SelectedPathChanged(DependencyObject D, DependencyPropertyChangedEventArgs E) {
            FileOpenBrowser U = (FileOpenBrowser)D;
            FileInfo F = (FileInfo)E.NewValue;

            //U.PathTextBox.Text = F.FullName;
            U.RaiseEvent(new RoutedEventArgs(FilePathChangedEvent, F));
        }

        #endregion

        #region Filter

        public string Filter {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(nameof(Filter), typeof(string), typeof(FileOpenBrowser), new PropertyMetadata("Any File (*.*)|*.*"));

        #endregion

        #region Filter Index

        public int FilterIndex {
            get => (int)GetValue(FilterIndexProperty);
            set => SetValue(FilterIndexProperty, value);
        }

        public static readonly DependencyProperty FilterIndexProperty =
            DependencyProperty.Register(nameof(FilterIndex), typeof(int), typeof(FileOpenBrowser), new PropertyMetadata(0));

        #endregion

        #region Initial Directory

        public DirectoryInfo InitialDirectory {
            get => (DirectoryInfo)GetValue(InitialDirectoryProperty);
            set => SetValue(InitialDirectoryProperty, value);
        }

        public static readonly DependencyProperty InitialDirectoryProperty =
            DependencyProperty.Register(nameof(InitialDirectory), typeof(DirectoryInfo), typeof(FileOpenBrowser), new PropertyMetadata(StringToFileSystemInfoTypeConverter.ExecutingLocation()));

        #endregion

        #region Title

        public string PopupTitle {
            get => (string)GetValue(PopupTitleProperty);
            set => SetValue(PopupTitleProperty, value);
        }

        public static readonly DependencyProperty PopupTitleProperty =
            DependencyProperty.Register(nameof(PopupTitle), typeof(string), typeof(FileOpenBrowser), new PropertyMetadata("Pick a file"));

        #endregion

        #endregion

        void PathTextBox_LostKeyboardFocus(object Sender, KeyboardFocusChangedEventArgs E) {
            if (TryGetFileInfo(PathTextBox.Text, out FileInfo Result)) {
                SelectedPath = Result;
            }
        }

        public static bool TryGetFileInfo(string Path, out FileInfo Result) {
            Result = (FileInfo)new StringToFileSystemInfoTypeConverter().ConvertFromString(Path);
            return Result != null;
        }

    }

}
