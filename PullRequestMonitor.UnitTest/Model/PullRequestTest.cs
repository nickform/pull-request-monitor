using System;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.UnitTest.Model
{
    [TestFixture]
    public class PullRequestTest
    {
        /// <summary>
        /// See https://www.visualstudio.com/en-us/docs/integrate/api/git/pull-requests/reviewers#update-a-reviewers-vote
        /// </summary>
        public static readonly short[] UnapprovedVoteCodes = { -10, -5, 0 };
        /// <summary>
        /// See https://www.visualstudio.com/en-us/docs/integrate/api/git/pull-requests/reviewers#update-a-reviewers-vote
        /// </summary>
        public static readonly short[] ApprovedVoteCodes = { +5, +10 };

        public static readonly int[] PullRequestIds = {10, 1234, 199790};
        private ITfGitRepository _repo;

        [SetUp]
        public void Setup()
        {
            _repo = Substitute.For<ITfGitRepository>();
        }

        [Test]
        public void TestId_ReturnsIdOfImpl()
        {
            const int testId = 98798;
            var systemUnderTest = new PullRequest(new GitPullRequest {PullRequestId = testId}, "server", _repo);

            Assert.That(systemUnderTest.Id, Is.EqualTo(testId));
        }
        
        [Test]
        public void TestRepository_ReturnsRepositoryPassedToConstructor()
        {
            const string repositoryName = "testRepoName.git";
            var systemUnderTest =
                new PullRequest(new GitPullRequest {Repository = new GitRepository {Name = repositoryName}},
                    "server-uri",
                    _repo);

            Assert.That(systemUnderTest.Repository, Is.EqualTo(_repo));
        }

        [Test]
        public void TestAuthorDisplayName_ReturnsDisplayNameOfImplsCreatedBy()
        {
            const string testAuthorName = "Test Author Display Name";
            var systemUnderTest =
                new PullRequest(new GitPullRequest {CreatedBy = new IdentityRef {DisplayName = testAuthorName}},
                    "server-uri",
                    _repo);

            Assert.That(systemUnderTest.AuthorDisplayName, Is.EqualTo(testAuthorName));
        }

        [Test]
        public void TestTitle_ReturnsTitleOfImpl()
        {
            const string testTitle = "Test pull request title";
            var systemUnderTest =
                new PullRequest(new GitPullRequest { Title = testTitle  },
                    "server-uri",
                    _repo);

            Assert.That(systemUnderTest.Title, Is.EqualTo(testTitle));
        }

        [Test]
        public void TestTitle_Indicates_IsWaitingForAuthor()
        {
            const string testTitle = "Test pull request title";
            IdentityRefWithVote identityRef = new IdentityRefWithVote();
            identityRef.Vote = -5;
            IdentityRefWithVote[] identityRefList = new IdentityRefWithVote[1];
            identityRefList[0] = identityRef;
            GitPullRequest gitPullRequest = new GitPullRequest {Title = testTitle};
            gitPullRequest.Reviewers = identityRefList;
            var systemUnderTest =
                new PullRequest(gitPullRequest,
                    "server-uri",
                    _repo);

            Assert.That(systemUnderTest.Title, Does.Contain(" [Waiting for author]"));
        }

        [Test]
        public void TestTitle_Indicates_IsRejected()
        {
            const string testTitle = "Test pull request title";
            IdentityRefWithVote identityRef = new IdentityRefWithVote();
            identityRef.Vote = -10;
            IdentityRefWithVote[] identityRefList = new IdentityRefWithVote[1];
            identityRefList[0] = identityRef;
            GitPullRequest gitPullRequest = new GitPullRequest { Title = testTitle };
            gitPullRequest.Reviewers = identityRefList;
            var systemUnderTest =
                new PullRequest(gitPullRequest,
                    "server-uri",
                    _repo);

            Assert.That(systemUnderTest.Title, Does.Contain(" [Rejected]"));
        }

        [Test]
        public void TestCreated_ReturnsCreatedOfImpl()
        {
            var creationDate = DateTime.UtcNow.AddDays(-1);
            var systemUnderTest =
                new PullRequest(new GitPullRequest { CreationDate = creationDate },
                    "server-uri",
                    _repo);

            Assert.That(systemUnderTest.Created, Is.EqualTo(creationDate));
        }

        [Test]
        public void TestCompleted_WhenImplStatusIsNotCompleted_ReturnsNull()
        {
            var systemUnderTest =
                new PullRequest(new GitPullRequest { Status= PullRequestStatus.Active },
                    "server-uri",
                    _repo);

            Assert.That(systemUnderTest.Completed, Is.Null);
        }

        [Test]
        public void TestCompleted_WhenImplStatusIsCompleted_ReturnsClosedOfImpl()
        {
            var closedDateTime = DateTime.UtcNow.AddDays(-1);
            var systemUnderTest =
                new PullRequest(new GitPullRequest { Status = PullRequestStatus.Completed, ClosedDate = closedDateTime},
                    "server-uri",
                    _repo);

            Assert.That(systemUnderTest.Completed, Is.EqualTo(closedDateTime));
        }

        [Test, TestCaseSource(nameof(UnapprovedVoteCodes))]
        public void TestIsApproved_WhenThereIsOneReviewerVotingZeroOrLess_ReturnsFalse(short vote)
        {
            var pullRequest = new GitPullRequest { Reviewers = new []{new IdentityRefWithVote { Vote = vote} }};
            var systemUnderTest = new PullRequest(pullRequest, "server-uri", _repo);

            Assert.That(systemUnderTest.IsApproved, Is.False);
        }

        [Test, TestCaseSource(nameof(ApprovedVoteCodes))]
        public void TestIsApproved_WhenThereIsOneReviewerVotingMoreThanZero_ReturnsTrue(short vote)
        {
            var pullRequest = new GitPullRequest { Reviewers = new[] { new IdentityRefWithVote { Vote = vote } } };
            var systemUnderTest = new PullRequest(pullRequest, "server-uri", _repo);

            Assert.That(systemUnderTest.IsApproved, Is.True);
        }

        [Test]
        public void TestIsApproved_WhenThereIsOneReviewerVotingMoreThanZeroAndOneVotingLessThanZero_ReturnsFalse()
        {
            var pullRequest = new GitPullRequest
            {
                Reviewers = new[]
                {
                    new IdentityRefWithVote { Vote = -5 },
                    new IdentityRefWithVote { Vote = +10 }
                }
            };
            var systemUnderTest = new PullRequest(pullRequest, "server-uri", _repo);

            Assert.That(systemUnderTest.IsApproved, Is.False);
        }

        [Test, TestCaseSource(nameof(PullRequestIds))]
        public void TestWebViewUri_ReturnsRepositoryUriPlusPullRequestPlusId(int pullRequestId)
        {
            const string serverUriString = "https://www.google.com/";
            const string projectName = "TestProjectName";
            const string repositoryName = "testRepoName.git";
            var project = Substitute.For<ITfProject>();
            project.Name.Returns(projectName);
            _repo.Project.Returns(project);
            _repo.Name.Returns(repositoryName);
            var systemUnderTest =
                new PullRequest(new GitPullRequest {PullRequestId = pullRequestId},
                    serverUriString, _repo);

            var expected = $"{serverUriString}{projectName}/_git/{repositoryName}/pullrequest/{pullRequestId:d}";
            Assert.That(systemUnderTest.WebViewUri.AbsoluteUri, Is.EqualTo(expected));
        }
    }
}