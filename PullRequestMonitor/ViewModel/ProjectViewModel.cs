using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.ViewModel
{
    public class ProjectViewModel : IEquatable<ProjectViewModel>, INotifyPropertyChanged
    {
        private readonly ITfProject _model;

        public ProjectViewModel(ITfProject model)
        {
            _model = model;
            _model.RepositoriesUpdated += (sender, args) => OnRepositoriesUpdated();

            Repositories = new ObservableCollection<RepositoryViewModel>();
        }

        public string ProjectName => _model.Name;
        public Guid Id => _model.Id;

        public ObservableCollection<RepositoryViewModel> Repositories { get; }

        /// <summary>
        /// Updates the Repositories 
        /// </summary>
        public void RefreshRepositories()
        {
            _model.RetrieveRepositories();
        }

        public bool Equals(ProjectViewModel other)
        {
            if (other == null) return false;

            return other.Id == Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProjectViewModel) obj);
        }

        public override int GetHashCode()
        {
            return _model.Id.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnRepositoriesUpdated()
        {
            Repositories.Clear();
            foreach (var modelRepository in _model.Repositories.OrderBy(repo => repo.Name))
            {
                Repositories.Add(new RepositoryViewModel(modelRepository));
            }
            OnPropertyChanged(nameof(Repositories));
        }
    }
}