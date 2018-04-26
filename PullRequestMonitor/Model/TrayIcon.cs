using System;
using System.Windows;

namespace PullRequestMonitor.Model
{
    public interface ITrayIcon
    {
        int? PullRequestCount { get; }
        int UnapprovedPullRequestCount { get; }
        int ApprovedPullRequestCount { get; }
        event EventHandler UpdateCompleted;
        void RunMonitor();
        MonitorStatus MonitorStatus { get; }
    }

    internal sealed class TrayIcon : ITrayIcon
    {
        private readonly IMonitor _monitor;

        public TrayIcon(IMonitor monitor)
        {
            _monitor = monitor;
            _monitor.UpdateCompleted += OnMonitorUpdateCompleted;
        }

        private void OnMonitorUpdateCompleted(object sender, EventArgs eventArgs)
        {
            Application.Current.Dispatcher.Invoke(() => { UpdateCompleted?.Invoke(this, EventArgs.Empty); });
        }

        public int? PullRequestCount => _monitor.Status == MonitorStatus.UpdateSuccessful ? UnapprovedPullRequestCount + ApprovedPullRequestCount : null as int?;
        public int UnapprovedPullRequestCount => _monitor.UnapprovedPullRequestCount;
        public int ApprovedPullRequestCount => _monitor.ApprovedPullRequestCount;
        public event EventHandler UpdateCompleted;
        public void RunMonitor()
        {
            _monitor.Start();
        }

        public MonitorStatus MonitorStatus => _monitor.Status;
    }
}
