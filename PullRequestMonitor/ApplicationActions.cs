using System;
using System.Windows;
using System.Windows.Input;
using PullRequestMonitor.Factories;
using PullRequestMonitor.Model;
using PullRequestMonitor.View;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor
{
    public interface IApplicationActions
    {
        ICommand ShowSettingsCommand { get; }
        ICommand ShowMonitorWindowCommand { get; }
        ICommand ExitApplicationCommand { get; }
        void UpdateMonitorViewModel();
    }

    public sealed class ApplicationActions : IApplicationActions
    {
        private readonly IMonitor _monitor;
        private readonly MonitorWindow _monitorWindow;
        private readonly IMonitorViewModelFactory _monitorViewModelFactory;
        private readonly SettingsWindow _settingsWindow;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly Lazy<MonitorWindowViewModel> _monitorViewModel;

        public ApplicationActions(IMonitor monitor, MonitorWindow  monitorWindow, IMonitorViewModelFactory monitorViewModelFactory, SettingsWindow settingsWindow, SettingsViewModel settingsViewModel)
        {
            _monitor = monitor;
            _monitorWindow = monitorWindow;
            _monitorWindow.Deactivated += MonitorWindowOnDeactivated;
            _monitorViewModelFactory = monitorViewModelFactory;
            _settingsWindow = settingsWindow;
            _settingsViewModel = settingsViewModel;
            _settingsWindow.DataContext = _settingsViewModel;

            _monitorViewModel = new Lazy<MonitorWindowViewModel>(InitializeMonitorViewModel);
        }

        private MonitorWindowViewModel InitializeMonitorViewModel()
        {
            var monitorViewModel = _monitorViewModelFactory.Create(_monitor);
            monitorViewModel.Update();
            return monitorViewModel;
        }

        public ICommand ShowSettingsCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => !_settingsWindow.IsVisible,
                    CommandAction = () =>
                    {
                        _settingsViewModel.RefreshProjects();
                        _settingsWindow.Show();
                    }
                };
            }
        }

        public ICommand ShowMonitorWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => !_monitorWindow.IsVisible,
                    CommandAction = () =>
                    {
                        _monitorWindow.DataContext = _monitorViewModel.Value;
                        _monitorViewModel.Value.Update();
                        _monitorWindow.Topmost = true;
                        _monitorWindow.Show();
                        _monitorWindow.Activate();
                    }
                };
            }
        }

        private void MonitorWindowOnDeactivated(object sender, EventArgs eventArgs)
        {
            _monitorWindow.Hide();
        }

        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
        }

        public void UpdateMonitorViewModel()
        {
            _monitorViewModel.Value.Update();
        }
    }
}