using PullRequestMonitor.Model;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.Factories
{
    public interface ITrayIconViewModelFactory
    {
        ITrayIconViewModel Create(ITrayIcon model);
        void Release(ITrayIconViewModel trayIconViewModel);
    }
}