#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;

#endregion

namespace Osu_BackgroundPurge.UserControls {
    public partial class FolderMultiBrowser {

        public FolderMultiBrowser() {
            InitializeComponent();
        }

        void BrowseButton_Click(object Sender, RoutedEventArgs E) {
            //string Init = StringToFileSystemInfoTypeConverter.ExecutingLocation().FullName;
            string Init = StringToFileSystemInfoTypeConverter.Desktop().FullName;

            using CommonOpenFileDialog OpenDialog = new CommonOpenFileDialog {
                IsFolderPicker = true,
                AllowNonFileSystemItems = false,
                DefaultDirectory = Init,
                InitialDirectory = Init,
                AllowPropertyEditing = true,
                //EnsurePathExists = true, //Despite appearing to be a useful function, when combined with 'Multiselect', this function throws false-positives in 90% of all cases with more than one item selected.
                //EnsureValidNames = true,
                EnsureReadOnly = false,
                Multiselect = true,
                Title = "Pick a folder"
            };

            //Select previous path if not null
            if (SelectedPaths != null && SelectedPaths.Count > 0) {
                OpenDialog.InitialDirectory = SelectedPaths[0].FullName;
                OpenDialog.DefaultDirectory = SelectedPaths[0].FullName;
            } 

            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (OpenDialog.ShowDialog()) {
                case CommonFileDialogResult.Ok:
                    List<DirectoryInfo> Dirs = new List<DirectoryInfo>();
                    foreach (string FileName in OpenDialog.FileNames) {
                        if (FileName.TryGetDirectoryInfo(true, out DirectoryInfo Dir)) {
                            Dirs.Add(Dir);
                        }
                    }

                    SelectedPaths = new ReadOnlyCollection<DirectoryInfo>(Dirs);
                    return;
            }
        }

        #region Routed Events

        public static readonly RoutedEvent DirectoryPathChangedEvent = EventManager.RegisterRoutedEvent(
       nameof(DirectoryPathChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FolderMultiBrowser));

        public event RoutedEventHandler DirectoryPathChanged {
            add => AddHandler(DirectoryPathChangedEvent, value);
            remove => RemoveHandler(DirectoryPathChangedEvent, value);
        }

        #endregion

        #region Dependency Properties

        #region Selected Path

        public ReadOnlyCollection<DirectoryInfo> SelectedPaths {
            get => (ReadOnlyCollection<DirectoryInfo>)GetValue(SelectedPathsProperty);
            set => SetValue(SelectedPathsProperty, value);
        }

        public static readonly DependencyProperty SelectedPathsProperty =
            DependencyProperty.Register(
                nameof(SelectedPaths),
                typeof(ReadOnlyCollection<DirectoryInfo>),
                typeof(FolderMultiBrowser),
                new FrameworkPropertyMetadata(SelectedPathsChanged) {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        static void SelectedPathsChanged(DependencyObject D, DependencyPropertyChangedEventArgs E) {
            FolderMultiBrowser U = (FolderMultiBrowser)D;
            ReadOnlyCollection<DirectoryInfo> I = (ReadOnlyCollection<DirectoryInfo>)E.NewValue;

            U.PathTextBox.Text = GetSummary(I);

            //U.PathTextBox.Text = F.FullName;
            U.RaiseEvent(new RoutedEventArgs(DirectoryPathChangedEvent, I));
        }

        public static string GetSummary<T>(ReadOnlyCollection<T> Paths) => Paths != null ? $"\"{string.Join("\", \"", Paths)}\"" : string.Empty;

        #endregion

        #region Filter

        public string Filter {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(nameof(Filter), typeof(string), typeof(FolderMultiBrowser), new PropertyMetadata("Any File (*.*)|*.*"));

        #endregion

        #region Filter Index

        public int FilterIndex {
            get => (int)GetValue(FilterIndexProperty);
            set => SetValue(FilterIndexProperty, value);
        }

        public static readonly DependencyProperty FilterIndexProperty =
            DependencyProperty.Register(nameof(FilterIndex), typeof(int), typeof(FolderMultiBrowser), new PropertyMetadata(0));

        #endregion

        #region Initial Directory

        public DirectoryInfo InitialDirectory {
            get => (DirectoryInfo)GetValue(InitialDirectoryProperty);
            set => SetValue(InitialDirectoryProperty, value);
        }

        public static readonly DependencyProperty InitialDirectoryProperty =
            DependencyProperty.Register(nameof(InitialDirectory), typeof(DirectoryInfo), typeof(FolderMultiBrowser), new PropertyMetadata(StringToFileSystemInfoTypeConverter.ExecutingLocation()));

        #endregion

        #region Title

        public string PopupTitle {
            get => (string)GetValue(PopupTitleProperty);
            set => SetValue(PopupTitleProperty, value);
        }

        public static readonly DependencyProperty PopupTitleProperty =
            DependencyProperty.Register(nameof(PopupTitle), typeof(string), typeof(FolderMultiBrowser), new PropertyMetadata("Pick a file"));

        #endregion

        #endregion

        void PathTextBox_LostKeyboardFocus(object Sender, KeyboardFocusChangedEventArgs E) {
            if (PathTextBox.Text.TryGetDirectoryInfo(true, out DirectoryInfo Result)) {
                SelectedPaths = new ReadOnlyCollection<DirectoryInfo>(new [] {Result});
            }
        }

    }

}
