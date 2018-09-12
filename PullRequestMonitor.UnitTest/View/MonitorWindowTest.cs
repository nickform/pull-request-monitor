using System.Collections.Concurrent;
using System.Threading;
using System.Windows;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;
using PullRequestMonitor.View;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.View
{
    public class MonitorWindowTest
    {
        [Test, Explicit, Category("Explicit"), Apartment(ApartmentState.STA)]
        public void ShowWindow()
        {
            MonitorWindow window;

            var t = new Thread(() =>
            {
                var gitPullRequests = new ConcurrentDictionary<int, IPullRequest>();
                for (int i = 0; i < 5; i++)
                {
                    var pullRequest = Substitute.For<IPullRequest>();
                    pullRequest.Id.Returns(i);
                    pullRequest.Title.Returns("Test pull request");
                    pullRequest.AuthorDisplayName.Returns("Alan Develo");
                    var repo = Substitute.For<ITfGitRepository>();
                    repo.Name.Returns("test-repo.git");
                    pullRequest.Repository.Returns(repo);
                    gitPullRequests.AddOrUpdate(i, pullRequest, (j, request) => request);
                }

                var project = Substitute.For<ITfProject>();
                project.Unapproved.Returns(gitPullRequests);
                project.Approved.Returns(gitPullRequests);

                var singleProjectViewModel = new SingleProjectViewModel(new PullRequestListViewModel(), new PullRequestListViewModel(), new PullRequestListViewModel());

                var monitor = Substitute.For<IMonitor>();
                monitor.Projects.Returns(new[] { project });
                monitor.Status.Returns(MonitorStatus.UpdateSuccessful);

                var monitorViewModel = new MonitorWindowViewModel(monitor,
                    Substitute.For<INoProjectsViewModel>(), singleProjectViewModel,
                    new FirstUpdateViewModel(), new CouldNotReachServerViewModel(
                        Substitute.For<IApplicationActions>(), Substitute.For<IAppSettings>()),
                    new UnrecognisedErrorViewModel());
                monitorViewModel.Update();
                window = new MonitorWindow
                {
                    WindowStyle = WindowStyle.SingleBorderWindow,
                    DataContext = monitorViewModel
                };

                window.Closed += (sender, args) => System.Windows.Threading.Dispatcher.ExitAllFrames();

                window.Show();

                System.Windows.Threading.Dispatcher.Run();
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }
    }
}