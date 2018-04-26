using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    [TestFixture]
    public class MonitorWindowViewModelTest
    {
        private IMonitor _monitor;
        private CouldNotReachServerViewModel _couldNotReachServerViewModel;

        [SetUp]
        public void SetUp()
        {
            _monitor = Substitute.For<IMonitor>();
            _couldNotReachServerViewModel = new CouldNotReachServerViewModel(Substitute.For<IApplicationActions>(), Substitute.For<IAppSettings>());
        }

        [Test]
        public void TestContentViewModel_WhenMonitorStatusIsAwaitingFirstUpdate_ReturnsFirstUpdateViewModel()
        {
            _monitor.Status.Returns(MonitorStatus.AwaitingFirstUpdate);
            var firstUpdateViewModel = new FirstUpdateViewModel();

            var systemUnderTest = new MonitorWindowViewModel(_monitor, Substitute.For<INoProjectsViewModel>(),
                new SingleProjectViewModel(new PullRequestListViewModel(), new PullRequestListViewModel()),
                firstUpdateViewModel, _couldNotReachServerViewModel, new UnrecognisedErrorViewModel());

            Assert.That(systemUnderTest.ContentViewModel, Is.EqualTo(firstUpdateViewModel));
        }

        [Test]
        public void TestContentViewModel_WhenMonitorStatusIsCouldNotReachServer_ReturnsCouldNotReachServerViewModel()
        {
            _monitor.Status.Returns(MonitorStatus.CouldNotReachServer);

            var systemUnderTest = new MonitorWindowViewModel(_monitor, Substitute.For<INoProjectsViewModel>(),
                new SingleProjectViewModel(new PullRequestListViewModel(), new PullRequestListViewModel()),
                new FirstUpdateViewModel(), _couldNotReachServerViewModel, new UnrecognisedErrorViewModel());

            Assert.That(systemUnderTest.ContentViewModel, Is.EqualTo(_couldNotReachServerViewModel));
        }

        [Test]
        public void TestContentViewModel_WhenMonitorStatusIsNoProject_ReturnsNoProjectsViewModel()
        {
            var noProjectsViewModel = Substitute.For<INoProjectsViewModel>();
            _monitor.Status.Returns(MonitorStatus.NoProjects);

            var systemUnderTest = new MonitorWindowViewModel(_monitor, noProjectsViewModel,
                new SingleProjectViewModel(new PullRequestListViewModel(), new PullRequestListViewModel()),
                new FirstUpdateViewModel(), _couldNotReachServerViewModel, new UnrecognisedErrorViewModel());

            Assert.That(systemUnderTest.ContentViewModel, Is.EqualTo(noProjectsViewModel));
        }

        [Test]
        public void TestContentViewModel_WhenMonitorStatusIsUnrecognisedError_ReturnsUnrecognisedErrorViewModel()
        {
            _monitor.Status.Returns(MonitorStatus.UnrecognisedError);
            var unrecognisedErrorViewModel = new UnrecognisedErrorViewModel();
            var systemUnderTest = new MonitorWindowViewModel(_monitor, Substitute.For<INoProjectsViewModel>(),
                new SingleProjectViewModel(new PullRequestListViewModel(), new PullRequestListViewModel()),
                new FirstUpdateViewModel(), _couldNotReachServerViewModel, unrecognisedErrorViewModel);

            Assert.That(systemUnderTest.ContentViewModel, Is.EqualTo(unrecognisedErrorViewModel));
        }

        [Test]
        public void TestContentViewModel_WhenMonitorStatusIsUpdateSuccessful_ReturnsSingleProjectViewModel()
        {
            var singleProjectViewModel = new SingleProjectViewModel(new PullRequestListViewModel(), new PullRequestListViewModel());
            _monitor.Projects.Returns(new ObservableCollection<ITfProject> {Substitute.For<ITfProject>()});
            _monitor.Status.Returns(MonitorStatus.UpdateSuccessful);

            var systemUnderTest = new MonitorWindowViewModel(_monitor,
                new NoProjectsViewModel(Substitute.For<IApplicationActions>()), singleProjectViewModel,
                new FirstUpdateViewModel(), _couldNotReachServerViewModel, new UnrecognisedErrorViewModel()); 

            Assert.That(systemUnderTest.ContentViewModel, Is.EqualTo(singleProjectViewModel));
        }

        [Test]
        public void TestUpdate_WhenMonitorStatusIsNoProjects_UpdatesNoProjectsViewModelOnly()
        {
            var singleProjectViewModel = new SingleProjectViewModel(new PullRequestListViewModel(), new PullRequestListViewModel());
            var noProjectsViewModel = Substitute.For<INoProjectsViewModel>();
            _monitor.Status.Returns(MonitorStatus.NoProjects);
            var systemUnderTest = new MonitorWindowViewModel(_monitor, noProjectsViewModel, singleProjectViewModel,
                new FirstUpdateViewModel(), _couldNotReachServerViewModel, new UnrecognisedErrorViewModel());

            systemUnderTest.Update();

            noProjectsViewModel.Received().Update();
            Assert.That(singleProjectViewModel.Approved.Model, Is.Null);
            Assert.That(singleProjectViewModel.Unapproved.Model, Is.Null);
        }

        [Test]
        public void TestUpdate_WhenProjectsHaveBeenAdded_RaisesContentViewModelChanged()
        {
            var numContentViewModelChanged = 0;
            var pullRequestListViewModel = new PullRequestListViewModel();
            var singleProjectViewModel = new SingleProjectViewModel(pullRequestListViewModel, pullRequestListViewModel);
            singleProjectViewModel.Model = null;
            var noProjectsViewModel = Substitute.For<INoProjectsViewModel>();
            var tfProject = Substitute.For<ITfProject>();
            tfProject.Approved.Returns(new ConcurrentDictionary<int, IPullRequest>());
            tfProject.Unapproved.Returns(new ConcurrentDictionary<int, IPullRequest>());
            _monitor.Projects.Returns(new[] {tfProject});
            _monitor.Status.Returns(MonitorStatus.UpdateSuccessful);
            var systemUnderTest = new MonitorWindowViewModel(_monitor, noProjectsViewModel, singleProjectViewModel,
                new FirstUpdateViewModel(), _couldNotReachServerViewModel, new UnrecognisedErrorViewModel());
            systemUnderTest.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(MonitorWindowViewModel.ContentViewModel))
                {
                    numContentViewModelChanged++;
                }
            };

            systemUnderTest.Update();

            Assert.That(numContentViewModelChanged, Is.EqualTo(1));
        }

        [Test]
        public void TestUpdate_WhenMonitorStatusIsUpdateSuccessful_UpdatesSingleProjectViewModelOnly()
        {
            var noProjectsViewModel = Substitute.For<INoProjectsViewModel>();
            var singleProjectViewModel = new SingleProjectViewModel(new PullRequestListViewModel(), new PullRequestListViewModel());

            var tfProject = Substitute.For<ITfProject>();
            var approvedDictionary = new ConcurrentDictionary<int, IPullRequest>();
            var unapprovedDictionary = new ConcurrentDictionary<int, IPullRequest>();
            tfProject.Approved.Returns(approvedDictionary);
            tfProject.Unapproved.Returns(unapprovedDictionary);
            _monitor.Projects.Returns(new[] {tfProject});
            _monitor.Status.Returns(MonitorStatus.UpdateSuccessful);

            var systemUnderTest = new MonitorWindowViewModel(_monitor,
                noProjectsViewModel, singleProjectViewModel,
                new FirstUpdateViewModel(), _couldNotReachServerViewModel, new UnrecognisedErrorViewModel());

            systemUnderTest.Update();

            noProjectsViewModel.DidNotReceive().Update();
            Assert.That(singleProjectViewModel.Approved.Model, Is.EqualTo(approvedDictionary));
            Assert.That(singleProjectViewModel.Unapproved.Model, Is.EqualTo(unapprovedDictionary));
        }
    }
}