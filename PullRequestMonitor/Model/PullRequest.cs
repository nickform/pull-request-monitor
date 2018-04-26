using System;
using System.Linq;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace PullRequestMonitor.Model
{
    public interface IPullRequest
    {
        ITfGitRepository Repository { get; }
        bool IsApproved { get; }
        int Id { get; }
        string Title { get; }
        string AuthorDisplayName { get; }
        DateTime Created { get; }
        Uri WebViewUri { get; }
    }

    internal sealed class PullRequest : IPullRequest
    {
        private readonly GitPullRequest _pullRequest;
        private readonly string _serverUri;

        public PullRequest(GitPullRequest pullRequest, string serverUri, ITfGitRepository repository)
        {
            _pullRequest = pullRequest;
            _serverUri = serverUri;
            Repository = repository;
        }

        public ITfGitRepository Repository { get; }
        public bool IsApproved => _pullRequest.Reviewers.Any(reviewer => reviewer.Vote > 0) && !_pullRequest.Reviewers.Any(reviewer => reviewer.Vote < 0);
        public int Id => _pullRequest.PullRequestId;
        public string Title => _pullRequest.Title;
        public string AuthorDisplayName => _pullRequest.CreatedBy.DisplayName;
        public DateTime Created => _pullRequest.CreationDate;

        public Uri WebViewUri => new Uri($"{_serverUri}{Repository.Project.Name}/_git/{Repository.Name}/pullrequest/{Id:d}");
    }
}