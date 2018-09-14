using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using PullRequestMonitor.Factories;
using PullRequestMonitor.Model;

namespace PullRequestMonitor
{
    public sealed class TfsConnection : ITfsConnection
    {
        private readonly VssConnection _serverConnection;
        private readonly IPullRequestFactory _pullRequestFactory;
        private readonly ITfProjectFactory _tfProjectFactory;
        private readonly ITfGitRepositoryFactory _repositoryFactory;
        private readonly Lazy<GitHttpClient> _gitHttpClient;
        private readonly Lazy<ProjectHttpClient> _projectHttpClient;

        private static readonly GitPullRequestSearchCriteria ActivePullRequestSearchCriteria = new GitPullRequestSearchCriteria
        {
            IncludeLinks = true,
            Status = PullRequestStatus.Active
        };

        private static readonly GitPullRequestSearchCriteria CompletedPullRequestSearchCriteria = new GitPullRequestSearchCriteria
        {
            IncludeLinks = true,
            Status = PullRequestStatus.Completed
        };

        public TfsConnection(VssConnection serverConnection, IPullRequestFactory pullRequestFactory, ITfProjectFactory tfProjectFactory, ITfGitRepositoryFactory repositoryFactory)
        {
            _serverConnection = serverConnection;
            _pullRequestFactory = pullRequestFactory;
            _tfProjectFactory = tfProjectFactory;
            _repositoryFactory = repositoryFactory;

            _gitHttpClient = new Lazy<GitHttpClient>(_serverConnection.GetClient<GitHttpClient>);
            _projectHttpClient = new Lazy<ProjectHttpClient>(_serverConnection.GetClient<ProjectHttpClient>);
        }

        public Task<IEnumerable<ITfProject>> GetProjects()
        {
            var task = Task.Run(() =>
            {
                var getProjectsTask = _projectHttpClient.Value.GetProjects();

                getProjectsTask.Wait();

                return getProjectsTask.Result.Select(proj => _tfProjectFactory.Create(proj, this));
            });

            return task;
        }

        public Task<IEnumerable<ITfGitRepository>> GetRepositoriesInProject(ITfProject project)
        {
            var task = Task.Run(() =>
            {
                var getRepositoriesTask = _gitHttpClient.Value.GetRepositoriesAsync(project.Id);

                getRepositoriesTask.Wait();

                return getRepositoriesTask.Result.Select(repo => _repositoryFactory.Create(repo, project));
            });

            return task;
        }

        public Task<IEnumerable<IPullRequest>> GetActivePullRequestsInProject(ITfProject project)
        {
            return PullRequestsInProject(project, ActivePullRequestSearchCriteria);
        }

        public Task<IEnumerable<IPullRequest>> GetCompletedPullRequestsInProject(ITfProject project)
        {
            return PullRequestsInProject(project, CompletedPullRequestSearchCriteria);
           // IOrderedEnumerable<IPullRequest> thing2 = thing.Result.OrderByDescending(x => x.Created.Date);
           //return Task.FromResult((IEnumerable<IPullRequest>) thing2);
        }

        private Task<IEnumerable<IPullRequest>> PullRequestsInProject(ITfProject project, GitPullRequestSearchCriteria pullRequestSearchCriteria)
        {
            var task = Task.Run(() =>
            {
                var getPullRequestsTask = _gitHttpClient.Value.GetPullRequestsByProjectAsync(project.Id,
                    pullRequestSearchCriteria);

                getPullRequestsTask.Wait();

                var result = new List<IPullRequest>();
                foreach (var pullRequest in getPullRequestsTask.Result)
                {
                    var repo = project.Repositories.FirstOrDefault(rep => rep.Id == pullRequest.Repository.Id) ??
                               _repositoryFactory.Create(pullRequest.Repository, project);

                    result.Add(_pullRequestFactory.Create(pullRequest, _serverConnection.Uri.AbsoluteUri, repo));
                }

                return result.AsEnumerable();
            });

            return task;
        }

        public void ReleasePullRequest(IPullRequest pullRequest)
        {
            _pullRequestFactory.Release(pullRequest);
        }
    }
}