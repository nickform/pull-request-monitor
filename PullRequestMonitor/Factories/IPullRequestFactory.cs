using Microsoft.TeamFoundation.SourceControl.WebApi;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.Factories
{
    public interface IPullRequestFactory
    {
        IPullRequest Create(GitPullRequest pullRequest, string serverUri, ITfGitRepository repository);
        void Release(IPullRequest pullRequest);
    }
}