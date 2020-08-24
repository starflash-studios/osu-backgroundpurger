#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Octokit;

#endregion

namespace Osu_BackgroundPurge.Windows {
    /// <summary>A <see cref="Window"/> handling update-checking for this application.</summary>
    /// <seealso cref="Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class UpdateWindow {
        /// <summary>The instance.</summary>
        public static UpdateWindow Instance;
        /// <summary>The company.</summary>
        public const string Company = "starflash-studios";
        /// <summary>The product.</summary>
        public const string Product = "osu-backgroundpurger";

        /// <summary>Gets the current (running) version.</summary>
        /// <value>The current running version.</value>
        public static Version Version => Assembly.GetEntryAssembly().GetName().Version;

        /// <summary>The latest found version (if any).</summary>
        public static Version Latest;

        /// <summary>The parent window.</summary>
        public Window ParentWindow;

        /// <summary>Initialises a new instance of the <see cref="UpdateWindow"/> class.</summary>
        public UpdateWindow() {
            InitializeComponent();
            Instance = this;
        }

        /// <summary>Asynchronously creates and initialises a <see cref="UpdateWindow"/>.</summary>
        public static async Task CreateAsync(Window Window) {
            if (Instance != null) {
                Instance.Show();
            } else {
                Window.Dispatcher.Invoke(() => {
                    UpdateWindow UC = new UpdateWindow {
                        ParentWindow = Window
                    };
                    UC.Init();
                });
                await Task.Delay(300).ConfigureAwait(false);
            }
        }

        /// <summary>Initialises this instance.</summary>
        public async void Init() {
            Debug.WriteLine("Init Called, Updating UI");
            Debug.WriteLine("Parent set");
            InitUI();

            Version CurrentVersion = Assembly.GetEntryAssembly().GetName().Version;
            Debug.WriteLine($"Checking for update; Current Version: {CurrentVersion}");

            bool HasUpdate;
            try {
                (bool B, Version V) = await CheckForUpdateAsync().ConfigureAwait(false);
                Debug.WriteLine("Created Task; Awaiting result");
                HasUpdate = B;
                Latest = V;
                #pragma warning disable CA1031 // Do not catch general exception types
            } catch {
                #pragma warning restore CA1031 // Do not catch general exception types
                HasUpdate = false;
            }

            Dispatcher.Invoke(() => {
                if (HasUpdate) {
                    Debug.WriteLine("Update Required; Showing Popup");
                    UIReplace(CurrentVersion, Latest);
                    Show();
                } else {
                    Debug.WriteLine("No Update Required; Showing Parent Window (if existent)");
                    ParentWindow?.Show();
                    Hide();
                    Close();
                }
            });
        }

        #region Update Checking

        /// <summary>Asynchronously returns a <see cref="IReadOnlyList{Release}"/> of all available <see cref="Release"/>(s) from GitHub.</summary>
        /// <returns><see cref="IReadOnlyList{Release}"/></returns>
        public static async Task<IReadOnlyList<Release>> GetReleasesAsync() {
            GitHubClient Client = new GitHubClient(new ProductHeaderValue(@"osu-backgroundpurger", $"v{Version}"));
            IRepositoriesClient Repo = Client.Repository;

            // ReSharper disable once AsyncConverter.AsyncAwaitMayBeElidedHighlighting
            return await Repo.Release.GetAll("starflash-studios", @"osu-backgroundpurger").ConfigureAwait(false);
        }

        /// <summary>Asynchronously iterates through all available releases, returning <c>true</c> if a release has a higher version than the currently installed one.</summary>
        /// <returns><see cref="ValueTuple{Boolean, Version}"/></returns>
        public static async Task<(bool, Version v)> CheckForUpdateAsync() {
            Version Newest = null;
            foreach (Release Release in await GetReleasesAsync().ConfigureAwait(false)) {
                Debug.WriteLine($"Release Found: {Release.TagName}");
                Version V = Version.Parse(Release.TagName);
                if (Newest == null || V > Newest) { Newest = V; }
            }

            if (Newest > Version) {
                Debug.WriteLine($"Update Required ({Version} >> {Newest})");
                return (true, Newest);
            }

            Debug.WriteLine("Update-to-date");
            return (false, null);
        }

        /// <summary>Returns a GitHub project url from the given product and company names.</summary>
        /// <param name="Product">The product.</param>
        /// <param name="Company">The company.</param>
        /// <returns></returns>
        public static string Url(string Product = Product, string Company = Company) => $"https://www.github.com/{Company}/{Product}/";

        /// <summary>Opens the GitHub project page in the system's default browser.</summary>
        /// <param name="Product">The product.</param>
        /// <param name="Company">The company.</param>
        public static void GotoPage(string Product = Product, string Company = Company) => Process.Start(Url(Product, Company));

        /// <summary>Opens the GitHub project releases page in the system's default browser.</summary>
        /// <param name="Product">The product.</param>
        /// <param name="Company">The company.</param>
        public static void GotoReleases(string Product = Product, string Company = Company) => Process.Start($"{Url(Product, Company)}releases/");

        /// <summary>Opens the specified release page of a GitHub project in the system's default browser. </summary>
        /// <param name="Version">The version.</param>
        /// <param name="Product">The product.</param>
        /// <param name="Company">The company.</param>
        public static void GotoUpdate(Version Version, string Product = Product, string Company = Company) => Process.Start($"{Url(Product, Company)}releases/tag/{Version}");

        #endregion

        #region UI        
        /// <summary>The templated 'title' string containing variables to replace.</summary>
        public string TemplateTitle;
        /// <summary>The templated 'current version' string containing variables to replace.</summary>
        public string TemplateCurVerText;
        /// <summary>The templated 'new version' string containing variables to replace.</summary>
        public string TemplateNewVerText;

        /// <summary>Initialises the UI.</summary>
        void InitUI() {
            Hide();
            TemplateTitle = Title;
            TemplateCurVerText = UpdateCurrentText.Text;
            TemplateNewVerText = UpdateNewText.Text;
        }

        /// <summary>Replaces UI template variables with readable values.</summary>
        /// <param name="Current">The current version.</param>
        /// <param name="Latest">The latest version.</param>
        public void UIReplace(Version Current, Version Latest) {
            if (Latest == null || Latest == default) { Latest = Current; }
            Title = Replace(TemplateTitle, Current, Latest);
            UpdateCurrentText.Text = Replace(TemplateCurVerText, Current, Latest);
            UpdateNewText.Text = Replace(TemplateNewVerText, Current, Latest);
        }

        /// <summary>Replaces <c>%%curVer%%</c> and <c>%%newVer%%</c> to the given current and latest versions.</summary>
        /// <param name="Template">The templated string.</param>
        /// <param name="Current">The current version.</param>
        /// <param name="Latest">The latest version.</param>
        /// <returns></returns>
        public static string Replace(string Template, Version Current, Version Latest) => Template.Replace("%%curVer%%", Current.ToString()).Replace("%%newVer%%", Latest.ToString());

        /// <summary>Handles the <c>Click</c> event of the <see cref="ReleasesButton"/> control. Calls <see cref="GotoReleases(string, string)"/>.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void ReleasesButton_Click(object Sender, RoutedEventArgs E) => GotoReleases();

        /// <summary>Handles the <c>Click</c> event of the <see cref="UpdateButton"/> control. Calls <see cref="GotoUpdate(System.Version, string, string)"/> with <see cref="Latest"/> as the supplied <see cref="System.Version"/>.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void UpdateButton_Click(object Sender, RoutedEventArgs E) => GotoUpdate(Latest);

        /// <summary>Handles the <c>Closing</c> event of the <see cref="Window"/>. Calls <see cref="ParentWindow"/>.<see cref="Window.Show"/>.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        void Window_Closing(object Sender, CancelEventArgs E) => ParentWindow?.Show();

        #endregion
    }
}
