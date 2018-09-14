using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.ViewModel
{
    public sealed class PullRequestListViewModel : IPullRequestListViewModel
    {
        public PullRequestListViewModel()
        {
            PullRequests = new ObservableCollection<PullRequestViewModel>();
        }

        public ConcurrentDictionary<int, IPullRequest> Model { get; set; }

        public void Update()
        {
            PullRequests.Clear();
            foreach (var pullRequest in Model.Values.OrderBy(pr => pr.Id))
            {
                PullRequests.Add(new PullRequestViewModel(pullRequest));
            }
        }

        public ObservableCollection<PullRequestViewModel> PullRequests { get; }
    }
}