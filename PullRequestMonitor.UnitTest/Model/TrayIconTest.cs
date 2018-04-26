using System;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.UnitTest.Model
{
    [TestFixture]
    public class TrayIconTest
    {
        private static readonly object[] PullRequestCounts =
        {
            new object[] { 0, 0},
            new object[] { 1, 0},
            new object[] { 0, 1},
            new object[] { 1, 1},
            new object[] { 7, 3},
            new object[] { 1098, 9999999}
        };

        private static Array GetMonitorStatuses()
        {
            return Enum.GetValues(typeof(MonitorStatus));
        }

        [Test]
        public void TestPullRequests_WhenThereAreNoProjects_ReturnsNull()
        {
            var trayIcon = new TrayIcon(Substitute.For<IMonitor>());

            Assert.That(trayIcon.PullRequestCount, Is.Null);
        }

        [Test]
        public void TestUnapprovedPullRequests_WhenThereAreNoProjects_ReturnsNull()
        {
            var systemUnderTest = new TrayIcon(Substitute.For<IMonitor>());

            Assert.That(systemUnderTest.UnapprovedPullRequestCount, Is.Zero);
        }

        [Test]
        public void TestApprovedPullRequests_WhenThereAreNoProjects_ReturnsNull()
        {
            var systemUnderTest = new TrayIcon(Substitute.For<IMonitor>());

            Assert.That(systemUnderTest.ApprovedPullRequestCount, Is.Zero);
        }

        [Test, TestCaseSource(nameof(PullRequestCounts))]
        public void TestPullRequqestCounts(int unapprovedPullRequestCount, int approvedPullRequestCount)
        {
            var monitor = Substitute.For<IMonitor>();
            monitor.Status.Returns(MonitorStatus.UpdateSuccessful);
            monitor.UnapprovedPullRequestCount.Returns(unapprovedPullRequestCount);
            monitor.ApprovedPullRequestCount.Returns(approvedPullRequestCount);
            monitor.Projects.Returns(new[] {Substitute.For<ITfProject>()});
            var systemUnderTest = new TrayIcon(monitor);

            Assert.That(systemUnderTest.UnapprovedPullRequestCount, Is.EqualTo(unapprovedPullRequestCount));
            Assert.That(systemUnderTest.ApprovedPullRequestCount, Is.EqualTo(approvedPullRequestCount));
            Assert.That(systemUnderTest.PullRequestCount, Is.EqualTo(unapprovedPullRequestCount + approvedPullRequestCount));
        }

        [Test]
        public void TestRunMonitor_CallsRunOnMonitor()
        {
            var monitor = Substitute.For<IMonitor>();
            var systemUnderTest = new TrayIcon(monitor);

            systemUnderTest.RunMonitor();

            monitor.Received().Start();
        }

        [Test, TestCaseSource(nameof(GetMonitorStatuses))]
        public void TestMonitorStatus_ReturnsStatusOfMonitor(MonitorStatus monitorStatus)
        {
            var monitor = Substitute.For<IMonitor>();
            monitor.Status.Returns(monitorStatus);
            var systemUnderTest = new TrayIcon(monitor);

            Assert.That(systemUnderTest.MonitorStatus, Is.EqualTo(monitor.Status));
        }
    }
}