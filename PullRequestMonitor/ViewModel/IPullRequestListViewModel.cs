using System.Collections.Concurrent;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.ViewModel
{
    internal interface IPullRequestListViewModel
    {
        ConcurrentDictionary<int, IPullRequest> Model { get; set; }

        void Update();
    }
}