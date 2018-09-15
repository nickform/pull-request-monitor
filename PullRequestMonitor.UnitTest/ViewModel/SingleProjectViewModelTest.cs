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
        public void TestName_WhenModelIsNull_ReturnsEmptyString()
        {
            var systemUnderTest = new SingleProjectViewModel(new ActivePullRequestListViewModel(), new ActivePullRequestListViewModel(), new CompletedPullRequestListViewModel());

            Assert.That(systemUnderTest.Name, Is.Empty);
        }

        [Test]
        public void TestName_WhenModelIsNotNull_ReturnsModelName()
        {
            var name = "testy name";
            var systemUnderTest = new SingleProjectViewModel(new ActivePullRequestListViewModel(), new ActivePullRequestListViewModel(), new CompletedPullRequestListViewModel());
            _tfProject.Name.Returns(name);
            systemUnderTest.Model = _tfProject;

            Assert.That(systemUnderTest.Name, Is.EqualTo(name));
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

        [Test]
        public void TestModelSetter_RaisesPropertyChangedForName()
        {
            var numCalls = 0;
            var systemUnderTest = new SingleProjectViewModel(new ActivePullRequestListViewModel(), new ActivePullRequestListViewModel(), new CompletedPullRequestListViewModel());
            systemUnderTest.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SingleProjectViewModel.Name)) numCalls++;
            };
            systemUnderTest.Model = _tfProject;

            Assert.That(numCalls, Is.EqualTo(1));
        }
    }
}