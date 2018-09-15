using PullRequestMonitor.Model;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.Factories
{
    public interface IMonitorViewModelFactory
    {
        MonitorWindowViewModel Create(IMonitor model);
        void Release(MonitorWindowViewModel monitorWindowViewModel);
    }
}