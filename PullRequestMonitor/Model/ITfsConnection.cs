using System.Collections.Generic;
using System.Threading.Tasks;

namespace PullRequestMonitor.Model
{
    public interface ITfsConnection
    {
        Task<IEnumerable<ITfProject>> GetProjects();
        Task<IEnumerable<ITfGitRepository>> GetRepositoriesInProject(ITfProject project);
        Task<IEnumerable<IPullRequest>> GetActivePullRequestsInProject(ITfProject project);
        void ReleasePullRequest(IPullRequest pullRequest);
    }
}