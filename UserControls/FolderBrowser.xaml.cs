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
    /// <summary>A <see cref="System.Windows.Controls.UserControl"/> allowing the user to select a <see cref="DirectoryInfo"/>.</summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class FolderBrowser {

        /// <summary>Initialises a new instance of the <see cref="FolderBrowser"/> class.</summary>
        public FolderBrowser() {
            InitializeComponent();
        }

        /// <summary>Handles the <c>Click</c> event of the <c>BrowseButton</c> control.
        /// <para/>Creates and opens a <see cref="CommonOpenFileDialog"/>, allowing the user to browse and select a folder.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>The directory path changed event</summary>
        public static readonly RoutedEvent DirectoryPathChangedEvent = EventManager.RegisterRoutedEvent(
       nameof(DirectoryPathChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FolderBrowser));

        /// <summary>Occurs when the selected directory path changes.</summary>
        public event RoutedEventHandler DirectoryPathChanged {
            add => AddHandler(DirectoryPathChangedEvent, value);
            remove => RemoveHandler(DirectoryPathChangedEvent, value);
        }

        #endregion

        #region Dependency Properties

        /// <summary>Gets or sets the path set via user input.</summary>
        /// <value>The user input path <see cref="string"/>.</value>
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

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="UserInputPath"/>.</summary>
        public static readonly DependencyProperty UserInputPathProperty =
            DependencyProperty.Register(nameof(UserInputPath), typeof(string), typeof(FolderBrowser), new PropertyMetadata(string.Empty));

        #region Selected Path

        /// <summary>Gets or sets the selected path.</summary>
        /// <value>The selected path <see cref="DirectoryInfo"/>.</value>
        public DirectoryInfo SelectedPath {
            get => (DirectoryInfo)GetValue(SelectedPathProperty);
            set => SetValue(SelectedPathProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="SelectedPath"/>.</summary>
        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register(
                nameof(SelectedPath),
                typeof(DirectoryInfo),
                typeof(FolderBrowser),
                new FrameworkPropertyMetadata(SelectedPathChanged) {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        /// <summary>Called when <see cref="SelectedPathProperty"/> changes.</summary>
        /// <param name="D">The <see cref="DependencyObject"/>.</param>
        /// <param name="E">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        static void SelectedPathChanged(DependencyObject D, DependencyPropertyChangedEventArgs E) {
            FolderBrowser U = (FolderBrowser)D;
            DirectoryInfo I = (DirectoryInfo)E.NewValue;

            //U.PathTextBox.Text = F.FullName;
            U.RaiseEvent(new RoutedEventArgs(DirectoryPathChangedEvent, I));
        }

        #endregion

        #region Filter

        /// <summary>Gets or sets the filter.</summary>
        /// <value>The filter <see cref="string"/>.</value>
        public string Filter {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="Filter"/>.</summary>
        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(nameof(Filter), typeof(string), typeof(FolderBrowser), new PropertyMetadata("Any File (*.*)|*.*"));

        #endregion

        #region Filter Index

        /// <summary>Gets or sets the index of the initially-selected filter.</summary>
        /// <value>The index of the filter.</value>
        public int FilterIndex {
            get => (int)GetValue(FilterIndexProperty);
            set => SetValue(FilterIndexProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="FilterIndex"/>.</summary>
        public static readonly DependencyProperty FilterIndexProperty =
            DependencyProperty.Register(nameof(FilterIndex), typeof(int), typeof(FolderBrowser), new PropertyMetadata(0));

        #endregion

        #region Initial Directory

        /// <summary>Gets or sets the initial directory.</summary>
        /// <value>The initial directory <see cref="DirectoryInfo"/>.</value>
        public DirectoryInfo InitialDirectory {
            get => (DirectoryInfo)GetValue(InitialDirectoryProperty);
            set => SetValue(InitialDirectoryProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="InitialDirectory"/>.</summary>
        public static readonly DependencyProperty InitialDirectoryProperty =
            DependencyProperty.Register(nameof(InitialDirectory), typeof(DirectoryInfo), typeof(FolderBrowser), new PropertyMetadata(StringToFileSystemInfoTypeConverter.ExecutingLocation()));

        #endregion

        #region Title

        /// <summary>Gets or sets the popup title.</summary>
        /// <value>The popup title <see cref="string"/>.</value>
        public string PopupTitle {
            get => (string)GetValue(PopupTitleProperty);
            set => SetValue(PopupTitleProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="PopupTitle"/>.</summary>
        public static readonly DependencyProperty PopupTitleProperty =
            DependencyProperty.Register(nameof(PopupTitle), typeof(string), typeof(FolderBrowser), new PropertyMetadata("Pick a file"));

        #endregion

        #endregion

        /// <summary>Handles the <c>LostKeyboardFocus</c> event of the <see cref="PathTextBox"/> control.
        /// <para/>Sets the <see cref="SelectedPath"/> to the newly-entered path, if valid.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="KeyboardFocusChangedEventArgs"/> instance containing the event data.</param>
        void PathTextBox_LostKeyboardFocus(object Sender, KeyboardFocusChangedEventArgs E) {
            if (PathTextBox.Text.TryGetDirectoryInfo(true, out DirectoryInfo Result)) {
                SelectedPath = Result;
            }
        }

    }

}
