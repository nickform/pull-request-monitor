using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.ViewModel
{
    internal sealed class MonitorWindowViewModel : IUpdateable, INotifyPropertyChanged
    {
        private readonly IMonitor _model;
        private readonly INoProjectsViewModel _noProjectsViewModel;
        private readonly SingleProjectViewModel _singleProjectViewModel;
        private readonly FirstUpdateViewModel _firstUpdateViewModel;
        private readonly CouldNotReachServerViewModel _couldNotReachServerViewModel;
        private readonly UnrecognisedErrorViewModel _unrecognisedErrorViewModel;

        public MonitorWindowViewModel(IMonitor model, INoProjectsViewModel noProjectsViewModel,
            SingleProjectViewModel singleProjectViewModel, FirstUpdateViewModel firstUpdateViewModel,
            CouldNotReachServerViewModel couldNotReachServerViewModel, UnrecognisedErrorViewModel unrecognisedErrorViewModel)
        {
            _model = model;

            _noProjectsViewModel = noProjectsViewModel;
            _singleProjectViewModel =  singleProjectViewModel;
            _firstUpdateViewModel = firstUpdateViewModel;
            _couldNotReachServerViewModel = couldNotReachServerViewModel;
            _unrecognisedErrorViewModel = unrecognisedErrorViewModel;

            _model.UpdateCompleted += (sender, args) => Update();
        }

        public IUpdateable ContentViewModel
        {
            get
            {
                switch (_model.Status)
                {
                    case MonitorStatus.AwaitingFirstUpdate:
                        return _firstUpdateViewModel;
                    case MonitorStatus.CouldNotReachServer:
                        return _couldNotReachServerViewModel;
                    // AuthorisationError,
                    case MonitorStatus.NoProjects:
                        return _noProjectsViewModel;
                    case MonitorStatus.UpdateSuccessful:
                        _singleProjectViewModel.Model = _model.Projects.First();
                        return _singleProjectViewModel;
                    default:
                        return _unrecognisedErrorViewModel;
                }
            }
        }

        public void Update()
        {
            ContentViewModel.Update();
            OnPropertyChanged(nameof(ContentViewModel));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}