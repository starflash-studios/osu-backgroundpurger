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
using Microsoft.WindowsAPICodePack.Dialogs;

#endregion

namespace Osu_BackgroundPurge.UserControls {
    public partial class FolderBrowser {

        public FolderBrowser() {
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
                Multiselect = false,
                Title = "Pick a folder"
            };

            //Select previous path if not null
            if (SelectedPath != null) {
                OpenDialog.InitialDirectory = SelectedPath.FullName;
                OpenDialog.DefaultDirectory = SelectedPath.FullName;
            } 

            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (OpenDialog.ShowDialog()) {
                case CommonFileDialogResult.Ok when OpenDialog.FileName.TryGetDirectoryInfo(true, out DirectoryInfo Dir):
                    SelectedPath = Dir;
                    return;
            }
        }

        #region Routed Events

        public static readonly RoutedEvent DirectoryPathChangedEvent = EventManager.RegisterRoutedEvent(
       nameof(DirectoryPathChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FolderBrowser));

        public event RoutedEventHandler DirectoryPathChanged {
            add => AddHandler(DirectoryPathChangedEvent, value);
            remove => RemoveHandler(DirectoryPathChangedEvent, value);
        }

        #endregion

        #region Dependency Properties

        public string UserInputPath {
            get => SelectedPath?.FullName ?? string.Empty;
            set {
                if (value.TryGetDirectoryInfo(true, out DirectoryInfo Result) && Result.Exists) {
                    SelectedPath = Result;
                    SetValue(UserInputPathProperty, Result.FullName);
                } else {
                    string OldPath = SelectedPath?.FullName ?? string.Empty;
                    PathTextBox.Text = OldPath;
                    SetValue(UserInputPathProperty, OldPath);
                }
            }
        }

        public static readonly DependencyProperty UserInputPathProperty =
            DependencyProperty.Register(nameof(UserInputPath), typeof(string), typeof(FolderBrowser), new PropertyMetadata(string.Empty));

        #region Selected Path

        public DirectoryInfo SelectedPath {
            get => (DirectoryInfo)GetValue(SelectedPathproperty);
            set => SetValue(SelectedPathproperty, value);
        }

        public static readonly DependencyProperty SelectedPathproperty =
            DependencyProperty.Register(
                nameof(SelectedPath),
                typeof(DirectoryInfo),
                typeof(FolderBrowser),
                new FrameworkPropertyMetadata(SelectedPathChanged) {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        static void SelectedPathChanged(DependencyObject D, DependencyPropertyChangedEventArgs E) {
            FolderBrowser U = (FolderBrowser)D;
            DirectoryInfo I = (DirectoryInfo)E.NewValue;

            //U.PathTextBox.Text = F.FullName;
            U.RaiseEvent(new RoutedEventArgs(DirectoryPathChangedEvent, I));
        }

        #endregion

        #region Filter

        public string Filter {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(nameof(Filter), typeof(string), typeof(FolderBrowser), new PropertyMetadata("Any File (*.*)|*.*"));

        #endregion

        #region Filter Index

        public int FilterIndex {
            get => (int)GetValue(FilterIndexProperty);
            set => SetValue(FilterIndexProperty, value);
        }

        public static readonly DependencyProperty FilterIndexProperty =
            DependencyProperty.Register(nameof(FilterIndex), typeof(int), typeof(FolderBrowser), new PropertyMetadata(0));

        #endregion

        #region Initial Directory

        public DirectoryInfo InitialDirectory {
            get => (DirectoryInfo)GetValue(InitialDirectoryProperty);
            set => SetValue(InitialDirectoryProperty, value);
        }

        public static readonly DependencyProperty InitialDirectoryProperty =
            DependencyProperty.Register(nameof(InitialDirectory), typeof(DirectoryInfo), typeof(FolderBrowser), new PropertyMetadata(StringToFileSystemInfoTypeConverter.ExecutingLocation()));

        #endregion

        #region Title

        public string PopupTitle {
            get => (string)GetValue(PopupTitleProperty);
            set => SetValue(PopupTitleProperty, value);
        }

        public static readonly DependencyProperty PopupTitleProperty =
            DependencyProperty.Register(nameof(PopupTitle), typeof(string), typeof(FolderBrowser), new PropertyMetadata("Pick a file"));

        #endregion

        #endregion

        void PathTextBox_LostKeyboardFocus(object Sender, KeyboardFocusChangedEventArgs E) {
            if (PathTextBox.Text.TryGetDirectoryInfo(true, out DirectoryInfo Result)) {
                SelectedPath = Result;
            }
        }

    }

}
