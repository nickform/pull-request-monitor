using Microsoft.TeamFoundation.Core.WebApi;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.Factories
{
    public interface ITfProjectFactory
    {
        ITfProject Create(TeamProjectReference projectReference, ITfsConnection tfsConnection);
        void Release(ITfProject project);
    }
}