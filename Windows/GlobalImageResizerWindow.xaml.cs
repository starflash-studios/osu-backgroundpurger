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
    public partial class GlobalImageResizerWindow {
        public GlobalImageResizerWindow() {
            InitializeComponent();

            UpdateUI();
            _Ready = true;
        }

        bool _Ready = false;

        public static IEnumerable<T> GetOpenWindows<T>() where T : Window => Application.Current.Windows.Cast<Window>().Where(Window => Window.GetType() == typeof(T)).Cast<T>();

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

        public static CompositingMode CompositingMode {
            get => GetEnum<CompositingMode>(Settings.Default.CompositingMode);
            set {
                Settings.Default.CompositingMode = (int)value;
                Settings.Default.Save();
            }
        }

        public static CompositingQuality CompositingQuality {
            get => GetEnum<CompositingQuality>(Settings.Default.CompositingQuality);
            set {
                Settings.Default.CompositingQuality = (int)value;
                Settings.Default.Save();
            }
        }

        public static InterpolationMode InterpolationMode {
            get => GetEnum<InterpolationMode>(Settings.Default.InterpolationMode);
            set {
                Settings.Default.InterpolationMode = (int)value;
                Settings.Default.Save();
            }
        }

        public static SmoothingMode SmoothingMode {
            get => GetEnum<SmoothingMode>(Settings.Default.SmoothingMode);
            set {
                Settings.Default.SmoothingMode = (int)value;
                Settings.Default.Save();
            }
        }

        public static PixelOffsetMode PixelOffsetMode {
            get => GetEnum<PixelOffsetMode>(Settings.Default.PixelOffsetMode);
            set {
                Settings.Default.PixelOffsetMode = (int)value;
                Settings.Default.Save();
            }
        }

        public static WrapMode WrapMode {
            get => GetEnum<WrapMode>(Settings.Default.WrapMode);
            set {
                Settings.Default.WrapMode = (int)value;
                Settings.Default.Save();
            }
        }

        public static T GetEnum<T>(int PropertyValue) where T : struct, Enum => (T)Enum.ToObject(typeof(T), PropertyValue);

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

        public static T GetValue<T>(EnumDropdown Dropdown) where T : struct, Enum => (T?)Dropdown?.GetValue() ?? default;

        void Dpd_SelectionChanged(object Sender, EnumSelectionChangedArgs E) => BtnSave.IsEnabled = _Ready && HasUnsavedChanges();

        public bool HasUnsavedChanges() => CompositingMode != GetValue<CompositingMode>(DpdCmpstMode)
                                           || CompositingQuality != GetValue<CompositingQuality>(DpdCmpstQual)
                                           || InterpolationMode != GetValue<InterpolationMode>(DpdInterpMode)
                                           || SmoothingMode != GetValue<SmoothingMode>(DpdSmoothMode)
                                           || PixelOffsetMode != GetValue<PixelOffsetMode>(DpdPxlOffsetMode)
                                           || WrapMode != GetValue<WrapMode>(DpdWrapMode);

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
