using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.UnitTest.Model
{
    [TestFixture]
    public class MonitorSettingsTest
    {
        private static readonly int[] pollIntervals = { 1, 10, 7, 32 };

        [Test]
        public void TestProjects_WhenAppSettingsVstsAccountIsEmpty_IsEmpty()
        {
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.VstsAccount.Returns("");
            appSettings.ProjectId.Returns(Guid.NewGuid());
            var systemUnderTest = new MonitorSettings(appSettings);

            Assert.That(systemUnderTest.Projects, Is.Empty);
        }

        [Test]
        public void TestProjects_WhenAppSettingsProjectIdIsEmptyGuid_IsEmpty()
        {
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.VstsAccount.Returns("not-empty");
            appSettings.ProjectId.Returns(Guid.Empty);
            var systemUnderTest = new MonitorSettings(appSettings);

            Assert.That(systemUnderTest.Projects, Is.Empty);
        }

        [Test]
        public void TestProjects_WhenAppSettingsProjectIdIsNotEmptyGuid_ContainsSingleProjectIdentifier()
        {
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.VstsAccount.Returns("not-an-account");
            appSettings.ProjectId.Returns(Guid.NewGuid());
            var systemUnderTest = new MonitorSettings(appSettings);

            Assert.That(systemUnderTest.Projects.Count(), Is.EqualTo(1));
        }

        [Test]
        public void TestProjects_ProjectIdentifierVstsAccount_MatchesAppSettingsVstsAccount()
        {
            var appSettings = Substitute.For<IAppSettings>();
            var expectedVstsAccount = "this-would-never-be-right-by-accident";
            appSettings.VstsAccount.Returns(expectedVstsAccount);
            appSettings.ProjectId.Returns(Guid.NewGuid());
            var systemUnderTest = new MonitorSettings(appSettings);

            // ReSharper disable once PossibleNullReferenceException
            var actualServerBaseUri = systemUnderTest.Projects.FirstOrDefault().VstsAccount;

            Assert.That(actualServerBaseUri, Is.EqualTo(expectedVstsAccount));
        }

        [Test]
        public void TestProjects_ProjectIdentifierId_MatchesAppSettingsProjectId()
        {
            var appSettings = Substitute.For<IAppSettings>();
            var expectedProjectId = Guid.NewGuid();
            appSettings.VstsAccount.Returns("account");
            appSettings.ProjectId.Returns(expectedProjectId);
            var systemUnderTest = new MonitorSettings(appSettings);

            // ReSharper disable once PossibleNullReferenceException
            var actualProjectId = systemUnderTest.Projects.FirstOrDefault().Id;

            Assert.That(actualProjectId, Is.EqualTo(expectedProjectId));
        }

        [Test, TestCaseSource(nameof(pollIntervals))]
        public void TestPollInterval_Returns1000TimesAppSettingPollIntervalSeconds(int pollIntervalSeconds)
        {
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.PollIntervalSeconds.Returns(pollIntervalSeconds);
            var systemUnderTest = new MonitorSettings(appSettings);

            Assert.That(systemUnderTest.PollInterval, Is.EqualTo(1000 * pollIntervalSeconds));
        }

        [Test]
        public void TestSettingsChanged_FiresWhenAppSettingsEventIsReceived()
        {
            int numSettingsChangedEvents = 0;
            var appSettings = Substitute.For<IAppSettings>();
            var systemUnderTest = new MonitorSettings(appSettings);
            systemUnderTest.SettingsChanged += (sender, args) => numSettingsChangedEvents++;

            appSettings.SettingsChanged += Raise.Event();

            Assert.That(numSettingsChangedEvents, Is.EqualTo(1));
        }
    }
}