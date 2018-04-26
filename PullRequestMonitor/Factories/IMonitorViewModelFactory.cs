using PullRequestMonitor.Model;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.Factories
{
    internal interface IMonitorViewModelFactory
    {
        MonitorWindowViewModel Create(IMonitor model);
        void Release(MonitorWindowViewModel monitorWindowViewModel);
    }
}