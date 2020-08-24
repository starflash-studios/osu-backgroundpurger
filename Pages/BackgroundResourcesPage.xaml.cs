#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;

using Res = Osu_BackgroundPurge.Properties.Resources;

#endregion

namespace Osu_BackgroundPurge.Pages {
    /// <summary>A <see cref="System.Windows.Controls.Page"/> responsible for allowing the user to specify which embedded backgrounds they would like their beatmaps to use.</summary>
    /// <seealso cref="System.Windows.Controls.Page" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class BackgroundResourcesPage {
        /// <summary>Initialises a new instance of the <see cref="BackgroundResourcesPage"/> class.
        /// <para/>Page is responsible for showing embedded (<see cref="Bitmap"/>) resources available for beatmap backgrounds.</summary>
        public BackgroundResourcesPage() {
            InitializeComponent();

            Tg_Click(null, null); //Update selected backgrounds counter
        }


        /// <summary>Gets or sets a value indicating whether the <see cref="Res.BG_LazerPink"/> background is selected.</summary>
        /// <value><c>True</c> if selected; otherwise, <c>false</c>.</value>
        public bool BgLazerPink {
            get => (bool)GetValue(BgLazerPinkProperty);
            set => SetValue(BgLazerPinkProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="BgLazerPink"/>.</summary>
        public static readonly DependencyProperty BgLazerPinkProperty =
            DependencyProperty.Register(nameof(BgLazerPink), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));


        /// <summary>Gets or sets a value indicating whether the <see cref="Res.BG_LazerPurple"/> background is selected.</summary>
        /// <value><c>True</c> if selected; otherwise, <c>false</c>.</value>
        public bool BgLazerPurple {
            get => (bool)GetValue(BgLazerPurpleProperty);
            set => SetValue(BgLazerPurpleProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="BgLazerPurple"/>.</summary>
        public static readonly DependencyProperty BgLazerPurpleProperty =
            DependencyProperty.Register(nameof(BgLazerPurple), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));


        /// <summary>Gets or sets a value indicating whether the <see cref="Res.BG_OsuAcademy"/> background is selected.</summary>
        /// <value><c>True</c> if selected; otherwise, <c>false</c>.</value>
        public bool BgOsuAcademy {
            get => (bool)GetValue(BgOsuAcademyProperty);
            set => SetValue(BgOsuAcademyProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="BgOsuAcademy"/>.</summary>
        public static readonly DependencyProperty BgOsuAcademyProperty =
            DependencyProperty.Register(nameof(BgOsuAcademy), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));

        /// <summary>Gets or sets a value indicating whether the <see cref="Res.BG_OsuCinematic"/> background is selected.</summary>
        /// <value><c>True</c> if selected; otherwise, <c>false</c>.</value>
        public bool BgOsuCinematic {
            get => (bool)GetValue(BgOsuCinematicProperty);
            set => SetValue(BgOsuCinematicProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="BgOsuCinematic"/>.</summary>
        public static readonly DependencyProperty BgOsuCinematicProperty =
            DependencyProperty.Register(nameof(BgOsuCinematic), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));

        /// <summary>Gets or sets a value indicating whether the <see cref="Res.BG_OsuDark"/> background is selected.</summary>
        /// <value><c>True</c> if selected; otherwise, <c>false</c>.</value>
        public bool BgOsuDark {
            get => (bool)GetValue(BgOsuDarkProperty);
            set => SetValue(BgOsuDarkProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="BgOsuDark"/>.</summary>
        public static readonly DependencyProperty BgOsuDarkProperty =
            DependencyProperty.Register(nameof(BgOsuDark), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));

        /// <summary>Gets or sets a value indicating whether the <see cref="Res.BG_OsuMinimalist"/> background is selected.</summary>
        /// <value><c>True</c> if selected; otherwise, <c>false</c>.</value>
        public bool BgOsuMinimalist {
            get => (bool)GetValue(BgOsuMinimalistProperty);
            set => SetValue(BgOsuMinimalistProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="BgOsuMinimalist"/>.</summary>
        public static readonly DependencyProperty BgOsuMinimalistProperty =
            DependencyProperty.Register(nameof(BgOsuMinimalist), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));

        /// <summary>Gets or sets a value indicating whether the <see cref="Res.BG_OsuParental"/> background is selected.</summary>
        /// <value><c>True</c> if selected; otherwise, <c>false</c>.</value>
        public bool BgOsuParental {
            get => (bool)GetValue(BgOsuParentalProperty);
            set => SetValue(BgOsuParentalProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="BgOsuParental"/>.</summary>
        public static readonly DependencyProperty BgOsuParentalProperty =
            DependencyProperty.Register(nameof(BgOsuParental), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));

        /// <summary>Gets or sets a value indicating whether the <see cref="Res.BG_OsuYukata"/> background is selected.</summary>
        /// <value><c>True</c> if selected; otherwise, <c>false</c>.</value>
        public bool BgOsuYukata {
            get => (bool)GetValue(BgOsuYukataProperty);
            set => SetValue(BgOsuYukataProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="BgOsuYukata"/>.</summary>
        public static readonly DependencyProperty BgOsuYukataProperty =
            DependencyProperty.Register(nameof(BgOsuYukata), typeof(bool), typeof(BackgroundResourcesPage), new PropertyMetadata(true));

        /// <summary>Gets the active (selected) backgrounds.</summary>
        /// <returns><see cref="IEnumerable{Bitmap}"/></returns>
        public IEnumerable<Bitmap> GetActiveBackgrounds() {
            if (BgLazerPink) { yield return Res.BG_LazerPink; }
            if (BgLazerPurple) { yield return Res.BG_LazerPurple; }
            if (BgOsuAcademy) { yield return Res.BG_OsuAcademy; }
            if (BgOsuCinematic) { yield return Res.BG_OsuCinematic; }
            if (BgOsuDark) { yield return Res.BG_OsuDark; }
            if (BgOsuMinimalist) { yield return Res.BG_OsuMinimalist; }
            if (BgOsuParental) { yield return Res.BG_OsuParental; }
            if (BgOsuYukata) { yield return Res.BG_OsuYukata; }
        }

        /// <summary>Gets a random background from <see cref="GetActiveBackgrounds"/>.</summary>
        /// <returns><see cref="Bitmap"/></returns>
        public Bitmap GetRandomBackground() => GetActiveBackgrounds().GetRandom();

        /// <summary>Handles the <c>Click</c> event of all Tg (<see cref="ToggleButton"/>) controls.
        /// <para/>Updates <see cref="LblCount"/> to display the amount of currently available (selected) backgrounds.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void Tg_Click(object Sender, RoutedEventArgs E) {
            int C = GetActiveBackgrounds().ToArray().Length;

            if (C <= 0) { //At least one background MUST be active at all times
                ((ToggleButton)Sender).IsChecked = true;
                C = 1;
            }

            LblCount.Content = $"[{C}] selected";
            LblCount.Focus();
        }
    }
}
