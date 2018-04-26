using System.Windows.Input;

namespace PullRequestMonitor.ViewModel
{
    public interface INoProjectsViewModel : IUpdateable
    {
        ICommand ShowSettingsCommand { get; }
    }

    internal sealed class NoProjectsViewModel : INoProjectsViewModel
    {
        private readonly IApplicationActions _applicationActions;

        public NoProjectsViewModel(IApplicationActions applicationActions)
        {
            _applicationActions = applicationActions;
        }

        public ICommand ShowSettingsCommand => _applicationActions.ShowSettingsCommand;
        public void Update() { }
    }
}