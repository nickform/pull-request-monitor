using PullRequestMonitor.Model;

namespace PullRequestMonitor.Factories
{
    public interface ITrayIconFactory
    {
        ITrayIcon Create(IMonitor monitor);
        void Release(ITrayIcon trayIcon);
    }
}