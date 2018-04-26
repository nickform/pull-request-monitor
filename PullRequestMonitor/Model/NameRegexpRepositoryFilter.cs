using System.Text.RegularExpressions;

namespace PullRequestMonitor.Model
{
    public class NameRegexpRepositoryFilter : IRepositoryFilter
    {
        private readonly string _repoNamePattern;

        public NameRegexpRepositoryFilter(string repoNamePattern)
        {
            _repoNamePattern = repoNamePattern;
        }

        public bool IncludesRepo(ITfGitRepository repository)
        {
            return Regex.IsMatch(repository.Name, _repoNamePattern);
        }
    }
}