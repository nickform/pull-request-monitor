using System;
using PullRequestMonitor.Model;

namespace PullRequestMonitor
{
    public class AppSettings : IAppSettings
    {
        public AppSettings()
        {
            if (Properties.Settings.Default.VstsAccount == "")
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.Save();
            }
        }

        public event EventHandler SettingsChanged;
        public string VstsAccount
        {
            get => Properties.Settings.Default.VstsAccount;
            set
            {
                if (value == Properties.Settings.Default.VstsAccount) return;

                Properties.Settings.Default.VstsAccount = value;
                // Reset other repo-locating settings on change of account.
                Properties.Settings.Default.ProjectId = Guid.Empty;
                Properties.Settings.Default.RepoNamePattern = ".*";
                Save();
            }
        }

        public Guid ProjectId
        {
            get => Properties.Settings.Default.ProjectId;
            set
            {
                if (value == Properties.Settings.Default.ProjectId) return;

                Properties.Settings.Default.ProjectId = value;
                Save();
            }
        }

        public string RepoNamePattern
        {
            get => Properties.Settings.Default.RepoNamePattern;
            set
            {
                if (value == Properties.Settings.Default.RepoNamePattern) return;

                Properties.Settings.Default.RepoNamePattern = value;
                Save();
            }
        }
        public int PollIntervalSeconds => Properties.Settings.Default.PollIntervalSeconds;

        private void Save()
        {
            Properties.Settings.Default.Save();
            OnSettingsChanged();
        }

        private void OnSettingsChanged()
        {
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}