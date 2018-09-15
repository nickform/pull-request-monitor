using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    [TestFixture]
    public class PullRequestDescendingListViewModelTest
    {
        [Test]
        public void TestPullRequests_WhenModelHasTwo_HasTwo()
        {
            var model = new ConcurrentDictionary<int, IPullRequest>();
            var first = Substitute.For<IPullRequest>();
            first.Completed.Returns(DateTime.Now);
            model.AddOrUpdate(0, first, (i, request) => request);
            var second = Substitute.For<IPullRequest>();
            second.Completed.Returns(DateTime.Today);
            model.AddOrUpdate(1, second, (i, request) => request);
            var systemUnderTest = new PullRequestDescendingListViewModel();
            systemUnderTest.Model = model;
            systemUnderTest.Update();

            Assert.That(systemUnderTest.PullRequests.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestPullRequests_FiltersOutPRsWithNullCompletedDateTime()
        {
            var model = new ConcurrentDictionary<int, IPullRequest>();
            var now = DateTime.Now;
            var latest = Substitute.For<IPullRequest>();
            latest.Id.Returns(1);
            latest.Completed.Returns(now.Subtract(TimeSpan.FromSeconds(37)));
            var latestButOne = Substitute.For<IPullRequest>();
            latestButOne.Id.Returns(2);
            latestButOne.Completed.Returns(now.Subtract(TimeSpan.FromMinutes(14)));
            var notCompletedYet = Substitute.For<IPullRequest>();
            notCompletedYet.Id.Returns(3);
            notCompletedYet.Completed.Returns(null as DateTime?);
            var latestButTwo = Substitute.For<IPullRequest>();
            latestButTwo.Id.Returns(4);
            latestButTwo.Completed.Returns(now.Subtract(TimeSpan.FromHours(1)));
            var prs = new List<IPullRequest> {latestButOne, latestButTwo, notCompletedYet, latest};
            foreach (var pr in prs)
            {
                model.AddOrUpdate(pr.Id, pr, (i, req) => req);
            }

            var systemUnderTest = new PullRequestDescendingListViewModel {Model = model};
            systemUnderTest.Update();

            Assert.That(systemUnderTest.PullRequests.Count, Is.EqualTo(3));
            foreach (var prViewModel in systemUnderTest.PullRequests)
            {
                Assert.NotNull(prViewModel.Completed);
            }
        }

        [Test]
        public void TestPullRequests_ReturnsPullRequestsOrderedByIdDescending()
        {
            var model = new ConcurrentDictionary<int, IPullRequest>();
            var now = DateTime.Now;
            var latest = Substitute.For<IPullRequest>();
            latest.Id.Returns(1);
            latest.Completed.Returns(now.Subtract(TimeSpan.FromSeconds(37)));
            var latestButOne = Substitute.For<IPullRequest>();
            latestButOne.Id.Returns(2);
            latestButOne.Completed.Returns(now.Subtract(TimeSpan.FromMinutes(14)));
            var latestButTwo = Substitute.For<IPullRequest>();
            latestButTwo.Id.Returns(4);
            latestButTwo.Completed.Returns(now.Subtract(TimeSpan.FromHours(1)));
            var prs = new List<IPullRequest> {latestButOne, latest, latestButTwo };
            foreach (var pr in prs)
            {
                model.AddOrUpdate(pr.Id, pr, (i, req) => req);
            }

            var systemUnderTest = new PullRequestDescendingListViewModel {Model = model};
            systemUnderTest.Update();

            DateTime? previousCompleted = now;
            foreach (var prViewModel in systemUnderTest.PullRequests)
            {
                Assert.That(prViewModel.Completed, Is.LessThan(previousCompleted));
                previousCompleted = prViewModel.Completed;
            }
        }
    }
}