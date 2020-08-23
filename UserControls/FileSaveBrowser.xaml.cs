#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;

#endregion

namespace Osu_BackgroundPurge.UserControls {
    public partial class FileSaveBrowser {
        public delegate void DlgOnChange(string NewPath);
        public DlgOnChange OnChange;

        public FileSaveBrowser() {
            InitializeComponent();

        }
        public static DirectoryInfo ExecutingLocation() => ExecutingApplication().Directory;

        public static FileInfo ExecutingApplication() => new FileInfo(Assembly.GetExecutingAssembly().Location);

        public string Filter = "Any File (*.*)|*.*";

        void BrowseButton_Click(object Sender, RoutedEventArgs E) {
            VistaSaveFileDialog SaveDialog = new VistaSaveFileDialog {
                AddExtension = true,
                Filter = Filter,
                FilterIndex = 0,
                FileName = SelectedSavePath,
                InitialDirectory = ExecutingLocation().FullName,
                OverwritePrompt = true,
                Title = "Pick a save location",
                ValidateNames = true
            };

            switch (SaveDialog.ShowDialog()) {
                case true:
                    PathTextBox.Text = SaveDialog.FileName;
                    OnChange?.Invoke(PathTextBox.Text);
                    break;
            }
        }

        #region Dependency Properties

        public string SelectedSavePath {
            get => (string)GetValue(SelectedSavePathProperty);
            set {
                SetValue(SelectedSavePathProperty, value);
                OnChange?.Invoke(value);
            }
        }

        public static readonly DependencyProperty SelectedSavePathProperty =
            DependencyProperty.Register(
                "SelectedSavePath",
                typeof(string),
                typeof(FileSaveBrowser),
                new FrameworkPropertyMetadata(SelectedSavePathChanged) {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        static void SelectedSavePathChanged(DependencyObject D, DependencyPropertyChangedEventArgs E) {
            ((FileSaveBrowser)D).PathTextBox.Text = E.NewValue.ToString();
        }

        #endregion

        void PathTextBox_LostKeyboardFocus(object Sender, KeyboardFocusChangedEventArgs E) {
            SelectedSavePath = PathTextBox.Text;
            OnChange?.Invoke(PathTextBox.Text);
        }
    }
}
