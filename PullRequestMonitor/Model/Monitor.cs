using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Castle.Core.Internal;
using PullRequestMonitor.Factories;
using PullRequestMonitor.Services;

namespace PullRequestMonitor.Model
{
    public enum MonitorStatus
    {
        /// <summary>
        /// The first update has not completed yet.
        /// </summary>
        AwaitingFirstUpdate,
        /// <summary>
        /// The settings for the monitor have no configured projects.
        /// </summary>
        NoProjects,
        /// <summary>
        /// The last update was succesful.
        /// </summary>
        UpdateSuccessful,
        /// <summary>
        /// At the time of the last update, the monitor could not reach the TFS server.
        /// </summary>
        CouldNotReachServer,
        /// <summary>
        /// Pull request monitor was not authorised to make the most recent request to the server.
        /// </summary>
        AuthorisationError,
        /// <summary>
        /// Something went wrong during the last update. The monitor will continue trying to update.
        /// </summary>
        UnrecognisedError
    }

    public interface IMonitor
    {
        IEnumerable<ITfProject> Projects { get; }
        int UnapprovedPullRequestCount { get; }
        int ApprovedPullRequestCount { get;  }
        event EventHandler UpdateCompleted;
        /// <summary>
        /// Reflects the status of the monitor.
        /// </summary>
        MonitorStatus Status { get; }
        void Start();
    }

    internal sealed class Monitor : IMonitor
    {
        private readonly IMonitorSettings _settings;
        private readonly ITfProjectCollectionCache _tfProjectCollectionCache;
        private readonly INameRegexpRepositoryFilterFactory _nameRegexpRepositoryFilterFactory;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<Guid, ITfProject> _projects;
        private Timer _timer;
        private uint _updatesStartedCount;
        private uint _updatesCompletedCount;
        private readonly Mutex _updateCountersMutex;

        public Monitor(IMonitorSettings settings, ITfProjectCollectionCache tfProjectCollectionCache, INameRegexpRepositoryFilterFactory nameRegexpRepositoryFilterFactory, ILogger logger)
        {
            _settings = settings;
            _settings.SettingsChanged += SettingsChangedHandler;
            _tfProjectCollectionCache = tfProjectCollectionCache;
            _nameRegexpRepositoryFilterFactory = nameRegexpRepositoryFilterFactory;
            _logger = logger;
            _projects = new ConcurrentDictionary<Guid, ITfProject>();
            Status = _settings.Projects.IsNullOrEmpty() ? MonitorStatus.NoProjects : MonitorStatus.AwaitingFirstUpdate;
            _updatesStartedCount = _updatesCompletedCount = 0;
            _updateCountersMutex = new Mutex();
        }

        public void Start()
        {
            if (!_settings.Projects.Any())
            {
                _logger.Info($"{nameof(Monitor)}: not starting monitor as zero projects are configured.");
                return; // Prevent log pollution due to zero-project updates
            }

            _logger.Info($"{nameof(Monitor)}: starting monitor.");
            _timer = new Timer(Update, _settings.Projects, 0, _settings.PollInterval);
        }

        public void Stop()
        {
            if (_timer != null)
            {
                _logger.Info($"{nameof(Monitor)}: stopping monitor.");
                _timer.Dispose();
            }
        }

        private void SettingsChangedHandler(object sender, EventArgs eventArgs)
        {
            _logger.Info($"{nameof(Monitor)}: settings changed; restarting...");
            Stop();
            Start();
        }

        public async Task<MonitorStatus> SyncMonitoredProjects(IEnumerable<MonitoredProjectSettings> monitoredProjectSettings)
        {
            foreach (var projectSettings in monitoredProjectSettings)
            {
                var projectCollection = _tfProjectCollectionCache.GetProjectCollection(projectSettings.VstsAccount);

                if (projectCollection.ProjectRetrievalStatus != RetrievalStatus.Suceeded)
                {
                    await projectCollection.RetrieveProjects();
                }

                switch (projectCollection.ProjectRetrievalStatus)
                {
                    case RetrievalStatus.FailedDueToConnection:
                        return MonitorStatus.CouldNotReachServer;
                    case RetrievalStatus.FailedDueToAuth:
                        return MonitorStatus.AuthorisationError;
                    case RetrievalStatus.FailedReasonUnknown:
                        return MonitorStatus.UnrecognisedError;
                }

                if (!_projects.ContainsKey(projectSettings.Id))
                {
                    _projects[projectSettings.Id] = projectCollection.GetProject(projectSettings.Id);
                }

                if (!string.IsNullOrEmpty(projectSettings.RepoNameRegexp))
                {
                    _projects[projectSettings.Id].RepositoryFilter = _nameRegexpRepositoryFilterFactory.Create(projectSettings.RepoNameRegexp);
                }
            }

            foreach (var removedProject in _projects.Keys.Where(id => monitoredProjectSettings.Count(s => s.Id == id) == 0))
            {
                ITfProject project;
                _projects.TryRemove(removedProject, out project);
            }

            return MonitorStatus.UpdateSuccessful;
        }

        public IEnumerable<ITfProject> Projects => _projects.Values;
        public int UnapprovedPullRequestCount => _projects.Values.Aggregate(0, (i, project) => i + project.UnapprovedPullRequestCount);
        public int ApprovedPullRequestCount => _projects.Values.Aggregate(0, (i, project) => i + project.ApprovedPullRequestCount);
        public event EventHandler UpdateCompleted;
        public MonitorStatus Status { get; private set; }

        public async void Update(object stateInfo)
        {
            var updateStarted = TryBeginUpdate(out var currentUpdateNumber);

            if (!updateStarted)
            {
                _logger.Info($"{nameof(Monitor)}: postponing update #{currentUpdateNumber + 1} as update {currentUpdateNumber} is still running.");
                return;
            }

            var projects = stateInfo as IEnumerable<MonitoredProjectSettings>;
            _logger.Info($"{nameof(Monitor)}: beginning update #{currentUpdateNumber}...");
            var updateStartTime = DateTime.Now;

            Status = await DoUpdate(projects);

            var updateDuration = DateTime.Now.Subtract(updateStartTime);
            var updateDurationString = $"{updateDuration.Seconds:D2}.{updateDuration.Milliseconds:D3} s";

            if (Status == MonitorStatus.CouldNotReachServer)
            {
                _logger.Info(
                    $"{nameof(Monitor)}: update #{currentUpdateNumber} failed in {updateDurationString} because the server hosting one or more projects could not be contacted");
            } else if (Status == MonitorStatus.AuthorisationError)
            {
                _logger.Info(
                    $"{nameof(Monitor)}: update #{currentUpdateNumber} failed in {updateDurationString} due to an authorisation problem with one or more projects");
            } else if (Status == MonitorStatus.UnrecognisedError)
            {
                _logger.Error(
                    $"{nameof(Monitor)}: update #{currentUpdateNumber} failed in {updateDurationString} due to one or more exceptions (logged above).");
            } else if (Status == MonitorStatus.NoProjects)
            {
                _logger.Warn(
                    $"{nameof(Monitor)}: update #{currentUpdateNumber} should not have been started as no projects are configured");
            } else if (Status == MonitorStatus.UpdateSuccessful) {
                _logger.Info(
                    $"{nameof(Monitor)}: update #{currentUpdateNumber} completed successfully in {updateDuration.Seconds}.{updateDuration.Milliseconds:D3}" +
                    $" s; pull request count is {ApprovedPullRequestCount + UnapprovedPullRequestCount}.");
            }

            CompleteUpdate();
            OnUpdateCompleted();
        }

        public async Task<MonitorStatus> DoUpdate(IEnumerable<MonitoredProjectSettings> monitoredProjects)
        {
            var projectSyncOutcome = await SyncMonitoredProjects(monitoredProjects);
            if (projectSyncOutcome != MonitorStatus.UpdateSuccessful)
            {
                return projectSyncOutcome;
            }

            if (!Projects.Any()) return MonitorStatus.NoProjects;

            Task[] taskArray = new Task[Projects.Count()];
            for (int i = 0; i < Projects.Count(); i++)
            {
                taskArray[i] = Projects.ElementAt(i).RetrievePullRequests();
            }
            Task.WaitAll(taskArray);

            if (Projects.Any(proj => proj.PullRequestRetrievalStatus == RetrievalStatus.FailedDueToConnection))
                return MonitorStatus.CouldNotReachServer;

            if (Projects.Any(proj => proj.PullRequestRetrievalStatus == RetrievalStatus.FailedDueToAuth))
                return MonitorStatus.AuthorisationError;

            if (Projects.Any(proj => proj.PullRequestRetrievalStatus == RetrievalStatus.FailedReasonUnknown) ||
                Projects.Any(proj => proj.PullRequestRetrievalStatus == RetrievalStatus.Unstarted))
                return MonitorStatus.UnrecognisedError;

            return MonitorStatus.UpdateSuccessful;
        }

        /// <summary>
        /// Try to begin a new update.
        /// </summary>
        /// <param name="currentUpdateNumber">Will be set to the current update number.</param>
        /// <returns>False if a previous update is still running otherwise true.</returns>
        private bool TryBeginUpdate(out uint currentUpdateNumber)
        {
            _updateCountersMutex.WaitOne();

            var succeeded = _updatesCompletedCount == _updatesStartedCount;
            if (succeeded) _updatesStartedCount++;
            currentUpdateNumber = _updatesStartedCount;

            _updateCountersMutex.ReleaseMutex();
            return succeeded;
        }

        /// <summary>
        /// Complete the current running update.
        /// </summary>
        private void CompleteUpdate()
        {
            _updateCountersMutex.WaitOne();

            _updatesCompletedCount = _updatesStartedCount;

            _updateCountersMutex.ReleaseMutex();
        }

        private void OnUpdateCompleted()
        {
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() => { UpdateCompleted?.Invoke(this, EventArgs.Empty); });
            }
        }
    }
}