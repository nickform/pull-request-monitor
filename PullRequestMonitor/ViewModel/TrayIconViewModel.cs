using System;
using System.ComponentModel;
using System.Windows.Input;
using PullRequestMonitor.Model;
using static System.String;

namespace PullRequestMonitor.ViewModel
{
    public interface ITrayIconViewModel : INotifyPropertyChanged
    {
        ICommand ShowSettingsCommand { get; }
        ICommand ShowMonitorWindowCommand { get; }
        ICommand ExitApplicationCommand { get; }
        ITrayIcon Model { get; set; }
        string TooltipText { get; }
    }

    public sealed class TrayIconViewModel : ITrayIconViewModel
    {
        private readonly IApplicationActions _applicationActions;
        private ITrayIcon _model;

        public TrayIconViewModel(IApplicationActions applicationActions)
        {
            _applicationActions = applicationActions;
        }

        public ITrayIcon Model
        {
            get => _model;
            set
            {
                if (value == _model) return;
                if (_model != null) _model.UpdateCompleted -= OnModelUpdateCompleted;
                _model = value;
                _model.UpdateCompleted += OnModelUpdateCompleted;
            }
        }

        private void OnModelUpdateCompleted(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged(nameof(PullRequestCount));
            OnPropertyChanged(nameof(TooltipText));
            _applicationActions.UpdateMonitorViewModel();
        }

        /// <summary>
        /// Shows the settings window.
        /// </summary>
        public ICommand ShowSettingsCommand => _applicationActions.ShowSettingsCommand;

        /// <summary>
        /// Toggles visibility of the Monitor window.
        /// </summary>
        public ICommand ShowMonitorWindowCommand => _applicationActions.ShowMonitorWindowCommand;

        /// <summary>
        /// Closes the application.
        /// </summary>
        public ICommand ExitApplicationCommand => _applicationActions.ExitApplicationCommand;

        public string TooltipText
        {
            get
            {
                switch (_model.MonitorStatus)
                {
                    case MonitorStatus.AwaitingFirstUpdate:
                        return Properties.Resources.AwaitingFirstUpdateMessage;
                    case MonitorStatus.NoProjects:
                        return Properties.Resources.NoProjectsMessage;
                    case MonitorStatus.CouldNotReachServer:
                        return Properties.Resources.CouldNotReachServerMessage;
                    case MonitorStatus.UnrecognisedError:
                        return Properties.Resources.UnrecognisedErrorMessage;
                    case MonitorStatus.AuthorisationError:
                        return Properties.Resources.AuthorisationErrorMessage;
                    case MonitorStatus.UpdateSuccessful:
                        return Format(Properties.Resources.PullRequestCountTooltipFormatString,
                            _model.UnapprovedPullRequestCount, _model.ApprovedPullRequestCount);
                    default:
                        throw new Exception("A new MonitorStatus code was added but this switch statement was not updated");
                }
            }
        }

        public int? PullRequestCount => _model.PullRequestCount;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}