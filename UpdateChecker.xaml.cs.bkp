using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

using Octokit;

namespace OsuBackgroundPurger {
    public partial class UpdateChecker {
        public static Version currentVersion = new Version(0, 0, 9 ,0);
        public const string company = "starflash-studios";
        public const string product = "osu-backgroundpurger";

        Window parent;

        public UpdateChecker() {
            InitializeComponent();
        }

        public static void Create(Window main = null) {
            UpdateChecker uC = new UpdateChecker();
            if (main != null) { uC.parent = main; }
            uC.Init();
        }

        public void Init() {
            Debug.WriteLine("Init Called, Updating UI");
            Debug.WriteLine("Parent set");
            InitUI();

            Debug.WriteLine("Checking for update; Current Version: " + currentVersion);
            bool hasUpdate = false;
            Version latestVersion = default;
            try {
                Task<Tuple<Version, bool>> a = GetUpdate();
                Debug.WriteLine("Created Task; Awaiting result");
                a.Wait();
                (Version latest, bool needsUpdate) = a.Result;
                Debug.WriteLine("Latest Version: " + latest + " | Has Update: " + hasUpdate);
                hasUpdate = needsUpdate;
                latestVersion = latest;
            } catch {
                hasUpdate = false;
            }
            if (hasUpdate) {
                Debug.WriteLine("Update Required; Showing Popup");
                UIReplace(currentVersion, latestVersion);
                Show();
            } else {
                Debug.WriteLine("No Update Required; Showing Parent Window (if existent)");
                parent?.Show();
                Hide();
                Close();
            }
        }


        #region Update Checking
        /// <summary>
        /// Returns whether the program needs an update or not
        /// </summary>
        /// <param name="product"></param>
        /// <param name="company"></param>
        /// <returns>Bool: True = Needs Update, False = Up to date</returns>
        public static async Task<bool> CheckForUpdate(string product = product, string company = company) => (await GetUpdate(product, company)).Item2;

        public static async Task<Tuple<Version, bool>> GetUpdate(string product = product, string company = company) {
            GitHubClient client = new GitHubClient(new ProductHeaderValue(product));
            Debug.WriteLine("Client: " + (client != null));
            Debug.WriteLine("Checking releases with Owner: " + company + " & Product: " + product);
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll(company, product);
            Debug.WriteLine("<<START>>");
            foreach (Release release in releases) {
                Debug.WriteLine("\tFound release: " + release.TagName + " >> " + release.Name);
            }
            Debug.WriteLine("<<END>>");
            Version latest = new Version(releases[0]);
            Debug.WriteLine("Latest version: " + latest);
            Debug.WriteLine("Current version: " + currentVersion);

            return new Tuple<Version, bool>(latest, currentVersion.IsOlder(latest));
        }

        public static string Url(string product = product, string company = company) => $"https://www.github.com/{company}/{product}/";

        public static void GotoUpdate(string product = product, string company = company) => Process.Start(Url(product, company));

        public struct Version {
            public int major;
            public int sector;
            public int minor;
            public int fix;

            public override string ToString() => $"{major}.{sector}.{minor}.{fix}";

            public Version(int maj = 0, int min = 0, int sec = 0, int typ = 0) {
                major = maj;
                sector = min;
                minor = sec;
                fix = typ;
            }

            public Version(string parse) {
                string[] split = parse.Split("."[0]);
                major = int.Parse(split[0]);
                sector = int.Parse(split[1]);
                minor = int.Parse(split[2]);
                fix = int.Parse(split[3]);
            }

            public Version(Release release) {
                string[] split = release.TagName.Split("."[0]);
                major = int.Parse(split[0]);
                sector = int.Parse(split[1]);
                minor = int.Parse(split[2]);
                fix = int.Parse(split[3]);
            }

            public Version EnforcePositive() => new Version(major.Positive(), sector.Positive(), minor.Positive(), fix.Positive());

            /// <summary>
            /// Returns false if 'other' is a higher version
            /// </summary>
            /// <param name="other"></param>
            /// <param name="levelOfScrutiny">0: Any value, 1: Security or above, 2: Minor or above, 3: Major or above</param>
            /// <returns></returns>
            public bool IsNewer(Version other, int levelOfScrutiny = 0) => !IsOlder(other, levelOfScrutiny);

            /// <summary>
            /// Returns true if 'other' is a higher version
            /// </summary>
            /// <param name="other"></param>
            /// <param name="levelOfScrutiny">0: Any value, 1: Security or above, 2: Minor or above, 3: Major or above</param>
            /// <returns></returns>
            public bool IsOlder(Version other, int levelOfScrutiny = 0) {
                switch (levelOfScrutiny) {
                    case 1:
                        return other.major > major || other.sector > sector || other.minor > minor;
                    case 2:
                        return other.major > major || other.sector > sector;
                    case 3:
                        return other.major > major;
                    default:
                        return other.major > major || other.sector > sector || other.minor > minor || other.fix > fix;
                }
            }
        }
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

        public void UIReplace(Version current, Version latest) {
            Title = Replace(templateTitle, current, latest);
            UpdateCurrentText.Text = Replace(templateCurVerText, current, latest);
            UpdateNewText.Text = Replace(templateNewVerText, current, latest);
        }

        public static string Replace(string template, Version current, Version latest) => template.Replace("%%curVer%%", current.ToString()).Replace("%%newVer%%", latest.ToString());

        void UpdateButton_Click(object sender, RoutedEventArgs e) => GotoUpdate();
        
        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => parent?.Show();

        #endregion
    }

    public static class UpdateCheckerExtensions {

        public static float Positive(this float a) => a < 0 ? -a : a;
        public static float Negative(this float a) => a < 0 ? a : -a;

        public static int Positive(this int a) => a < 0 ? -a : a;
        public static int Negative(this int a) => a < 0 ? a : -a;
    }
}
