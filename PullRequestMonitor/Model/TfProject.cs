using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.TeamFoundation.Core.WebApi;
using PullRequestMonitor.Exceptions;
using PullRequestMonitor.Services;

namespace PullRequestMonitor.Model
{
    /// <summary>
    /// Represents a TFS project.
    /// </summary>
    public interface ITfProject
    {
        Guid Id { get; }
        string Name { get; }
        Task RetrievePullRequests();
        RetrievalStatus PullRequestRetrievalStatus { get; }
        ConcurrentDictionary<int, IPullRequest> Approved { get; }
        ConcurrentDictionary<int, IPullRequest> Unapproved { get; }
        int ApprovedPullRequestCount { get; }
        int UnapprovedPullRequestCount { get; }
        Task RetrieveRepositories();
        RetrievalStatus RepositoryRetrievalStatus { get; }
        IEnumerable<ITfGitRepository> Repositories { get; }
        IRepositoryFilter RepositoryFilter { get; set; }
        event EventHandler RepositoriesUpdated;
    }

    internal sealed class TfProject : ITfProject
    {
        private readonly TeamProjectReference _projectReference;
        private readonly ITfsConnection _tfsConnection;
        private readonly ILogger _logger;
        private readonly List<ITfGitRepository> _repositories;
        private IRepositoryFilter _repositoryFilter;

        public TfProject(TeamProjectReference projectReference, ITfsConnection tfsConnection, ILogger logger)
        {
            _projectReference = projectReference;
            _tfsConnection = tfsConnection;
            _logger = logger;

            Approved = new ConcurrentDictionary<int, IPullRequest>();
            Unapproved = new ConcurrentDictionary<int, IPullRequest>();
            _repositories = new List<ITfGitRepository>();
        }

        public Guid Id => _projectReference.Id;
        public string Name => _projectReference.Name;

        public Task RetrievePullRequests()
        {
            var doPrRetrievalTask = new Task(DoPullRequestRetrieval);
            doPrRetrievalTask.Start();
            return doPrRetrievalTask;
        }

        public RetrievalStatus PullRequestRetrievalStatus { get; private set; }
        public ConcurrentDictionary<int, IPullRequest> Approved { get; }
        public ConcurrentDictionary<int, IPullRequest> Unapproved { get; }
        public int ApprovedPullRequestCount => Approved.Count;
        public int UnapprovedPullRequestCount => Unapproved.Count;

        public Task RetrieveRepositories()
        {
            var doRepoRetrievalTask = new Task(DoRepositoryRetrieval);
            doRepoRetrievalTask.Start();
            return doRepoRetrievalTask;
        }

        public RetrievalStatus RepositoryRetrievalStatus { get; private set; }

        public IEnumerable<ITfGitRepository> Repositories => _repositories.Where(IncludeRepo);

        public IRepositoryFilter RepositoryFilter
        {
            get => _repositoryFilter;
            set
            {
                if (value == _repositoryFilter) return;

                _repositoryFilter = value;
                OnRepositoriesUpdated();
            }
        }

        public event EventHandler RepositoriesUpdated;

        private bool IncludeRepo(ITfGitRepository repo)
        {
            return RepositoryFilter == null || RepositoryFilter.IncludesRepo(repo);
        }

        private void OnRepositoriesUpdated()
        {
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() => { RepositoriesUpdated?.Invoke(this, EventArgs.Empty); });
            }
        }

        public void DoPullRequestRetrieval()
        {
            if (PullRequestRetrievalStatus == RetrievalStatus.Ongoing) return;

            _logger.Info(
                $"{nameof(TfProject)}: beginning retrieval of pull requests in {Name} ({_projectReference.Url})...");

            PullRequestRetrievalStatus = RetrievalStatus.Ongoing;
            var exceptions = new List<Exception>();
            try
            {
                var getPRsTask = _tfsConnection.GetActivePullRequestsInProject(this);

                getPRsTask.Wait();

                var activePullRequestIds = EnsureActivePrsArePresent(getPRsTask.Result);
                RemoveStalePullRequests(activePullRequestIds);
            }
            catch (AggregateException ae)
            {
                exceptions.AddRange(ae.Flatten().InnerExceptions);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }

            if (exceptions.Any(ExceptionClassifiers.IsConnectivityException))
            {
                PullRequestRetrievalStatus = RetrievalStatus.FailedDueToConnection;
                _logger.Info(
                    $"{nameof(TfProject)}: refreshing pull requests in {Name} ({_projectReference.Url}) failed because the server cannot be contacted.");
            }
            else if (exceptions.Any(ExceptionClassifiers.IsAuthorisationException))
            {
                PullRequestRetrievalStatus = RetrievalStatus.FailedDueToAuth;
                _logger.Info(
                    $"{nameof(TfProject)}: refreshing pull requests in {Name} ({_projectReference.Url}) failed due to an authorisation problem.");
            }
            else if (exceptions.Any())
            {
                PullRequestRetrievalStatus = RetrievalStatus.FailedReasonUnknown;
                _logger.Error(
                    $"{nameof(TfProject)}: refreshing pull requests in {Name} ({_projectReference.Url}) failed due to one or more unrecognised exceptions:.");
                var i = 0;
                foreach (var exception in exceptions)
                {
                    _logger.Error($"\tupdate exception {i}:", exception);
                    i++;
                }
            }
            else
            {
                PullRequestRetrievalStatus = RetrievalStatus.Suceeded;
                _logger.Info(
                    $"{nameof(TfProject)}: pull requests in {Name} ({_projectReference.Url}) successfully refreshed;" +
                    $" count is {UnapprovedPullRequestCount + ApprovedPullRequestCount}");
            }
        }

        /// <summary>
        /// Adds any of the supplied <c>IPullRequest</c> in <paramref name="activePullRequests"/> in repositories
        /// matching the current repository filter to the <c>Approved</c> or <c>Unapproved</c> dictionaries.
        /// </summary>
        /// <param name="activePullRequests">The currently active pull requests in the project based on the most
        /// recent update from the server.</param>
        /// <returns>The subset of <paramref name="activePullRequests"/> which were added to one of the dictionaries.</returns>
        public IEnumerable<int> EnsureActivePrsArePresent(IEnumerable<IPullRequest> activePullRequests)
        {
            var activePullRequestIds = new List<int>();
            foreach (var pullRequest in activePullRequests.Where(pr => IncludeRepo(pr.Repository)))
            {
                if (pullRequest.IsApproved)
                {
                    Approved.AddOrUpdate(pullRequest.Id, pullRequest, (i, existingValue) =>
                    {
                        _tfsConnection.ReleasePullRequest(existingValue);
                        return pullRequest;
                    });
                    if (Unapproved.ContainsKey(pullRequest.Id))
                    {
                        IPullRequest removed;
                        Unapproved.TryRemove(pullRequest.Id, out removed);
                    }
                }
                else
                {
                    Unapproved.AddOrUpdate(pullRequest.Id, pullRequest, (i, existingValue) =>
                    {
                        _tfsConnection.ReleasePullRequest(existingValue);
                        return pullRequest;
                    });
                    if (Approved.ContainsKey(pullRequest.Id))
                    {
                        IPullRequest removed;
                        Approved.TryRemove(pullRequest.Id, out removed);
                    }
                }

                activePullRequestIds.Add(pullRequest.Id);
            }

            return activePullRequestIds;
        }

        /// <summary>
        /// Removes pull requests not in the <param name="visiblePullRequests"/>
        /// from the <c>Approved</c> and <c>Unapproved</c> dictionaries and releases
        /// them. 
        /// </summary>
        /// <param name="visiblePullRequests">The pull requests which should be visible
        /// according to what was last retrieved from the server and the current
        /// repository filter.</param>
        public void RemoveStalePullRequests(IEnumerable<int> visiblePullRequests)
        {
            var toRemove = new List<int>();
            foreach (var pullRequestId in Approved.Keys.Concat(Unapproved.Keys))
            {
                if (!visiblePullRequests.Contains(pullRequestId))
                    toRemove.Add(pullRequestId);
            }

            foreach (var pullRequestId in toRemove)
            {
                IPullRequest removed;
                Unapproved.TryRemove(pullRequestId, out removed);
                if (removed != null)
                {
                    _tfsConnection.ReleasePullRequest(removed);
                }
                Approved.TryRemove(pullRequestId, out removed);
                if (removed != null)
                {
                    _tfsConnection.ReleasePullRequest(removed);
                }
            }
        }

        private void DoRepositoryRetrieval()
        {
            if (RepositoryRetrievalStatus == RetrievalStatus.Ongoing ||
                RepositoryRetrievalStatus == RetrievalStatus.Suceeded) return;

            _logger.Info(
                $"{nameof(TfProject)}: beginning update of repositories in {Name} ({_projectReference.Url})...");

            RepositoryRetrievalStatus = RetrievalStatus.Ongoing;
            var exceptions = new List<Exception>();
            try
            {
                var getReposTask = _tfsConnection.GetRepositoriesInProject(this);
                getReposTask.Wait();

                _repositories.Clear();
                _repositories.AddRange(getReposTask.Result);

                RepositoryRetrievalStatus = RetrievalStatus.Suceeded;
            }
            catch (AggregateException ae)
            {
                exceptions.AddRange(ae.Flatten().InnerExceptions);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }

            if (exceptions.Any(ExceptionClassifiers.IsConnectivityException))
            {
                RepositoryRetrievalStatus = RetrievalStatus.FailedDueToConnection;
                _logger.Info(
                    $"{nameof(TfProject)}: retrieving repositories in {Name} ({_projectReference.Url}) failed because the server cannot be contacted.");
            }
            else if (exceptions.Any(ExceptionClassifiers.IsAuthorisationException))
            {
                RepositoryRetrievalStatus = RetrievalStatus.FailedDueToAuth;
                _logger.Info(
                    $"{nameof(TfProject)}: retrieving repositories in {Name} ({_projectReference.Url}) failed due to an authorisation problem.");
            }
            else if (exceptions.Any())
            {
                RepositoryRetrievalStatus = RetrievalStatus.FailedReasonUnknown;
                _logger.Error(
                    $"{nameof(TfProject)}: retrieving respositories in {Name} ({_projectReference.Url}) failed due to one or more unrecognised exceptions:.");
                var i = 0;
                foreach (var exception in exceptions)
                {
                    _logger.Error($"\tupdate exception {i}:", exception);
                    i++;
                }
            }
            else
            {
                RepositoryRetrievalStatus = RetrievalStatus.Suceeded;
                _logger.Info(
                    $"{nameof(TfProject)}: repositories in {Name} ({_projectReference.Url}) successfully retrieved;" +
                    $" count is {UnapprovedPullRequestCount + ApprovedPullRequestCount}");
            }

            OnRepositoriesUpdated();
        }
    }
}