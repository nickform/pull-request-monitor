using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PullRequestMonitor.Exceptions;
using PullRequestMonitor.Factories;
using PullRequestMonitor.Services;

namespace PullRequestMonitor.Model
{
    public interface ITfProjectCollection
    {
        RetrievalStatus ProjectRetrievalStatus { get; }
        Task RetrieveProjects();
        IEnumerable<ITfProject> Projects { get;}
        event EventHandler ProjectRetrievalCompleted;
        ITfProject GetProject(Guid projectId);
    }

    public sealed class TfProjectCollection : ITfProjectCollection
    {
        private readonly string _uri;
        private readonly ITfsConnectionFactory _connectionFactory;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<Guid, ITfProject> _projects;

        public RetrievalStatus ProjectRetrievalStatus { get; private set; }
        public IEnumerable<ITfProject> Projects => _projects.Values;
        public event EventHandler ProjectRetrievalCompleted;

        public TfProjectCollection(string uri, ITfsConnectionFactory connectionFactory, ILogger logger)
        {
            _uri = uri;
            _connectionFactory = connectionFactory;
            _logger = logger;

            _projects = new ConcurrentDictionary<Guid, ITfProject>();

            ProjectRetrievalStatus = RetrievalStatus.Unstarted;
        }

        public async Task RetrieveProjects()
        {
            if (ProjectRetrievalStatus == RetrievalStatus.Suceeded) return;

            ProjectRetrievalStatus = RetrievalStatus.Ongoing;
            _logger.Info($"{nameof(TfProjectCollection)}: triggering one-time retrieval of projects from {_uri}...");

            var exceptions = new List<Exception>();
            try
            {
                var connection = _connectionFactory.Create(_uri);
                var currentProjectReferences = await connection.GetProjects();
                foreach (var projectReference in currentProjectReferences)
                {
                    _projects[projectReference.Id] = projectReference;
                }

                ProjectRetrievalStatus = RetrievalStatus.Suceeded;
                _logger.Info($"{nameof(TfProjectCollection)}: projects successfully retrieved; project count is {Projects.Count()}.");
            }
            catch (AggregateException ae)
            {
                exceptions.AddRange(ae.Flatten().InnerExceptions);
                ProjectRetrievalStatus = RetrievalStatus.FailedReasonUnknown;
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                ProjectRetrievalStatus = RetrievalStatus.FailedReasonUnknown;
            }


            if (exceptions.Any(ExceptionClassifiers.IsConnectivityException))
            {
                ProjectRetrievalStatus = RetrievalStatus.FailedDueToConnection;
                _logger.Info($"{nameof(TfProjectCollection)}: retrieving projects failed because the server cannot be contacted.");
            } else if (exceptions.Any(ExceptionClassifiers.IsAuthorisationException))
            {
                ProjectRetrievalStatus = RetrievalStatus.FailedDueToAuth;
                _logger.Info($"{nameof(TfProjectCollection)}: retrieving projects failed because of an authentication or authorisation error.");
            }
            else if (exceptions.Any())
            {
                _logger.Error($"{nameof(TfProjectCollection)}: retrieving projects failed due to one or more unrecognised exceptions. The first:", exceptions[0]);
            }

            OnProjectRetrievalCompleted(this, EventArgs.Empty);
        }

        public ITfProject GetProject(Guid projectId)
        {
            if (ProjectRetrievalStatus != RetrievalStatus.Suceeded)
            {
                throw new InvalidOperationException("Cannot get any project until projects have been succesfully retrieved.");
            }

            if (!_projects.ContainsKey(projectId))
            {
                return null;
            }

            return _projects[projectId];
        }

        private void OnProjectRetrievalCompleted(object sender, EventArgs args)
        {
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProjectRetrievalCompleted?.Invoke(this, EventArgs.Empty);
                });
            }
        }
    }
}