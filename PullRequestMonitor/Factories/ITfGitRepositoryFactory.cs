using Microsoft.TeamFoundation.SourceControl.WebApi;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.Factories
{
    public interface ITfGitRepositoryFactory
    {
        ITfGitRepository Create(GitRepository gitRepository, ITfProject project);
        void Release(ITfGitRepository toRelease);
    }
}