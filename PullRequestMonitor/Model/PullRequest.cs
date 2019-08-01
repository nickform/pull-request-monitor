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
        DateTime? Completed { get;  }
        Uri WebViewUri { get; }
    }

    public sealed class PullRequest : IPullRequest
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
        public bool IsApproved
        {
            get
            {
                IdentityRefWithVote[] reviewers = _pullRequest.Reviewers;
                if (reviewers == null)
                    return false;

                return reviewers.Any(reviewer => reviewer.Vote > 0) && !IsWaitingForAuthor && !IsRejected;
            }
        }

        private bool IsWaitingForAuthor
        {
            get
            {
                IdentityRefWithVote[] reviewers = _pullRequest.Reviewers;
                if (reviewers == null)
                    return false;

                return reviewers.Any(reviewer => reviewer.Vote == -5);
            }
        }

        private bool IsRejected
        {
            get
            {
                IdentityRefWithVote[] reviewers = _pullRequest.Reviewers;
                if (reviewers == null)
                    return false;

                return _pullRequest.Reviewers.Any(reviewer => reviewer.Vote == -10);
            }
        }

        private bool IsDraft
        {
            get
            {
                if (_pullRequest.IsDraft == null)
                    return false;

                return (bool)_pullRequest.IsDraft;
            }
        
        }

        public int Id => _pullRequest.PullRequestId;
        public string Title
        {
            get
            {
                string postfix = "";
                if (IsDraft)
                    postfix += " [Draft]";
                if (IsRejected)
                    postfix += " [Rejected]";
                if (IsWaitingForAuthor)
                    postfix += " [Waiting for author]";

                return _pullRequest.Title + postfix;
            }
        }

        public string AuthorDisplayName => _pullRequest.CreatedBy.DisplayName;
        public DateTime Created => _pullRequest.CreationDate;

        public DateTime? Completed
        {
            get
            {
                if (_pullRequest.Status != PullRequestStatus.Completed)
                {
                    return null;
                }

                return _pullRequest.ClosedDate;
            }
        }

        public Uri WebViewUri => new Uri($"{_serverUri}{Repository.Project.Name}/_git/{Repository.Name}/pullrequest/{Id:d}");
    }
}