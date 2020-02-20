using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

using Octokit;

namespace OsuBackgroundPurger {
    public partial class UpdateChecker {
        public const string company = "starflash-studios";
        public const string product = "osu-backgroundpurger";

        public Window parentWindow;

        public UpdateChecker() {
            InitializeComponent();
        }

        public static void Create(Window main = null) {
            UpdateChecker uC = new UpdateChecker();
            if (main != null) { uC.parentWindow = main; }
            uC.Init();
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
            } catch {
                hasUpdate = false;
            }
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
        public async Task<(bool, Version v)> CheckForUpdate() {
            Version curVer = Assembly.GetEntryAssembly().GetName().Version;

            GitHubClient client = new GitHubClient(new ProductHeaderValue("osu-backgroundpurger", "v" + curVer));
            IRepositoriesClient repo = client.Repository;

            foreach (Release release in await repo.Release.GetAll("starflash-studios", "osu-backgroundpurger")) {
                Debug.WriteLine("Release Found: " + release.TagName);
                Version v = Version.Parse(release.TagName);
                Debug.WriteLine("\tRelease Version: " + v);
                if (v > curVer) {
                    Debug.WriteLine("Update Required (" + curVer + " >> " + v + ")");
                    return (true, v);
                }
            }
            Debug.WriteLine("Update-to-date");
            return (false, null);
        }

        public static string Url(string product = product, string company = company) => $"https://www.github.com/{company}/{product}/";

        public static void GotoUpdate(string product = product, string company = company) => Process.Start(Url(product, company) + "releases/");

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
            if (latest == null || latest == default) { latest = current; }
            Title = Replace(templateTitle, current, latest);
            UpdateCurrentText.Text = Replace(templateCurVerText, current, latest);
            UpdateNewText.Text = Replace(templateNewVerText, current, latest);
        }

        public static string Replace(string template, Version current, Version latest) => template.Replace("%%curVer%%", current.ToString()).Replace("%%newVer%%", latest.ToString());

        void UpdateButton_Click(object sender, RoutedEventArgs e) => GotoUpdate();
        
        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => parentWindow?.Show();

        #endregion
    }

    public static class UpdateCheckerExtensions {

        public static float Positive(this float a) => a < 0 ? -a : a;
        public static float Negative(this float a) => a < 0 ? a : -a;

        public static int Positive(this int a) => a < 0 ? -a : a;
        public static int Negative(this int a) => a < 0 ? a : -a;
    }
}
