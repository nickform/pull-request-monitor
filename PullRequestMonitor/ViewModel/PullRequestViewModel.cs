using System;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.ViewModel
{
    public sealed class PullRequestViewModel
    {
        private readonly IPullRequest _model;

        public PullRequestViewModel(IPullRequest model)
        {
            _model = model;
        }

        public int Id => _model.Id;
        public string Title => _model.Title;
        public string Author => _model.AuthorDisplayName;
        public string Repository => _model.Repository.Name;
        public Uri WebViewUri => _model.WebViewUri;
        public DateTime Created => _model.Created;
        public DateTime? Completed => _model.Completed;
        public bool IsCompleted => _model.Completed != null;
    }
}