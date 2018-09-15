using System.Collections.Concurrent;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    [TestFixture]
    public class PullRequestListViewModelTest
    {
        [Test]
        public void TestPullRequests_WhenModelHasTwo_HasTwo()
        {
            var model = new ConcurrentDictionary<int, IPullRequest>();
            model.AddOrUpdate(0, Substitute.For<IPullRequest>(), (i, request) => request);
            model.AddOrUpdate(1, Substitute.For<IPullRequest>(), (i, request) => request);
            var systemUnderTest = new ActivePullRequestListViewModel();
            systemUnderTest.Model = model;
            systemUnderTest.Update();

            Assert.That(systemUnderTest.PullRequests.Count, Is.EqualTo(2));
        }
        [Test]
        public void TestPullRequests_ReturnsPullRequestsOrderedByIdAscending()
        {
            var model = new ConcurrentDictionary<int, IPullRequest>();
            var pr1495 = Substitute.For<IPullRequest>();
            pr1495.Id.Returns(1495);
            var pr66 = Substitute.For<IPullRequest>();
            pr66.Id.Returns(66);
            var pr3 = Substitute.For<IPullRequest>();
            pr3.Id.Returns(3);
            var pr237840 = Substitute.For<IPullRequest>();
            pr237840.Id.Returns(237840);
            var prs = new List<IPullRequest> {pr1495, pr66, pr3, pr237840};
            foreach (var pr in prs)
            {
                model.AddOrUpdate(pr.Id, pr, (i, req) => req);
            }
            var systemUnderTest = new ActivePullRequestListViewModel {Model = model};
            systemUnderTest.Update();

            var previousId = 0;
            foreach (var prViewModel in systemUnderTest.PullRequests)
            {
                Assert.That(prViewModel.Id, Is.GreaterThan(previousId));
                previousId = prViewModel.Id;
            }
        }
    }
}