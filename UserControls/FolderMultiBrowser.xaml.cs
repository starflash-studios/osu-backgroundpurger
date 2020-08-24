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
using System.IO;
using System.Windows;
using System.Windows.Data;

using Microsoft.WindowsAPICodePack.Dialogs;

#endregion

namespace Osu_BackgroundPurge.UserControls {
    /// <summary>A <see cref="System.Windows.Controls.UserControl"/> allowing the user to select multiple <see cref="DirectoryInfo"/>s.</summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class FolderMultiBrowser {

        /// <summary>Initialises a new instance of the <see cref="FolderMultiBrowser"/> class.</summary>
        public FolderMultiBrowser() {
            InitializeComponent();
        }

        /// <summary>Handles the <c>Click</c> event of the <c>BrowseButton</c> control.
        /// <para/>Creates and opens a <see cref="CommonOpenFileDialog"/>, allowing the user to browse and select multiple folders.</summary>
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

        /// <summary>The directory path changed event</summary>
        public static readonly RoutedEvent DirectoryPathChangedEvent = EventManager.RegisterRoutedEvent(
       nameof(DirectoryPathChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FolderMultiBrowser));

        /// <summary>Occurs when the selected directory paths change.</summary>
        public event RoutedEventHandler DirectoryPathChanged {
            add => AddHandler(DirectoryPathChangedEvent, value);
            remove => RemoveHandler(DirectoryPathChangedEvent, value);
        }

        #endregion

        #region Dependency Properties

        #region Selected Path

        /// <summary>Gets or sets the selected paths.</summary>
        /// <value>The selected paths of type <see cref="DirectoryInfo"/>.</value>
        public ReadOnlyCollection<DirectoryInfo> SelectedPaths {
            get => (ReadOnlyCollection<DirectoryInfo>)GetValue(SelectedPathsProperty);
            set => SetValue(SelectedPathsProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="SelectedPaths"/>.</summary>
        public static readonly DependencyProperty SelectedPathsProperty =
            DependencyProperty.Register(
                nameof(SelectedPaths),
                typeof(ReadOnlyCollection<DirectoryInfo>),
                typeof(FolderMultiBrowser),
                new FrameworkPropertyMetadata(SelectedPathsChanged) {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        /// <summary>Called when <see cref="SelectedPathsProperty"/> changes.</summary>
        /// <param name="D">The <see cref="DependencyObject"/>.</param>
        /// <param name="E">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        static void SelectedPathsChanged(DependencyObject D, DependencyPropertyChangedEventArgs E) {
            FolderMultiBrowser U = (FolderMultiBrowser)D;
            ReadOnlyCollection<DirectoryInfo> I = (ReadOnlyCollection<DirectoryInfo>)E.NewValue;

            U.PathTextBox.Text = GetSummary(I);

            //U.PathTextBox.Text = F.FullName;
            U.RaiseEvent(new RoutedEventArgs(DirectoryPathChangedEvent, I));
        }

        /// <summary>Gets a string-based summary of the specified <see cref="ReadOnlyCollection{T}"/>.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Elements">The elements.</param>
        /// <returns><see cref="string"/></returns>
        public static string GetSummary<T>(ReadOnlyCollection<T> Elements) => Elements != null ? $"\"{string.Join("\", \"", Elements)}\"" : string.Empty;

        #endregion

        #region Filter

        /// <summary>Gets or sets the filter.</summary>
        /// <value>The filter <see cref="string"/>.</value>
        public string Filter {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="Filter"/>.</summary>
        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(nameof(Filter), typeof(string), typeof(FolderMultiBrowser), new PropertyMetadata("Any File (*.*)|*.*"));

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
            DependencyProperty.Register(nameof(FilterIndex), typeof(int), typeof(FolderMultiBrowser), new PropertyMetadata(0));

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
            DependencyProperty.Register(nameof(InitialDirectory), typeof(DirectoryInfo), typeof(FolderMultiBrowser), new PropertyMetadata(StringToFileSystemInfoTypeConverter.ExecutingLocation()));

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
            DependencyProperty.Register(nameof(PopupTitle), typeof(string), typeof(FolderMultiBrowser), new PropertyMetadata("Pick a file"));

        #endregion

        #endregion

    }

}
