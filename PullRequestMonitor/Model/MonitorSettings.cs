using System;
using System.Collections.Generic;
using System.Linq;

namespace PullRequestMonitor.Model
{
    public interface IMonitorSettings
    {
        IEnumerable<MonitoredProjectSettings> Projects { get; }

        /// <summary>
        /// The period between successive polls to the server in ms.
        /// </summary>
        int PollInterval { get; }
        /// <summary>
        /// Indicates that the settings have been changed.
        /// </summary>
        event EventHandler SettingsChanged;
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class MonitorSettings : IMonitorSettings
    {
        private readonly IAppSettings _appSettings;
        public event EventHandler SettingsChanged;

        public MonitorSettings(IAppSettings appSettings)
        {
            _appSettings = appSettings;
            _appSettings.SettingsChanged += OnSettingsChanged;
        }

        public IEnumerable<MonitoredProjectSettings> Projects
        {
            get
            {
                if (_appSettings.Account == "" || _appSettings.ProjectId == Guid.Empty)
                {
                    return Enumerable.Empty<MonitoredProjectSettings>();
                }

                return new[]
                {
                    new MonitoredProjectSettings
                    {
                        Id = _appSettings.ProjectId,
                        Account = _appSettings.Account,
                        RepoNameRegexp = _appSettings.RepoNamePattern
                    }
                };
            }
        }

        public int PollInterval => 1000 * _appSettings.PollIntervalSeconds;
        private void OnSettingsChanged(object sender, EventArgs eventArgs)
        {
            SettingsChanged?.Invoke(this, eventArgs);
        }
    }
}