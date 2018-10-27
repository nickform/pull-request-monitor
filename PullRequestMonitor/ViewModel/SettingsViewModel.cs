using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using PullRequestMonitor.Factories;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly IAppSettings _model;
        private readonly ITfProjectCollectionCache _projectCollectionCache;
        private ITfProjectCollection _teamProjectCollection;

        public SettingsViewModel(IAppSettings model, ITfProjectCollectionCache projectCollectionCache)
        {
            _model = model;
            _projectCollectionCache = projectCollectionCache;

            Projects = new ObservableCollection<ProjectViewModel>();

            SetTeamProjectCollection();
        }

        private void SetTeamProjectCollection()
        {
            if (_teamProjectCollection != null)
            {
                _teamProjectCollection.ProjectRetrievalCompleted -= OnProjectRetrievalCompleted;
            }

            _teamProjectCollection = _projectCollectionCache.GetProjectCollection(_model.Account);

            _teamProjectCollection.ProjectRetrievalCompleted += OnProjectRetrievalCompleted;

            RefreshProjects();

            OnPropertyChanged(nameof(IsRefreshingProjects));
            OnPropertyChanged(nameof(HasProjects));
            OnPropertyChanged(nameof(HasSelectedProject));
        }

        public string Account
        {
            get => _model.Account;
            set
            {
                if (value == _model.Account) return;

                _model.Account = value;
                SetTeamProjectCollection();
            }
        }

        public ObservableCollection<ProjectViewModel> Projects { get; }

        public ProjectViewModel SelectedProject
        {
            get { return Projects.FirstOrDefault(p => p.Id == _model.ProjectId); }
            set
            {
                _model.ProjectId = value?.Id ?? Guid.Empty;
                OnPropertyChanged(nameof(SelectedProject));
                OnPropertyChanged(nameof(HasSelectedProject));
                SelectedProject?.RefreshRepositories();
            }
        }

        public string RepoNamePattern
        {
            get => _model.RepoNamePattern;
            set
            {
                _model.RepoNamePattern = value;
                OnPropertyChanged(nameof(RepoNamePattern));
            }
        }

        private bool IsRefreshingProjects => _teamProjectCollection.ProjectRetrievalStatus == RetrievalStatus.Ongoing;

        public bool HasProjects => _teamProjectCollection.ProjectRetrievalStatus == RetrievalStatus.Suceeded;

        public bool HasSelectedProject => SelectedProject != null;

        public void RefreshProjects()
        {
            if (IsRefreshingProjects) return;

            _teamProjectCollection.RetrieveProjects();
        }

        /// <summary>
        /// Updates a collection of <c>ProjectViewModel</c> so that it contains one entry per
        /// entry in a list of <c>ITfProject</c>. 
        /// </summary>
        /// <param name="refreshedProjects">The model objects for which <paramref name="toUpdate"/> should contain view models.</param>
        /// <param name="toUpdate">The view model collection to update. Must be sorted by project name.</param>
        public static void UpdateProjectViewModels(IEnumerable<ITfProject> refreshedProjects, ObservableCollection<ProjectViewModel> toUpdate)
        {
            var existingListIndex = 0;
            foreach (var project in refreshedProjects.OrderBy(p => p.Name))
            {
                // While the  viewmodel at this index is alphabetically before the current project,
                // remove it.
                int? comparisonResult = null;
                while (existingListIndex < toUpdate.Count &&
                    (comparisonResult = String.CompareOrdinal(project.Name, toUpdate[existingListIndex].ProjectName)) > 0)
                {
                    toUpdate.RemoveAt(existingListIndex);
                    comparisonResult = null;
                }

                // If comparisonResult is null, we have reached the end of the view models
                // and should insert a new view model for the current project here. If compa
                if (comparisonResult == null || comparisonResult < 0)
                {
                    toUpdate.Insert(existingListIndex, new ProjectViewModel(project));
                }

                // All future searches should start from the next element in the existing collection
                existingListIndex++;

                // To get here, we must have found an exact match and don't need to do anything.
            }

            // Remove any remaining view models...
            while (existingListIndex < toUpdate.Count)
            {
                toUpdate.RemoveAt(existingListIndex);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnProjectRetrievalCompleted(object sender, EventArgs eventArgs)
        {
            UpdateProjectViewModels(_teamProjectCollection.Projects, Projects);
            SelectedProject = SelectedProject;
            OnPropertyChanged(nameof(IsRefreshingProjects));
            OnPropertyChanged(nameof(HasProjects));
            OnPropertyChanged(nameof(HasSelectedProject));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}