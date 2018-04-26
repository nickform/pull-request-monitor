using System.Windows.Input;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.ViewModel
{
    internal sealed class CouldNotReachServerViewModel : IUpdateable
    {
        private readonly IApplicationActions _applicationActions;
        private readonly IAppSettings _settings;

        public CouldNotReachServerViewModel(IApplicationActions applicationActions, IAppSettings settings)
        {
            _applicationActions = applicationActions;
            _settings = settings;
        }

        public string ServerWebViewUrl => VstsServerURL.GetVstsServerURL(_settings.VstsAccount);
        public ICommand ShowSettingsCommand => _applicationActions.ShowSettingsCommand;

        public void Update() {}
    }
}