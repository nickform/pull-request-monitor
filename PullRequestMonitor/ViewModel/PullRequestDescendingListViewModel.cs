using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.ViewModel
{
    public sealed class PullRequestDescendingListViewModel : IPullRequestListViewModel
    {
        public PullRequestDescendingListViewModel()
        {
            PullRequests = new ObservableCollection<PullRequestViewModel>();
        }

        public ConcurrentDictionary<int, IPullRequest> Model { get; set; }

        public void Update()
        {
            PullRequests.Clear();
            foreach (var pullRequest in Model.Values.Where(pr => pr.Completed != null).OrderByDescending(pr => pr.Completed))
            {
                PullRequests.Add(new PullRequestViewModel(pullRequest));
            }
        }

        public ObservableCollection<PullRequestViewModel> PullRequests { get; }
    }
}