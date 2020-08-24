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

    /// <summary>A <see cref="System.Windows.Controls.UserControl"/> allowing the user to select a <see cref="FileInfo"/> to open.</summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class FileOpenBrowser {

        /// <summary>Initialises a new instance of the <see cref="FileOpenBrowser"/> class.</summary>
        public FileOpenBrowser() {
            InitializeComponent();
        }

        /// <summary>Handles the <c>Click</c> event of the <c>BrowseButton</c> control.
        /// <para/>Creates and opens a <see cref="VistaOpenFileDialog"/>, allowing the user to browse and select a file to open.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
                case true when OpenDialog.FileName.TryGetFileInfo(true, out FileInfo Result):
                    SelectedPath = Result;
                    break;
            }
        }

        #region Routed Events

        /// <summary>The file path changed event</summary>
        public static readonly RoutedEvent FilePathChangedEvent = EventManager.RegisterRoutedEvent(nameof(FilePathChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FileOpenBrowser));

        /// <summary>Occurs when the selected file path changes.</summary>
        public event RoutedEventHandler FilePathChanged {
            add => AddHandler(FilePathChangedEvent, value);
            remove => RemoveHandler(FilePathChangedEvent, value);
        }

        #endregion

        #region Dependency Properties

        #region Selected Path

        /// <summary>Gets or sets the selected paths.</summary>
        /// <value>The selected paths of type <see cref="FileInfo"/>.</value>
        public FileInfo SelectedPath {
            get => (FileInfo)GetValue(SelectedPathProperty);
            set {
                SetValue(SelectedPathProperty, value);
                PathTextBox.Text = value.FullName;
            }
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="SelectedPath"/>.</summary>
        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register(
                nameof(SelectedPath),
                typeof(FileInfo),
                typeof(FileOpenBrowser),
                new FrameworkPropertyMetadata(SelectedPathChanged) {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        /// <summary>Called when <see cref="SelectedPathProperty"/> changes.</summary>
        /// <param name="D">The <see cref="DependencyObject"/>.</param>
        /// <param name="E">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        static void SelectedPathChanged(DependencyObject D, DependencyPropertyChangedEventArgs E) {
            FileOpenBrowser U = (FileOpenBrowser)D;
            FileInfo F = (FileInfo)E.NewValue;

            //U.PathTextBox.Text = F.FullName;
            U.RaiseEvent(new RoutedEventArgs(FilePathChangedEvent, F));
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
        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(nameof(Filter), typeof(string), typeof(FileOpenBrowser), new PropertyMetadata("Any File (*.*)|*.*"));

        #endregion

        #region Filter Index
        
        /// <summary>Gets or sets the index of the filter.</summary>
        /// <value>The index of the filter.</value>
        public int FilterIndex {
            get => (int)GetValue(FilterIndexProperty);
            set => SetValue(FilterIndexProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="FilterIndex"/>.</summary>
        public static readonly DependencyProperty FilterIndexProperty =
            DependencyProperty.Register(nameof(FilterIndex), typeof(int), typeof(FileOpenBrowser), new PropertyMetadata(0));

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
            DependencyProperty.Register(nameof(InitialDirectory), typeof(DirectoryInfo), typeof(FileOpenBrowser), new PropertyMetadata(StringToFileSystemInfoTypeConverter.ExecutingLocation()));

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
            DependencyProperty.Register(nameof(PopupTitle), typeof(string), typeof(FileOpenBrowser), new PropertyMetadata("Pick a file"));

        #endregion

        #endregion

        /// <summary>Handles the <c>LostKeyboardFocus</c> event of the <see cref="PathTextBox"/> control.
        /// <para/>Sets the <see cref="SelectedPath"/> to the newly-entered path, if valid.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="KeyboardFocusChangedEventArgs"/> instance containing the event data.</param>
        void PathTextBox_LostKeyboardFocus(object Sender, KeyboardFocusChangedEventArgs E) {
            if (PathTextBox.Text.TryGetFileInfo(true, out FileInfo Result)) {
                SelectedPath = Result;
            }
        }

    }

}
