using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PullRequestMonitor.Model
{
    /// <summary>
    /// Represents a TFS project.
    /// </summary>
    public interface ITfProject
    {
        Guid Id { get; }
        string Name { get; }
        Task RetrievePullRequests();
        RetrievalStatus PullRequestRetrievalStatus { get; }
        ConcurrentDictionary<int, IPullRequest> Approved { get; }
        ConcurrentDictionary<int, IPullRequest> Unapproved { get; }
        ConcurrentDictionary<int, IPullRequest> Completed { get; }
        int ApprovedPullRequestCount { get; }
        int UnapprovedPullRequestCount { get; }
        int CompletedPullRequestCount { get; }
        Task RetrieveRepositories();
        RetrievalStatus RepositoryRetrievalStatus { get; }
        IEnumerable<ITfGitRepository> Repositories { get; }
        IRepositoryFilter RepositoryFilter { get; set; }
        event EventHandler RepositoriesUpdated;
    }
}