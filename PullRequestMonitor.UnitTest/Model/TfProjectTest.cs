using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Core.WebApi;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using PullRequestMonitor.Model;
using PullRequestMonitor.Services;
using PullRequestMonitor.UnitTest.Exceptions;

namespace PullRequestMonitor.UnitTest.Model
{
    [TestFixture]
    public class TfProjectTest
    {
        private ITfsConnection _tfsConnection;
        private ILogger _logger;

        public static readonly object[] PullRequestCountCases =
        {
            0, 1, 2, 66
        };

        public static Exception[] CouldNotReachServerExceptions = CommonExceptionExamples.CouldNotReachServerExceptions;
        public static Exception[] UnauthorisedExceptions = CommonExceptionExamples.UnauthorisedExceptions;
        public static Exception[] UnrecognisedExceptions = CommonExceptionExamples.UnrecognisedExceptions;

        [SetUp]
        public void SetUp()
        {
            _tfsConnection = Substitute.For<ITfsConnection>();
            _logger = Substitute.For<ILogger>();
        }

        [Test]
        public void TestConstructor_SetsPullRequestRetrievalStatusToUnstarted()
        {
            var systemUnderTest = new TfProject(new TeamProjectReference(), Substitute.For<ITfsConnection>(), Substitute.For<ILogger>());

            Assert.That(systemUnderTest.PullRequestRetrievalStatus, Is.EqualTo(RetrievalStatus.Unstarted));
        }

        [Test]
        public void TestConstructor_SetsRepositoriesRetrievalStatusToUnstarted()
        {
            var systemUnderTest = new TfProject(new TeamProjectReference(), Substitute.For<ITfsConnection>(), Substitute.For<ILogger>());

            Assert.That(systemUnderTest.RepositoryRetrievalStatus, Is.EqualTo(RetrievalStatus.Unstarted));
        }

        [Test]
        public void TestID_ReturnsProjectIdFromIdentifierPassedToConstructor()
        {
            var projectId = Guid.NewGuid();
            var systemUnderTest = new TfProject(new TeamProject {Id = projectId}, _tfsConnection, _logger);

            Assert.That(systemUnderTest.Id, Is.EqualTo(projectId));
        }

        [Test]
        public void TestName_ReturnsTeamProjectRefName()
        {
            var expectedName = "What's in a name?";
            var systemUnderTest = new TfProject(new TeamProject { Name = expectedName }, Substitute.For<ITfsConnection>(), _logger);

            Assert.That(systemUnderTest.Name, Is.EqualTo(expectedName));
        }

        [Test]
        public void TestApproved_IsNotNull()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);

            Assert.That(systemUnderTest.Approved, Is.Not.Null);
        }

        [Test]
        public void TestApprovalNeeded_IsNotNull()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);

            Assert.That(systemUnderTest.Unapproved, Is.Not.Null);
        }

        [Test]
        public void TestRepositories_BeforeFirstUpdate_IsNotNull()
        {
            var systemUnderTest = new TfProject(new TeamProjectReference(), _tfsConnection, _logger);

            Assert.That(systemUnderTest.Repositories, Is.Not.Null);
        }

        [Test]
        public void TestRepositories_BeforeFirstUpdate_IsEmpty()
        {
            var systemUnderTest = new TfProject(new TeamProjectReference(), _tfsConnection, _logger);

            Assert.That(systemUnderTest.Repositories, Is.Empty);
        }

        [Test]
        public async Task TestRepositories_AfterUpdatingRepositories_WhenRepoFilterIsNull_ReturnsReposReturnedByServer()
        {
            var projectId = Guid.NewGuid();
            var systemUnderTest = new TfProject(new TeamProject { Id = projectId }, _tfsConnection, _logger);
            var reposFromServer = new List<ITfGitRepository>();
            reposFromServer.Add(Substitute.For<ITfGitRepository>());
            reposFromServer.Add(Substitute.For<ITfGitRepository>());
            reposFromServer.Add(Substitute.For<ITfGitRepository>());
            _tfsConnection.GetRepositoriesInProject(systemUnderTest).Returns(reposFromServer);
            systemUnderTest.RepositoryFilter = null; // For clarity of the test

            await systemUnderTest.RetrieveRepositories();

            foreach (var repo in reposFromServer)
            {
                Assert.That(systemUnderTest.Repositories, Does.Contain(repo));
            }
        }

        [Test]
        public async Task TestRepositories_AfterUpdatingRepositories_WhenRepoFilterIsNotNull_AppliesFilter()
        {
            var projectId = Guid.NewGuid();
            var systemUnderTest = new TfProject(new TeamProject { Id = projectId }, _tfsConnection, _logger);
            var includedRepos = new HashSet<ITfGitRepository>();
            includedRepos.Add(Substitute.For<ITfGitRepository>());
            includedRepos.Add(Substitute.For<ITfGitRepository>());
            includedRepos.Add(Substitute.For<ITfGitRepository>());
            var excludedRepos = new HashSet<ITfGitRepository>();
            excludedRepos.Add(Substitute.For<ITfGitRepository>());
            excludedRepos.Add(Substitute.For<ITfGitRepository>());
            var reposFromServer = new List<ITfGitRepository>();
            reposFromServer.AddRange(includedRepos);
            reposFromServer.AddRange(excludedRepos);
            _tfsConnection.GetRepositoriesInProject(systemUnderTest).Returns(reposFromServer);
            var repoFilter = Substitute.For<IRepositoryFilter>();
            repoFilter.IncludesRepo(Arg.Any<ITfGitRepository>())
                .Returns(callInfo => includedRepos.Contains(callInfo.Arg<ITfGitRepository>()));
            systemUnderTest.RepositoryFilter = repoFilter;
            await systemUnderTest.RetrieveRepositories();

            foreach (var repo in includedRepos)
            {
                Assert.That(systemUnderTest.Repositories.Contains(repo), Is.True);
            }

            foreach (var repo in excludedRepos)
            {
                Assert.That(systemUnderTest.Repositories.Contains(repo), Is.False);
            }
        }

        [Test]
        public async Task TestUpdateRepositories_GetsProjectRepositoriesFromServer()
        {
            var projectId = Guid.NewGuid();
            var systemUnderTest = new TfProject(new TeamProject{Id = projectId}, _tfsConnection, _logger);
            _tfsConnection.GetRepositoriesInProject(systemUnderTest).Returns(Enumerable.Empty<ITfGitRepository>());

            await systemUnderTest.RetrieveRepositories();

            await _tfsConnection.Received().GetRepositoriesInProject(systemUnderTest);
        }

        [Test]
        public void TestEnsureActivePrsArePresent_WhenThereIsANewApprovedPR_WhichMatchesRepoFilter_AddsToApproved()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            Assert.That(systemUnderTest.Approved.Count, Is.Zero);

            var pullRequest = Substitute.For<IPullRequest>();
            pullRequest.IsApproved.Returns(true);
            var activePullRequests = new[] { pullRequest };
            systemUnderTest.RepositoryFilter = null; // <-- null filter means no filtering

            systemUnderTest.EnsureActivePrsArePresent(activePullRequests);

            Assert.That(systemUnderTest.Approved.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestEnsureActivePrsArePresent_WhenThereIsANewApprovedPR_WhichDoesNotMatchRepoFilter_DoesNotAddToApproved()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            Assert.That(systemUnderTest.Approved.Count, Is.Zero);

            var pullRequest = Substitute.For<IPullRequest>();
            pullRequest.IsApproved.Returns(true);
            var activePullRequests = new [] { pullRequest };
            var filter = Substitute.For<IRepositoryFilter>();
            filter.IncludesRepo(Arg.Any<ITfGitRepository>()).Returns(false);
            systemUnderTest.RepositoryFilter = filter;

            systemUnderTest.EnsureActivePrsArePresent(activePullRequests);

            Assert.That(systemUnderTest.Approved.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestEnsureActivePrsArePresent_WhenThereIsANewUnapprovedPR_WhichMatchesRepoFilter_AddsToUnapproved()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            Assert.That(systemUnderTest.Unapproved.Count, Is.Zero);

            var pullRequest = Substitute.For<IPullRequest>();
            pullRequest.IsApproved.Returns(false);
            var activePullRequests = new[] { pullRequest };
            systemUnderTest.RepositoryFilter = null; // <-- null filter means no filtering

            systemUnderTest.EnsureActivePrsArePresent(activePullRequests);

            Assert.That(systemUnderTest.Unapproved.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestEnsureActivePrsArePresent_WhenThereIsANewUnapprovedPR_WhichDoesNotMatchRepoFilter_DoesNotAddToUnapproved()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            Assert.That(systemUnderTest.Unapproved.Count, Is.Zero);

            var pullRequest = Substitute.For<IPullRequest>();
            pullRequest.IsApproved.Returns(false);
            var activePullRequests = new[] { pullRequest };
            _tfsConnection.GetActivePullRequestsInProject(systemUnderTest).Returns(activePullRequests);
            var filter = Substitute.For<IRepositoryFilter>();
            filter.IncludesRepo(Arg.Any<ITfGitRepository>()).Returns(false);
            systemUnderTest.RepositoryFilter = filter;

            systemUnderTest.EnsureActivePrsArePresent(activePullRequests);

            Assert.That(systemUnderTest.Unapproved.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestEnsureActivePrsArePresent_ReturnsAllActivePrsMatchingRepoFilter()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            var includedRepo = Substitute.For<ITfGitRepository>();
            var excludedRepo = Substitute.For<ITfGitRepository>();
            var repoFilter = Substitute.For<IRepositoryFilter>();
            repoFilter.IncludesRepo(includedRepo).Returns(true);
            repoFilter.IncludesRepo(excludedRepo).Returns(false);
            systemUnderTest.RepositoryFilter = repoFilter;
            var activePrs = new List<IPullRequest>();
            var expectedPrIds = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                var pr = Substitute.For<IPullRequest>();
                pr.Id.Returns(i);
                activePrs.Add(pr);
                if (i % 2 == 0)
                {
                    pr.Repository.Returns(includedRepo);
                    expectedPrIds.Add(i);
                }
                else
                {
                    pr.Repository.Returns(excludedRepo);
                }
            }

            var actualPrIds = systemUnderTest.EnsureActivePrsArePresent(activePrs);

            Assert.That(actualPrIds, Is.EquivalentTo(expectedPrIds));
        }

        [Test]
        public void TestEnsureActivePrsArePresent_WhenAnExistingPRIsStillPresent_ReleasesPreviousVersion()
        {
            const int testId = 134907;
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            var firstVersionOfPr = Substitute.For<IPullRequest>();
            firstVersionOfPr.Id.Returns(testId);
            systemUnderTest.Unapproved.AddOrUpdate(testId, firstVersionOfPr, (i, existingValue) => firstVersionOfPr);
            var secondVersionOfPr = Substitute.For<IPullRequest>();
            secondVersionOfPr.Id.Returns(testId);
            var activePullRequests = new[] { secondVersionOfPr };

            systemUnderTest.EnsureActivePrsArePresent(activePullRequests);

            _tfsConnection.Received().ReleasePullRequest(firstVersionOfPr);
        }

        [Test]
        public void TestEnsureActivePrsArePresent_ReplacesApprovedPullRequestWithMoreRecentlyRetrievedInstance()
        {
            const int testKey = 3424;
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            var firstRetrievedInstance = Substitute.For<IPullRequest>();
            // This is a hack relying on the fact that client code can add to this dictionary...
            systemUnderTest.Approved.AddOrUpdate(testKey, firstRetrievedInstance, (i, existingValue) => firstRetrievedInstance);
            var secondRetrievedInstance = Substitute.For<IPullRequest>();
            secondRetrievedInstance.Id.Returns(testKey);
            secondRetrievedInstance.IsApproved.Returns(true);
            var activePullRequests = new [] {secondRetrievedInstance};

            systemUnderTest.EnsureActivePrsArePresent(activePullRequests);

            Assert.That(systemUnderTest.Approved[testKey], Is.EqualTo(secondRetrievedInstance));
        }

        [Test]
        public void TestEnsureActivePrsArePresent_ReplacesUnapprovedPullRequestWithMoreRecentlyRetrievedInstance()
        {
            const int testKey = 5678;
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            var firstRetrievedInstance = Substitute.For<IPullRequest>();
            // This is a hack relying on the fact that client code can add to this dictionary...
            systemUnderTest.Unapproved.AddOrUpdate(testKey, firstRetrievedInstance, (i, existingValue) => firstRetrievedInstance);
            var secondRetrievedInstance = Substitute.For<IPullRequest>();
            secondRetrievedInstance.Id.Returns(testKey);
            var activePullRequests = new[] { secondRetrievedInstance };

            systemUnderTest.EnsureActivePrsArePresent(activePullRequests);

            Assert.That(systemUnderTest.Unapproved[testKey], Is.EqualTo(secondRetrievedInstance));
        }

        [Test]
        public void TestRemoveStalePullRequests_RemovesAndReleasesAllStalePullRequests()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            var activePrIds = new List<int>();
            var stalePrs = new List<IPullRequest>();
            for (int i = 0; i < 13; i++)
            {
                var pr = Substitute.For<IPullRequest>();
                pr.Id.Returns(i);
                // This test covers approved and unapproved prs in one go
                if (i % 3 == 0)
                {
                    systemUnderTest.Approved.AddOrUpdate(i, pr, (i1, existingValue) => pr);
                } else {
                    systemUnderTest.Unapproved.AddOrUpdate(i, pr, (i1, existingValue) => pr);
                }

                if (i % 2 == 0)
                {
                    activePrIds.Add(i);
                }
                else
                {
                    stalePrs.Add(pr);
                }

            }

            systemUnderTest.RemoveStalePullRequests(activePrIds);

            foreach (var activePrId in activePrIds)
            {
                var prIsStillPresent = systemUnderTest.Unapproved.ContainsKey(activePrId) ||
                                       systemUnderTest.Approved.ContainsKey(activePrId);
                Assert.That(prIsStillPresent, Is.True);
            }

            foreach (var stalePr in stalePrs)
            {
                _tfsConnection.Received().ReleasePullRequest(stalePr);
            }
        }

        [Test, TestCaseSource(nameof(PullRequestCountCases))]
        public void TestApprovedPullRequestCount_ReturnsApprovedCount(int numApprovedPullRequests)
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);

            for (int i = 0; i < numApprovedPullRequests; i++)
            {
                systemUnderTest.Approved.AddOrUpdate(i, Substitute.For<IPullRequest>(), (id, request) => request);
            }

            Assert.That(systemUnderTest.ApprovedPullRequestCount, Is.EqualTo(numApprovedPullRequests));
        }

        [Test, TestCaseSource(nameof(PullRequestCountCases))]
        public void TestUnapprovedPullRequestCount_ReturnsUnapprovedCount(int numUnapprovedPullRequests)
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);

            for (int i = 0; i < numUnapprovedPullRequests; i++)
            {
                systemUnderTest.Unapproved.AddOrUpdate(i, Substitute.For<IPullRequest>(), (id, request) => request);
            }

            Assert.That(systemUnderTest.UnapprovedPullRequestCount, Is.EqualTo(numUnapprovedPullRequests));
        }

        [Test]
        public void TestUnapprovedPullRequestBecomingApprovedIsNotDuplicated()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);

            const int pullRequestId = 8793;
            var pullRequest = Substitute.For<IPullRequest>();
            pullRequest.Id.Returns(pullRequestId);
            pullRequest.IsApproved.Returns(true);
            systemUnderTest.Unapproved.AddOrUpdate(pullRequestId, pullRequest, (id, request) => request);
            _tfsConnection.GetActivePullRequestsInProject(systemUnderTest).Returns(new []{pullRequest});

            systemUnderTest.DoPullRequestRetrieval();

            Assert.That(systemUnderTest.Approved.ContainsKey(pullRequestId), Is.True);
            Assert.That(systemUnderTest.Unapproved.ContainsKey(pullRequestId), Is.False);
        }

        [Test]
        public void TestApprovedPullRequestBecomingUnapprovedIsNotDuplicated()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);

            const int pullRequestId = 8793;
            var pullRequest = Substitute.For<IPullRequest>();
            pullRequest.Id.Returns(pullRequestId);
            pullRequest.IsApproved.Returns(false);
            systemUnderTest.Approved.AddOrUpdate(pullRequestId, pullRequest, (id, request) => request);
            _tfsConnection.GetActivePullRequestsInProject(systemUnderTest).Returns(new[] { pullRequest });

            systemUnderTest.DoPullRequestRetrieval();

            Assert.That(systemUnderTest.Approved.ContainsKey(pullRequestId), Is.False);
            Assert.That(systemUnderTest.Unapproved.ContainsKey(pullRequestId), Is.True);
        }

        [Test]
        public void TestDoPullRequestRetrieval_SetsPullRequestRetrievalStatusToOngoing()
        {
            var observedRetrievalStatus = RetrievalStatus.Unstarted;
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            _tfsConnection.GetActivePullRequestsInProject(systemUnderTest).Returns(info =>
            {
                observedRetrievalStatus = systemUnderTest.PullRequestRetrievalStatus;
                return Enumerable.Empty<IPullRequest>();
            });

            systemUnderTest.DoPullRequestRetrieval();

            Assert.That(observedRetrievalStatus, Is.EqualTo(RetrievalStatus.Ongoing));
        }

        [Test]
        public void TestDoPullRequestRetrieval_GetsActivePullRequestsForTheProject()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            _tfsConnection.GetActivePullRequestsInProject(systemUnderTest).Returns(Enumerable.Empty<IPullRequest>());

            systemUnderTest.DoPullRequestRetrieval();

            _tfsConnection.Received().GetActivePullRequestsInProject(systemUnderTest);
        }

        [Test, TestCaseSource(nameof(CouldNotReachServerExceptions))]
        public async Task TestRetrievePullRequests_WhenGettingPullRequestsFailsDueToConnection_SetsStatusToFailedDueToConnection(Exception exception)
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            _tfsConnection.GetActivePullRequestsInProject(systemUnderTest).Throws(exception);

            await systemUnderTest.RetrievePullRequests();

            Assert.That(systemUnderTest.PullRequestRetrievalStatus, Is.EqualTo(RetrievalStatus.FailedDueToConnection));
        }

        [Test, TestCaseSource(nameof(UnauthorisedExceptions))]
        public async Task TestRetrievePullRequests_WhenGettingPullRequestsFailsDueToAuth_SetsStatusToFailedDueToAuth(Exception exception)
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            _tfsConnection.GetActivePullRequestsInProject(systemUnderTest).Throws(exception);

            await systemUnderTest.RetrievePullRequests();

            Assert.That(systemUnderTest.PullRequestRetrievalStatus, Is.EqualTo(RetrievalStatus.FailedDueToAuth));
        }

        [Test, TestCaseSource(nameof(UnrecognisedExceptions))]
        public async Task TestRetrievePullRequests_WhenGettingPullRequestsFailsForUnknownReason_SetsStatusToFailedReasonUnknown(Exception exception)
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            _tfsConnection.GetActivePullRequestsInProject(systemUnderTest).Throws(exception);

            await systemUnderTest.RetrievePullRequests();

            Assert.That(systemUnderTest.PullRequestRetrievalStatus, Is.EqualTo(RetrievalStatus.FailedReasonUnknown));
        }

        [Test]
        public async Task TestRetrievePullRequests_WhenGettingPullRequestsSucceeds_SetsStatusToSucceeded()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            _tfsConnection.GetActivePullRequestsInProject(systemUnderTest).Returns(Enumerable.Empty<IPullRequest>());

            await systemUnderTest.RetrievePullRequests();

            Assert.That(systemUnderTest.PullRequestRetrievalStatus, Is.EqualTo(RetrievalStatus.Suceeded));
        }

        [Test]
        public async Task TestRetrieveRepositories_WhenRepositoryRetrievalStatusIsUnstarted_CallsConnection()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            _tfsConnection.GetRepositoriesInProject(systemUnderTest).Returns(Enumerable.Empty<ITfGitRepository>());

            await systemUnderTest.RetrieveRepositories();

            await _tfsConnection.Received(1).GetRepositoriesInProject(systemUnderTest);
        }

        [Test, TestCaseSource(nameof(CouldNotReachServerExceptions))]
        public async Task TestRetrieveRepositories_WhenGettingRepositoriesFailsDueToConnection_SetsStatusToFailedDueToConnection(Exception exception)
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            _tfsConnection.GetRepositoriesInProject(systemUnderTest).Throws(exception);

            await systemUnderTest.RetrieveRepositories();

            Assert.That(systemUnderTest.RepositoryRetrievalStatus, Is.EqualTo(RetrievalStatus.FailedDueToConnection));
        }

        [Test, TestCaseSource(nameof(UnauthorisedExceptions))]
        public async Task TestRetrieveRepositories_WhenGettingRepositoriesFailsDueToAuth_SetsStatusToFailedDueToAuth(Exception exception)
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            _tfsConnection.GetRepositoriesInProject(systemUnderTest).Throws(exception);

            await systemUnderTest.RetrieveRepositories();

            Assert.That(systemUnderTest.RepositoryRetrievalStatus, Is.EqualTo(RetrievalStatus.FailedDueToAuth));
        }

        [Test, TestCaseSource(nameof(UnrecognisedExceptions))]
        public async Task TestRetrieveRepositories_WhenGettingRepositoriesFailsForUnknownReason_SetsStatusToFailedReasonUnknown(Exception exception)
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            _tfsConnection.GetRepositoriesInProject(systemUnderTest).Throws(exception);

            await systemUnderTest.RetrieveRepositories();

            Assert.That(systemUnderTest.RepositoryRetrievalStatus, Is.EqualTo(RetrievalStatus.FailedReasonUnknown));
        }

        [Test]
        public async Task TestRetrieveRepositories_WhenGettingReposSucceeds_SetsRepoRetrievalStatusToSucceeded()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            _tfsConnection.GetRepositoriesInProject(systemUnderTest).Returns(Enumerable.Empty<ITfGitRepository>());

            await systemUnderTest.RetrieveRepositories();

            Assert.That(systemUnderTest.RepositoryRetrievalStatus, Is.EqualTo(RetrievalStatus.Suceeded));
        }

        [Test]
        public async Task TestRetrieveRepositories_WhenRepoRetrievalStatusIsSucceeded_DoesNotCallConnection()
        {
            var systemUnderTest = new TfProject(new TeamProject(), _tfsConnection, _logger);
            _tfsConnection.GetRepositoriesInProject(systemUnderTest).Returns(Enumerable.Empty<ITfGitRepository>());
            await systemUnderTest.RetrieveRepositories();
            Assert.That(systemUnderTest.RepositoryRetrievalStatus, Is.EqualTo(RetrievalStatus.Suceeded));
            await _tfsConnection.Received(1).GetRepositoriesInProject(systemUnderTest);

            await systemUnderTest.RetrieveRepositories();

            await _tfsConnection.Received(1).GetRepositoriesInProject(systemUnderTest);
        }
    }
}