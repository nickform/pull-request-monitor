using System.ComponentModel;
using System.Runtime.CompilerServices;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.ViewModel
{
    public sealed class SingleProjectViewModel : IUpdateable, INotifyPropertyChanged
    {
        private ITfProject _model;

        public SingleProjectViewModel(ActivePullRequestListViewModel unapproved, ActivePullRequestListViewModel approved, CompletedPullRequestListViewModel completed)
        {
            Unapproved = unapproved;
            Approved = approved;
            Completed = completed;
        }

        public ITfProject Model
        {
            get => _model;

            set
            {
                if (value == _model)
                {
                    return;
                }

                _model = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Name => Model == null ? "" : Model.Name;

        public ActivePullRequestListViewModel Unapproved { get; }

        public ActivePullRequestListViewModel Approved { get; }

        public CompletedPullRequestListViewModel Completed { get; }

        public void Update()
        {
            if (Model != null)
            {
                Unapproved.Model = Model.Unapproved;
                Approved.Model = Model.Approved;
                Completed.Model = Model.Completed;
            }

            Unapproved.Update();
            Approved.Update();
            Completed.Update();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}