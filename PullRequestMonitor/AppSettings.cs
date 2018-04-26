using System;
using PullRequestMonitor.Model;

namespace PullRequestMonitor
{
    public class AppSettings : IAppSettings
    {
        public event EventHandler SettingsChanged;
        public string VstsAccount
        {
            get => Properties.Settings.Default.VstsAccount;
            set
            {
                if (value == Properties.Settings.Default.VstsAccount) return;

                Properties.Settings.Default.VstsAccount = value;
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
                Properties.Settings.Default.RepoNamePattern = value;
                Properties.Settings.Default.Save();
                OnSettingsChanged();
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