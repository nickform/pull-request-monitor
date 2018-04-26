using System;

namespace PullRequestMonitor.Model
{
    public interface IAppSettings
    {
        string VstsAccount { get; set; }
        Guid ProjectId { get; set; }
        string RepoNamePattern { get; set; }
        int PollIntervalSeconds { get; }
        event EventHandler SettingsChanged;
    }
}