using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    [TestFixture]
    public class TrayIconViewModelTest
    {
        private static readonly object[] UnapprovedAndApprovedPullRequestCounts =
        {
            new object[] {0, 1},
            new object[] {1, 0},
            new object[] {1, 1},
            new object[] {1, 3},
            new object[] {0, 0},
            new object[] {33, 12}
        };

        private static readonly int?[] PullRequestCounts = {null, 0, 1, 2, 7, 999};

        private static readonly object[] MapOfNonSuccessStateToMessages =
        {
            new object[] { MonitorStatus.AwaitingFirstUpdate, Properties.Resources.AwaitingFirstUpdateMessage},
            new object[] { MonitorStatus.NoProjects, Properties.Resources.NoProjectsMessage},
            new object[] { MonitorStatus.CouldNotReachServer, Properties.Resources.CouldNotReachServerMessage},
            new object[] { MonitorStatus.AuthorisationError, Properties.Resources.AuthorisationErrorMessage},
            new object[] { MonitorStatus.UnrecognisedError, Properties.Resources.UnrecognisedErrorMessage}
        };

        [Test]
        public void TestShowSettingsCommand_ReturnsApplicationActionsCommand()
        {
            var delegateCommand = new DelegateCommand();
            var applicationActions = Substitute.For<IApplicationActions>();
            applicationActions.ShowSettingsCommand.Returns(delegateCommand);
            var systemUnderTest = new TrayIconViewModel(applicationActions);

            Assert.That(systemUnderTest.ShowSettingsCommand, Is.EqualTo(delegateCommand));
        }

        [Test]
        public void TestShowMonitorWindowCommand_ReturnsApplicationActionsCommand()
        {
            var delegateCommand = new DelegateCommand();
            var applicationActions = Substitute.For<IApplicationActions>();
            applicationActions.ShowMonitorWindowCommand.Returns(delegateCommand);
            var systemUnderTest = new TrayIconViewModel(applicationActions);

            Assert.That(systemUnderTest.ShowMonitorWindowCommand, Is.EqualTo(delegateCommand));
        }

        [Test]
        public void TestExitApplicationCommand_ReturnsApplicationActionsCommand()
        {
            var delegateCommand = new DelegateCommand();
            var applicationActions = Substitute.For<IApplicationActions>();
            applicationActions.ExitApplicationCommand.Returns(delegateCommand);
            var systemUnderTest = new TrayIconViewModel(applicationActions);

            Assert.That(systemUnderTest.ExitApplicationCommand, Is.EqualTo(delegateCommand));
        }

        [Test, TestCaseSource(nameof(MapOfNonSuccessStateToMessages))]
        public void TestTooltipText_WhenStatusIsNotUpdateSuccessful_ReturnsMatchingMessage(MonitorStatus monitorStatus, string expectedMessage)
        {
            var trayIcon = Substitute.For<ITrayIcon>();
            trayIcon.MonitorStatus.Returns(monitorStatus);
            var systemUnderTest = new TrayIconViewModel(Substitute.For<IApplicationActions>());
            systemUnderTest.Model = trayIcon;

            Assert.That(systemUnderTest.TooltipText, Is.EqualTo(expectedMessage));
        }

        [Test, TestCaseSource(nameof(UnapprovedAndApprovedPullRequestCounts))]
        public void TestTooltipText_WhenStatusIsUpdateSuccessful_IncorporatesPullRequestCounts(int numUnapprovedPullRequests, int numApprovedPullRequests)
        {
            var trayIcon = Substitute.For<ITrayIcon>();
            trayIcon.MonitorStatus.Returns(MonitorStatus.UpdateSuccessful);
            trayIcon.UnapprovedPullRequestCount.Returns(numUnapprovedPullRequests);
            trayIcon.ApprovedPullRequestCount.Returns(numApprovedPullRequests);
            var systemUnderTest = new TrayIconViewModel(Substitute.For<IApplicationActions>());
            systemUnderTest.Model = trayIcon;

            var expectedToolTipText = string.Format(Properties.Resources.PullRequestCountTooltipFormatString,
                numUnapprovedPullRequests, numApprovedPullRequests);
            Assert.That(systemUnderTest.TooltipText, Is.EqualTo(expectedToolTipText));
        }

        [Test]
        public void TestCanSetAndGetModel()
        {
            var systemUnderTest = new TrayIconViewModel(Substitute.For<IApplicationActions>());
            Assert.That(systemUnderTest.Model, Is.Null);

            var trayIcon = Substitute.For<ITrayIcon>();

            systemUnderTest.Model = trayIcon;

            Assert.That(systemUnderTest.Model, Is.EqualTo(trayIcon));
        }

        [Test]
        public void TestModelSetter_WhenValueIsNew_Subscribes()
        {
            var systemUnderTest = new TrayIconViewModel(Substitute.For<IApplicationActions>());
            Assert.That(systemUnderTest.Model, Is.Null);

            var trayIcon = Substitute.For<ITrayIcon>();

            systemUnderTest.Model = trayIcon;

            trayIcon.Received().UpdateCompleted += Arg.Any<EventHandler>();
        }

        [Test]
        public void TestModelSetter_WhenValueIsNewAndExistingIsNotNull_UnsubscribesFromExistingValue()
        {
            var systemUnderTest = new TrayIconViewModel(Substitute.For<IApplicationActions>());
            var existingValue = Substitute.For<ITrayIcon>();
            systemUnderTest.Model = existingValue;
            var newValue = Substitute.For<ITrayIcon>();

            systemUnderTest.Model = newValue;

            existingValue.Received().UpdateCompleted -= Arg.Any<EventHandler>();
        }

        [Test]
        public void TestModelSetter_WhenValueIsExistingAndNotNull_DoesNotResubscribe()
        {
            var systemUnderTest = new TrayIconViewModel(Substitute.For<IApplicationActions>());
            var trayIcon = Substitute.For<ITrayIcon>();
            systemUnderTest.Model = trayIcon;

            trayIcon.Received(1).UpdateCompleted += Arg.Any<EventHandler>();

            systemUnderTest.Model = trayIcon;

            trayIcon.Received(1).UpdateCompleted += Arg.Any<EventHandler>();
        }

        [Test]
        public void TestOnModelCompleted_RaisesPropertyChangedNotifications()
        {
            var expectedPropertyNotifications = new HashSet<string>();
            expectedPropertyNotifications.Add("PullRequestCount");
            expectedPropertyNotifications.Add("TooltipText");
            var systemUnderTest = new TrayIconViewModel(Substitute.For<IApplicationActions>());
            var trayIcon = Substitute.For<ITrayIcon>();
            systemUnderTest.Model = trayIcon;

            systemUnderTest.PropertyChanged +=
                (sender, args) =>
                {
                    Assert.That(expectedPropertyNotifications.Contains(args.PropertyName));
                    expectedPropertyNotifications.Remove(args.PropertyName);
                };

            trayIcon.UpdateCompleted += Raise.Event();

            Assert.That(expectedPropertyNotifications, Is.Empty);
        }

        [Test]
        public void TestOnModelCompleted_CallsApplicationActionsUpdateMonitorViewModel()
        {
            var applicationActions = Substitute.For<IApplicationActions>();
            var systemUnderTest = new TrayIconViewModel(applicationActions);
            var trayIcon = Substitute.For<ITrayIcon>();
            systemUnderTest.Model = trayIcon;

            trayIcon.UpdateCompleted += Raise.Event();

            applicationActions.Received().UpdateMonitorViewModel();
        }

        [Test, TestCaseSource(nameof(PullRequestCounts))]
        public void TestPullRequestCount_ReturnsModelPullRequestCount(int? pullRequestCount)
        {
            var systemUnderTest = new TrayIconViewModel(Substitute.For<IApplicationActions>());
            var trayIcon = Substitute.For<ITrayIcon>();
            trayIcon.PullRequestCount.Returns(pullRequestCount);

            systemUnderTest.Model = trayIcon;

            Assert.That(systemUnderTest.PullRequestCount, Is.EqualTo(pullRequestCount));
        }
    }
}