using System.Collections.Concurrent;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    [TestFixture]
    public class SingleProjectViewModelTest
    {
        private readonly ITfProject _tfProject = Substitute.For<ITfProject>();

        public SingleProjectViewModelTest()
        {
            _tfProject.Approved.Returns(new ConcurrentDictionary<int, IPullRequest>());
            _tfProject.Unapproved.Returns(new ConcurrentDictionary<int, IPullRequest>());
            _tfProject.Completed.Returns(new ConcurrentDictionary<int, IPullRequest>());
        }

        [Test]
        public void TestApproved_ForNewInstance_IsNotNull()
        {
            var systemUnderTest = new SingleProjectViewModel(new ActivePullRequestListViewModel(), new ActivePullRequestListViewModel(), new CompletedPullRequestListViewModel());

            Assert.That(systemUnderTest.Approved, Is.Not.Null);
        }


        [Test]
        public void TestApprovalNeeded_ForNewInstance_IsNotNull()
        {
            var systemUnderTest = new SingleProjectViewModel(new ActivePullRequestListViewModel(), new ActivePullRequestListViewModel(), new CompletedPullRequestListViewModel());

            Assert.That(systemUnderTest.Unapproved, Is.Not.Null);
        }
    }
}