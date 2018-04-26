using PullRequestMonitor.Model;

namespace PullRequestMonitor.ViewModel
{
    public class RepositoryViewModel
    {
        private readonly ITfGitRepository _model;

        public RepositoryViewModel(ITfGitRepository model)
        {
            _model = model;
        }

        public string Name => _model.Name;
    }
}