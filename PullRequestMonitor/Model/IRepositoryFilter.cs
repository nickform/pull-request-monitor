namespace PullRequestMonitor.Model
{
    public interface IRepositoryFilter
    {
        bool IncludesRepo(ITfGitRepository repository);
    }
}