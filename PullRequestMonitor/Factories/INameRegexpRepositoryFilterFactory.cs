using PullRequestMonitor.Model;

namespace PullRequestMonitor.Factories
{
    public interface INameRegexpRepositoryFilterFactory
    {
        NameRegexpRepositoryFilter Create(string repoNamePattern);
        void Release(NameRegexpRepositoryFilter toRelease);
    }
}