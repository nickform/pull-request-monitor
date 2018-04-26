using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Factories;
using PullRequestMonitor.Model;
using PullRequestMonitor.Services;
using Monitor = PullRequestMonitor.Model.Monitor;

namespace PullRequestMonitor.UnitTest.Model
{
    [TestFixture]
    public class MonitorTest
    {
        private static readonly int[] PullRequestCounts = { 0, 1, 7, 66 };

        private const int UpdateTimeoutSeconds = 1;
        private IMonitorSettings _monitorSettings;
        private ITfProjectCollectionCache _tfProjectCollectionCache;
        private MonitoredProjectSettings _aMonitoredProjectSettings;
        private INameRegexpRepositoryFilterFactory _nameRegexpRepositoryFilterFactory;
        private ILogger _logger;

        [SetUp]
        public void SetUp()
        {
            _monitorSettings = Substitute.For<IMonitorSettings>();
            _tfProjectCollectionCache = Substitute.For<ITfProjectCollectionCache>();
            _aMonitoredProjectSettings =
                new MonitoredProjectSettings {Id = Guid.NewGuid(), VstsAccount = "b"};
            _nameRegexpRepositoryFilterFactory = Substitute.For<INameRegexpRepositoryFilterFactory>();
            _logger = Substitute.For<ILogger>();
        }

        [Test]
        public void Projects_ForDefaultInstance_IsNotNull()
        {
            var monitor = new Monitor(_monitorSettings, _tfProjectCollectionCache, _nameRegexpRepositoryFilterFactory, _logger);

            Assert.That(monitor.Projects, Is.Not.Null);
        }

        [Test]
        public void Projects_WhenSettingsHasNoProjects_IsEmpty()
        {
            var monitor = new Monitor(_monitorSettings, _tfProjectCollectionCache, _nameRegexpRepositoryFilterFactory, _logger);

            Assert.That(monitor.Projects, Is.Empty);
        }

        [Test]
        public void Projects_WhenSettingsHasOneProject_HasOneProject()
        {
            var monitoredProjectSettingses = new[] {_aMonitoredProjectSettings};
            _monitorSettings.Projects.Returns(monitoredProjectSettingses);
            var monitor = new Monitor(_monitorSettings, _tfProjectCollectionCache, _nameRegexpRepositoryFilterFactory, _logger);
            monitor.SyncMonitoredProjects(monitoredProjectSettingses);

            Assert.That(monitor.Projects.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task DoUpdate_CallsProjectsUpdate()
        {
            var projectSettingses = new[] { _aMonitoredProjectSettings };
            _monitorSettings.Projects.Returns(projectSettingses);
            var tfProject = Substitute.For<ITfProject>();
            var tfsServer = Substitute.For<ITfProjectCollection>();
            tfsServer.GetProject(_aMonitoredProjectSettings.Id).Returns(tfProject);
            _tfProjectCollectionCache.GetProjectCollection(Arg.Any<string>()).Returns(tfsServer);
            var systemUnderTest = new Monitor(_monitorSettings, _tfProjectCollectionCache, _nameRegexpRepositoryFilterFactory, _logger);

            await systemUnderTest.DoUpdate(projectSettingses);

#pragma warning disable 4014
            tfProject.Received().RetrievePullRequests();
#pragma warning restore 4014
        }

        [Test]
        public void SyncToSettings_ForEachProjectIdentifier_GetsProjectCollectionFromCache()
        {
            var projectSettingses = new[]
            {
                new MonitoredProjectSettings { VstsAccount = "1st server a/c", Id = Guid.NewGuid() },
                new MonitoredProjectSettings { VstsAccount = "2nd server a/c", Id = Guid.NewGuid() },
                new MonitoredProjectSettings { VstsAccount = "3rd server a/c", Id = Guid.NewGuid() },
            };
            _monitorSettings.Projects.Returns(projectSettingses);
            var tfsServer = Substitute.For<ITfProjectCollection>();
            tfsServer.GetProject(Arg.Any<Guid>()).Returns(Substitute.For<ITfProject>());
            _tfProjectCollectionCache.GetProjectCollection(Arg.Any<string>()).Returns(tfsServer);
            var systemUnderTest = new Monitor(_monitorSettings, _tfProjectCollectionCache, _nameRegexpRepositoryFilterFactory, _logger);

            systemUnderTest.SyncMonitoredProjects(projectSettingses);

            foreach (var projectIdentifier in projectSettingses)
            {
                _tfProjectCollectionCache.Received().GetProjectCollection(projectIdentifier.VstsAccount);
            }
        }

        [Test]
        public void SyncToSettings_ForEachProjectIdentifier_GetsProjectFromProjectCollection()
        {
            var projectSettingses = new[]
            {
                new MonitoredProjectSettings { VstsAccount = "1st server url", Id = Guid.NewGuid() },
                new MonitoredProjectSettings { VstsAccount = "2nd server url", Id = Guid.NewGuid() },
                new MonitoredProjectSettings { VstsAccount = "3rd server url", Id = Guid.NewGuid() },
            };
            _monitorSettings.Projects.Returns(projectSettingses);
            var tfsServer = Substitute.For<ITfProjectCollection>();
            tfsServer.GetProject(Arg.Any<Guid>()).Returns(Substitute.For<ITfProject>());
            _tfProjectCollectionCache.GetProjectCollection(Arg.Any<string>()).Returns(tfsServer);
            var systemUnderTest = new Monitor(_monitorSettings, _tfProjectCollectionCache, _nameRegexpRepositoryFilterFactory, _logger);

            systemUnderTest.SyncMonitoredProjects(projectSettingses);

            foreach (var projectIdentifier in projectSettingses)
            {
                tfsServer.Received().GetProject(projectIdentifier.Id);
            }
        }

        [Test]
        public void SyncToSettings_ForEachProjectIdentifierWithNonEmptyRepoNameRegexp_CreatesNameRegexpRepoFilter()
        {
            var projectSettingses = new[]
            {
                new MonitoredProjectSettings { VstsAccount = "1st server url", Id = Guid.NewGuid() },
                new MonitoredProjectSettings { VstsAccount = "2nd server url", Id = Guid.NewGuid(), RepoNameRegexp = "*"},
                new MonitoredProjectSettings { VstsAccount = "3rd server url", Id = Guid.NewGuid(), RepoNameRegexp = "lasdf"},
            };
            _monitorSettings.Projects.Returns(projectSettingses);
            var tfsServer = Substitute.For<ITfProjectCollection>();
            tfsServer.GetProject(Arg.Any<Guid>()).Returns(Substitute.For<ITfProject>());
            _tfProjectCollectionCache.GetProjectCollection(Arg.Any<string>()).Returns(tfsServer);
            _nameRegexpRepositoryFilterFactory.Create(Arg.Any<string>()).Returns(new NameRegexpRepositoryFilter("*"));
            var systemUnderTest = new Monitor(_monitorSettings, _tfProjectCollectionCache, _nameRegexpRepositoryFilterFactory, _logger);

            systemUnderTest.SyncMonitoredProjects(projectSettingses);

            Assert.That(_nameRegexpRepositoryFilterFactory.ReceivedCalls().Count(),
                Is.EqualTo(projectSettingses.Count(pi => !string.IsNullOrEmpty(pi.RepoNameRegexp))));
            foreach (var projectIdentifier in projectSettingses.Where(pi => !string.IsNullOrEmpty(pi.RepoNameRegexp)))
            {
                _nameRegexpRepositoryFilterFactory.Received().Create(projectIdentifier.RepoNameRegexp);
            }
        }

        [Test]
        public void SyncToSettings_RemovesRemovedProjects()
        {
            var projectSettingses = new[] { _aMonitoredProjectSettings };
            _monitorSettings.Projects.Returns(projectSettingses);
            var tfProject = Substitute.For<ITfProject>();
            var tfsServer = Substitute.For<ITfProjectCollection>();
            tfsServer.GetProject(_aMonitoredProjectSettings.Id).Returns(tfProject);
            _tfProjectCollectionCache.GetProjectCollection(Arg.Any<string>()).Returns(tfsServer);
            var systemUnderTest = new Monitor(_monitorSettings, _tfProjectCollectionCache, _nameRegexpRepositoryFilterFactory, _logger);
            systemUnderTest.SyncMonitoredProjects(projectSettingses);
            Assert.That(systemUnderTest.Projects, Contains.Item(tfProject));

            systemUnderTest.SyncMonitoredProjects(Enumerable.Empty<MonitoredProjectSettings>());

            Assert.That(systemUnderTest.Projects, Does.Not.Contains(tfProject));
        }

        [Test, TestCaseSource(nameof(PullRequestCounts))]
        public void UnapprovedPullRequestCount_ReturnsNumberOfUnapprovedPullRequests(int pullRequestCount)
        {
            var projectSettingses = new[] { _aMonitoredProjectSettings };
            _monitorSettings.Projects.Returns(projectSettingses);
            var tfProject = Substitute.For<ITfProject>();
            tfProject.UnapprovedPullRequestCount.Returns(pullRequestCount);
            var tfsServer = Substitute.For<ITfProjectCollection>();
            tfsServer.GetProject(_aMonitoredProjectSettings.Id).Returns(tfProject);
            _tfProjectCollectionCache.GetProjectCollection(Arg.Any<string>()).Returns(tfsServer);
            var systemUnderTest = new Monitor(_monitorSettings, _tfProjectCollectionCache, _nameRegexpRepositoryFilterFactory, _logger);

            systemUnderTest.SyncMonitoredProjects(projectSettingses);

            Assert.That(systemUnderTest.UnapprovedPullRequestCount, Is.EqualTo(pullRequestCount));
        }

        [Test, TestCaseSource(nameof(PullRequestCounts))]
        public void ApprovedPullRequestCount_ReturnsNumberOfApprovedPullRequests(int pullRequestCount)
        {
            var projectSettingses = new[] { _aMonitoredProjectSettings };
            _monitorSettings.Projects.Returns(projectSettingses);
            var tfProject = Substitute.For<ITfProject>();
            tfProject.ApprovedPullRequestCount.Returns(pullRequestCount);
            var tfsServer = Substitute.For<ITfProjectCollection>();
            tfsServer.GetProject(_aMonitoredProjectSettings.Id).Returns(tfProject);
            _tfProjectCollectionCache.GetProjectCollection(Arg.Any<string>()).Returns(tfsServer);
            var systemUnderTest = new Monitor(_monitorSettings, _tfProjectCollectionCache, _nameRegexpRepositoryFilterFactory, _logger);

            systemUnderTest.SyncMonitoredProjects(projectSettingses);

            Assert.That(systemUnderTest.ApprovedPullRequestCount, Is.EqualTo(pullRequestCount));
        }

        [Test]
        public void TestStatus_BeforeFirstUpdate_WhenThereAreProjects_IsAwaitingFirstUpdate()
        {
            var tfProject = Substitute.For<ITfProject>();
            _monitorSettings.Projects.Returns(new[] { new MonitoredProjectSettings() });
            var tfsServer = Substitute.For<ITfProjectCollection>();
            tfsServer.GetProject(Arg.Any<Guid>()).Returns(tfProject);
            _tfProjectCollectionCache.GetProjectCollection(_aMonitoredProjectSettings.VstsAccount).Returns(tfsServer);
            var systemUnderTest = new Monitor(_monitorSettings, _tfProjectCollectionCache, _nameRegexpRepositoryFilterFactory, _logger);

            Assert.That(systemUnderTest.Status, Is.EqualTo(MonitorStatus.AwaitingFirstUpdate));
        }

        [Test]
        public void TestStatus_BeforeFirstUpdate_WhenThereAreNoProjects_IsNoProjects()
        {
            var systemUnderTest = new Monitor(_monitorSettings, _tfProjectCollectionCache, _nameRegexpRepositoryFilterFactory, _logger);

            Assert.That(systemUnderTest.Status, Is.EqualTo(MonitorStatus.NoProjects));
        }
    }
}
