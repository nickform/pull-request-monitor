using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    [TestFixture]
    public class PullRequestViewModelTest
    {
        public static IEnumerable<IPullRequest> PullRequests
        {
            get
            {
                var pullRequestList = new List<IPullRequest>();

                for (int i = 0; i < 4; i++)
                {
                    var pullRequest = Substitute.For<IPullRequest>();
                    pullRequest.Id.Returns(i);
                    pullRequestList.Add(pullRequest);
                }

                return pullRequestList;
            }
        }

        [Test, TestCaseSource(nameof(PullRequests))]
        public void TestId_ReturnsIdOfPullRequest(IPullRequest pullRequest)
        {
            var systemUnderTest = new PullRequestViewModel(pullRequest);

            Assert.That(systemUnderTest.Id, Is.EqualTo(pullRequest.Id));
        }

        [Test]
        public void TestWebViewUri_ReturnsModelWebViewUri()
        {
            var pullRequest = Substitute.For<IPullRequest>();
            var webViewUri = new Uri("https://made.up.domain");
            pullRequest.WebViewUri.Returns(webViewUri);
            var systemUnderTest = new PullRequestViewModel(pullRequest);

            Assert.That(systemUnderTest.WebViewUri, Is.EqualTo(webViewUri));
        }

        [Test]
        public void TestTitle_ReturnsModelTitle()
        {
            const string testTitle = "Test pull request title";
            var pullRequest = Substitute.For<IPullRequest>();
            pullRequest.Title.Returns(testTitle);
            var systemUnderTest = new PullRequestViewModel(pullRequest);

            Assert.That(systemUnderTest.Title, Is.EqualTo(testTitle));
        }

        [Test]
        public void TestAuthorDisplayName_ReturnsModelAuthorDisplayName()
        {
            const string testAuthorDisplayName = "Test author display name";
            var pullRequest = Substitute.For<IPullRequest>();
            pullRequest.AuthorDisplayName.Returns(testAuthorDisplayName);
            var systemUnderTest = new PullRequestViewModel(pullRequest);

            Assert.That(systemUnderTest.Author, Is.EqualTo(testAuthorDisplayName));
        }

        [Test]
        public void TestRepository_ReturnsModelRepositoryName()
        {
            const string testRepositoryName = "Test repository name";
            var pullRequest = Substitute.For<IPullRequest>();
            var repo = Substitute.For<ITfGitRepository>();
            repo.Name.Returns(testRepositoryName);
            pullRequest.Repository.Returns(repo);
            var systemUnderTest = new PullRequestViewModel(pullRequest);

            Assert.That(systemUnderTest.Repository, Is.EqualTo(testRepositoryName));
        }

        [Test]
        public void TestCreated_ReturnsModelCreated()
        {
            var expectedCreatedDateTime = DateTime.UtcNow.AddDays(-30);
            var pullRequest = Substitute.For<IPullRequest>();
            pullRequest.Created.Returns(expectedCreatedDateTime);
            var systemUnderTest = new PullRequestViewModel(pullRequest);

            Assert.That(systemUnderTest.Created, Is.EqualTo(expectedCreatedDateTime));
        }

        [Test]
        public void TestCompleted_WhenModelCompletedIsNull_ReturnsNull()
        {
            var pullRequest = Substitute.For<IPullRequest>();
            pullRequest.Completed.Returns(null as DateTime?);
            var systemUnderTest = new PullRequestViewModel(pullRequest);

            Assert.That(systemUnderTest.Completed, Is.Null);
        }

        [Test]
        public void TestCompleted_WhenModelCompletedIsNotNull_ReturnsModelCompleted()
        {
            var expectedCompletedDateTime = DateTime.UtcNow.AddDays(-3);
            var pullRequest = Substitute.For<IPullRequest>();
            pullRequest.Completed.Returns(expectedCompletedDateTime);
            var systemUnderTest = new PullRequestViewModel(pullRequest);

            Assert.That(systemUnderTest.Completed, Is.EqualTo(expectedCompletedDateTime));
        }

        [Test]
        public void TestIsCompleted_WhenModelCompletedIsNull_ReturnsFalse()
        {
            var pullRequest = Substitute.For<IPullRequest>();
            pullRequest.Completed.Returns(null as DateTime?);
            var systemUnderTest = new PullRequestViewModel(pullRequest);

            Assert.That(systemUnderTest.IsCompleted, Is.False);
        }

        [Test]
        public void TestIsCompleted_WhenModelCompletedIsNotNull_ReturnsTrue()
        {
            var expectedCompletedDateTime = DateTime.UtcNow.AddDays(-3);
            var pullRequest = Substitute.For<IPullRequest>();
            pullRequest.Completed.Returns(expectedCompletedDateTime);
            var systemUnderTest = new PullRequestViewModel(pullRequest);

            Assert.That(systemUnderTest.IsCompleted, Is.True);
        }
    }
}