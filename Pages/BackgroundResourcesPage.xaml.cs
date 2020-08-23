#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls.Primitives;

#endregion

namespace Osu_BackgroundPurge.Pages {
    public partial class BackgroundResourcesPage {
        public BackgroundResourcesPage() {
            InitializeComponent();

            Tg_Click(null, null); //Update selected backgrounds counter
        }

        public bool BgLazerPink {
            get => (bool)GetValue(BgLazerPinkProperty);
            set => SetValue(BgLazerPinkProperty, value);
        }

        public static readonly DependencyProperty BgLazerPinkProperty =
            DependencyProperty.Register(nameof(BgLazerPink), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));

        public bool BgLazerPurple {
            get => (bool)GetValue(BgLazerPurpleProperty);
            set => SetValue(BgLazerPurpleProperty, value);
        }

        public static readonly DependencyProperty BgLazerPurpleProperty =
            DependencyProperty.Register(nameof(BgLazerPurple), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));

        public bool BgOsuAcademy {
            get => (bool)GetValue(BgOsuAcademyProperty);
            set => SetValue(BgOsuAcademyProperty, value);
        }

        public static readonly DependencyProperty BgOsuAcademyProperty =
            DependencyProperty.Register(nameof(BgOsuAcademy), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));

        public bool BgOsuCinematic {
            get => (bool)GetValue(BgOsuCinematicProperty);
            set => SetValue(BgOsuCinematicProperty, value);
        }

        public static readonly DependencyProperty BgOsuCinematicProperty =
            DependencyProperty.Register(nameof(BgOsuCinematic), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));

        public bool BgOsuDark {
            get => (bool)GetValue(BgOsuDarkProperty);
            set => SetValue(BgOsuDarkProperty, value);
        }

        public static readonly DependencyProperty BgOsuDarkProperty =
            DependencyProperty.Register(nameof(BgOsuDark), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));

        public bool BgOsuMinimalist {
            get => (bool)GetValue(BgOsuMinimalistProperty);
            set => SetValue(BgOsuMinimalistProperty, value);
        }

        public static readonly DependencyProperty BgOsuMinimalistProperty =
            DependencyProperty.Register(nameof(BgOsuMinimalist), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));

        public bool BgOsuParental {
            get => (bool)GetValue(BgOsuParentalProperty);
            set => SetValue(BgOsuParentalProperty, value);
        }

        public static readonly DependencyProperty BgOsuParentalProperty =
            DependencyProperty.Register(nameof(BgOsuParental), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));

        public bool BgOsuYukata {
            get => (bool)GetValue(BgOsuYukataProperty);
            set => SetValue(BgOsuYukataProperty, value);
        }

        public static readonly DependencyProperty BgOsuYukataProperty =
            DependencyProperty.Register(nameof(BgOsuYukata), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));

        public IEnumerable<Bitmap> GetActiveBackgrounds() {
            if (BgLazerPink) { yield return Properties.Resources.BG_LazerPink; }
            if (BgLazerPurple) { yield return Properties.Resources.BG_LazerPurple; }
            if (BgOsuAcademy) { yield return Properties.Resources.BG_OsuAcademy; }
            if (BgOsuCinematic) { yield return Properties.Resources.BG_OsuCinematic; }
            if (BgOsuDark) { yield return Properties.Resources.BG_OsuDark; }
            if (BgOsuMinimalist) { yield return Properties.Resources.BG_OsuMinimalist; }
            if (BgOsuParental) { yield return Properties.Resources.BG_OsuParental; }
            if (BgOsuYukata) { yield return Properties.Resources.BG_OsuYukata; }
        }

        public Bitmap GetRandomBackground() => GetActiveBackgrounds().GetRandom();

        void Tg_Click(object Sender, RoutedEventArgs E) {
            Bitmap[] ActiveBitmaps = GetActiveBackgrounds().ToArray();

            //#if (DEBUG)
            //foreach(Bitmap ActiveBitmap in ActiveBitmaps) {
            //    Debug.WriteLine($"{ActiveBitmap} is active. ");
            //}
            //#endif

            int C = ActiveBitmaps.Length;

            if (C <= 0) { //At least one background MUST be active at all times
                ((ToggleButton)Sender).IsChecked = true;
                C = 1;
            }

            LblCount.Content = $"[{C}] selected";
            LblCount.Focus();
        }
    }
}
