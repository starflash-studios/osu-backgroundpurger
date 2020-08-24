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
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Threading;
using Osu_BackgroundPurge.Properties;
using Osu_BackgroundPurge.UserControls;

#endregion

namespace Osu_BackgroundPurge.Windows {
    /// <summary>A <see cref="Window"/> allowing the user to specify image resize options for this application.</summary>
    /// <seealso cref="MahApps.Metro.Controls.MetroWindow" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="Pages.OverrideBackgroundsPage" />
    public partial class GlobalImageResizerWindow {
        /// <summary>Initialises a new instance of the <see cref="GlobalImageResizerWindow"/> class.</summary>
        public GlobalImageResizerWindow() {
            InitializeComponent();

            UpdateUI();
            _Ready = true;
        }

        /// <summary>Whether or not this instance is ready to be used.
        /// <para/>If <c>false</c>, <see cref="BtnSave_Click(object, RoutedEventArgs)"/> events are ignored.</summary>
        bool _Ready = false;

        /// <summary>Gets the open windows of type <typeparamref name="T"/>.</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        public static IEnumerable<T> GetOpenWindows<T>() where T : Window => Application.Current.Windows.Cast<Window>().Where(Window => Window.GetType() == typeof(T)).Cast<T>();

        /// <summary>Resets the image resize options to the default settings.</summary>
        public static void Reset() {
            CompositingMode = CompositingMode.SourceCopy;
            CompositingQuality = CompositingQuality.HighQuality;
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            WrapMode = WrapMode.TileFlipXY;

            foreach(GlobalImageResizerWindow W in GetOpenWindows<GlobalImageResizerWindow>()) {
                W.Dispatcher.Invoke(W.UpdateUI, DispatcherPriority.Normal);
            }
        }

        /// <summary>Updates the relative UI dropdowns to their related properties.</summary>
        public void UpdateUI() {
            BtnSave.IsEnabled = false;

            _Ready = false;
            DpdCmpstMode.SetValue(CompositingMode, false);
            DpdCmpstQual.SetValue(CompositingQuality, false);
            DpdInterpMode.SetValue(InterpolationMode, false);
            DpdSmoothMode.SetValue(SmoothingMode, false);
            DpdPxlOffsetMode.SetValue(PixelOffsetMode, false);
            DpdWrapMode.SetValue(WrapMode, false);
            _Ready = true;
        }

        #region Properties

        /// <summary>Gets or sets the image resizer's compositing mode.</summary>
        /// <value>The compositing mode <see cref="CompositingMode"/>.</value>
        public static CompositingMode CompositingMode {
            get => Settings.Default.CompositingMode.ConvertToEnum(CompositingMode.SourceCopy);
            set {
                Settings.Default.CompositingMode = (int)value;
                Settings.Default.Save();
            }
        }

        /// <summary>Gets or sets the image resizer's compositing quality.</summary>
        /// <value>The compositing quality <see cref="CompositingQuality"/>.</value>
        public static CompositingQuality CompositingQuality {
            get => Settings.Default.CompositingQuality.ConvertToEnum(CompositingQuality.HighQuality);
            set {
                Settings.Default.CompositingQuality = (int)value;
                Settings.Default.Save();
            }
        }

        /// <summary>Gets or sets the image resizer's interpolation mode.</summary>
        /// <value>The interpolation mode <see cref="InterpolationMode"/>.</value>
        public static InterpolationMode InterpolationMode {
            get => Settings.Default.InterpolationMode.ConvertToEnum(InterpolationMode.HighQualityBicubic);
            set {
                Settings.Default.InterpolationMode = (int)value;
                Settings.Default.Save();
            }
        }

        /// <summary>Gets or sets the image resizer's smoothing mode.</summary>
        /// <value>The smoothing mode <see cref="SmoothingMode"/>.</value>
        public static SmoothingMode SmoothingMode {
            get => Settings.Default.SmoothingMode.ConvertToEnum(SmoothingMode.HighQuality);
            set {
                Settings.Default.SmoothingMode = (int)value;
                Settings.Default.Save();
            }
        }

        /// <summary>Gets or sets the image resizer's pixel offset mode.</summary>
        /// <value>The pixel offset mode <see cref="PixelOffsetMode"/>.</value>
        public static PixelOffsetMode PixelOffsetMode {
            get => Settings.Default.PixelOffsetMode.ConvertToEnum(PixelOffsetMode.HighQuality);
            set {
                Settings.Default.PixelOffsetMode = (int)value;
                Settings.Default.Save();
            }
        }

        /// <summary>Gets or sets the image resizer's wrap mode.</summary>
        /// <value>The wrap mode <see cref="WrapMode"/>.</value>
        public static WrapMode WrapMode {
            get => Settings.Default.WrapMode.ConvertToEnum(WrapMode.TileFlipXY);
            set {
                Settings.Default.WrapMode = (int)value;
                Settings.Default.Save();
            }
        }

        #endregion

        void BtnReset_Click(object Sender, RoutedEventArgs E) => Reset();

        void BtnSave_Click(object Sender, RoutedEventArgs E) {
            BtnSave.IsEnabled = false;
            CompositingMode = (CompositingMode)DpdCmpstMode.GetValue();
            CompositingQuality = (CompositingQuality)DpdCmpstQual.GetValue();
            InterpolationMode = (InterpolationMode)DpdInterpMode.GetValue();
            SmoothingMode = (SmoothingMode)DpdSmoothMode.GetValue();
            PixelOffsetMode = (PixelOffsetMode)DpdPxlOffsetMode.GetValue();
            WrapMode = (WrapMode)DpdWrapMode.GetValue();
        }

        /// <summary>Gets the selected enum value (of type <typeparamref name="T"/>) from the specified <paramref name="Dropdown"/>.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Dropdown">The dropdown.</param>
        /// <returns><typeparamref name="T"/></returns>
        public static T GetValue<T>(EnumDropdown Dropdown) where T : struct, Enum => (T?)Dropdown?.GetValue() ?? default;

        /// <summary>Handles the <c>SelectionChanged</c> event of the dropdown controls.
        /// <para/>Enables/Disables the <see cref="BtnSave"/> control relative to whether or not the window <see cref="HasUnsavedChanges"/>.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="EnumSelectionChangedArgs"/> instance containing the event data.</param>
        void Dpd_SelectionChanged(object Sender, EnumSelectionChangedArgs E) => BtnSave.IsEnabled = _Ready && HasUnsavedChanges();

        /// <summary>Determines whether or not the window has unsaved changes.</summary>
        /// <returns><c>True</c> if there are unsaved changes; otherwise, <c>false</c>.</returns>
        public bool HasUnsavedChanges() => CompositingMode != GetValue<CompositingMode>(DpdCmpstMode)
                                           || CompositingQuality != GetValue<CompositingQuality>(DpdCmpstQual)
                                           || InterpolationMode != GetValue<InterpolationMode>(DpdInterpMode)
                                           || SmoothingMode != GetValue<SmoothingMode>(DpdSmoothMode)
                                           || PixelOffsetMode != GetValue<PixelOffsetMode>(DpdPxlOffsetMode)
                                           || WrapMode != GetValue<WrapMode>(DpdWrapMode);

        /// <summary>Handles the <c>Closing</c> event of this instance.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        void MetroWindow_Closing(object Sender, CancelEventArgs E) {
            if (HasUnsavedChanges()) {
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (MessageBox.Show(this, "Are you sure you want to exit?\n\nYou have unsaved changes. Press 'No' now and click on the 'SAVE CHANGES' button to save your new values.", $"Unsaved Changes・{Title}", MessageBoxButton.YesNo, MessageBoxImage.Warning)) {
                    case MessageBoxResult.No:
                    case MessageBoxResult.Cancel:
                        E.Cancel = true;
                        break;
                }
            }
        }
    }
}
