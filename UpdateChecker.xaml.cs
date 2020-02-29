using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Octokit;

namespace OsuBackgroundPurger {
    public partial class UpdateChecker {
        public static UpdateChecker instance;
        public const string company = "starflash-studios";
        public const string product = "osu-backgroundpurger";

        public static Version version => Assembly.GetEntryAssembly().GetName().Version;

        public Window parentWindow;

        public UpdateChecker() {
            InitializeComponent();
            instance = this;
        }

        /// <summary>
        /// Creates and initialises the UpdateChecker
        /// </summary>
        public static async void Create(Dispatcher dispatcher, Window window = null) {
            instance?.Close();
            dispatcher.Invoke(() => {
                UpdateChecker uC = new UpdateChecker {
                    parentWindow = window
                };
                uC.Init();
            });
            await Task.Delay(300);
        }

        public async void Init() {
            Debug.WriteLine("Init Called, Updating UI");
            Debug.WriteLine("Parent set");
            InitUI();

            Version currentVersion = Assembly.GetEntryAssembly().GetName().Version;
            Debug.WriteLine("Checking for update; Current Version: " + currentVersion);

            Version latestVersion = default;
            bool hasUpdate;
            try {
                (bool b, Version v) = await CheckForUpdate();
                Debug.WriteLine("Created Task; Awaiting result");
                hasUpdate = b;
                latestVersion = v;
#pragma warning disable CA1031 // Do not catch general exception types
            } catch {
                hasUpdate = false;
            }
#pragma warning restore CA1031 // Do not catch general exception types
            if (hasUpdate) {
                Debug.WriteLine("Update Required; Showing Popup");
                UIReplace(currentVersion, latestVersion);
                Show();
            } else {
                Debug.WriteLine("No Update Required; Showing Parent Window (if existent)");
                parentWindow?.Show();
                Hide();
                Close();
            }
        }

        #region Update Checking
        /// <summary>
        /// Returns a list of all available releases from GitHub
        /// </summary>
        /// <returns></returns>
        public static async Task<IReadOnlyList<Release>> GetReleasesAsync() {
            GitHubClient client = new GitHubClient(new ProductHeaderValue(@"osu-backgroundpurger", "v" + version));
            IRepositoriesClient repo = client.Repository;

            return await repo.Release.GetAll("starflash-studios", @"osu-backgroundpurger");
        }

        /// <summary>
        /// Iterates through each available release to see if any is newer than the current installed version
        /// </summary>
        /// <returns></returns>
        public static async Task<(bool, Version v)> CheckForUpdate() {
            Version newest = null;
            foreach (Release release in await GetReleasesAsync()) {
                Debug.WriteLine("Release Found: " + release.TagName);
                Version v = Version.Parse(release.TagName);
                if (newest == null || v > newest) { newest = v; }
            }

            if (newest > version) {
                Debug.WriteLine("Update Required (" + version + " >> " + newest + ")");
                return (true, newest);
            }

            Debug.WriteLine("Update-to-date");
            return (false, null);
        }

        /// <summary>
        /// Returns a GitHub project url from the given product and company names
        /// </summary>
        /// <param name="product"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        public static string Url(string product = product, string company = company) => $"https://www.github.com/{company}/{product}/";

        /// <summary>
        /// Opens the GitHub project releases page in the OS's default browser
        /// </summary>
        /// <param name="product"></param>
        /// <param name="company"></param>
        public static void GotoUpdate(string product = product, string company = company) => Process.Start(Url(product, company) + "releases/");

        /// <summary>
        /// Opens the GitHub project page in the OS's default browser
        /// </summary>
        /// <param name="product"></param>
        /// <param name="company"></param>
        public static void GotoPage(string product = product, string company = company) => Process.Start(Url(product, company));

        #endregion

        #region UI
        public string templateTitle;
        public string templateCurVerText;
        public string templateNewVerText;

        void InitUI() {
            Hide();
            templateTitle = Title;
            templateCurVerText = UpdateCurrentText.Text;
            templateNewVerText = UpdateNewText.Text;
        }

        /// <summary>
        /// Replaces UI template variables with actual readable values
        /// </summary>
        /// <param name="current"></param>
        /// <param name="latest"></param>
        public void UIReplace(Version current, Version latest) {
            if (latest == null || latest == default) { latest = current; }
            Title = Replace(templateTitle, current, latest);
            UpdateCurrentText.Text = Replace(templateCurVerText, current, latest);
            UpdateNewText.Text = Replace(templateNewVerText, current, latest);
        }

        /// <summary>
        /// Replaces %%curVer%% and %%newVer%% to the given current and latest versions
        /// </summary>
        /// <param name="template"></param>
        /// <param name="current"></param>
        /// <param name="latest"></param>
        /// <returns></returns>
        public static string Replace(string template, Version current, Version latest) => template.Replace("%%curVer%%", current.ToString()).Replace("%%newVer%%", latest.ToString());

        void UpdateButton_Click(object sender, RoutedEventArgs e) => GotoUpdate();
        
        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => parentWindow?.Show();

        #endregion
    }

    public static class UpdateCheckerExtensions {
        /// <summary>
        /// Enforces the value to be positive (float)
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static float Positive(this float a) => a < 0 ? -a : a;

        /// <summary>
        /// Enforces the value to be negative (float)
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static float Negative(this float a) => a < 0 ? a : -a;

        /// <summary>
        /// Enforces the value to be positive (int)
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static int Positive(this int a) => a < 0 ? -a : a;

        /// <summary>
        /// Enforces the value to be negative (int)
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static int Negative(this int a) => a < 0 ? a : -a;
    }
}
