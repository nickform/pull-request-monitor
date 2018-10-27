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
        public void TestProjects_WhenAppSettingsAccountIsEmpty_IsEmpty()
        {
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.Account.Returns("");
            appSettings.ProjectId.Returns(Guid.NewGuid());
            var systemUnderTest = new MonitorSettings(appSettings);

            Assert.That(systemUnderTest.Projects, Is.Empty);
        }

        [Test]
        public void TestProjects_WhenAppSettingsProjectIdIsEmptyGuid_IsEmpty()
        {
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.Account.Returns("not-empty");
            appSettings.ProjectId.Returns(Guid.Empty);
            var systemUnderTest = new MonitorSettings(appSettings);

            Assert.That(systemUnderTest.Projects, Is.Empty);
        }

        [Test]
        public void TestProjects_WhenAppSettingsProjectIdIsNotEmptyGuid_ContainsSingleProjectIdentifier()
        {
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.Account.Returns("not-an-account");
            appSettings.ProjectId.Returns(Guid.NewGuid());
            var systemUnderTest = new MonitorSettings(appSettings);

            Assert.That(systemUnderTest.Projects.Count(), Is.EqualTo(1));
        }

        [Test]
        public void TestProjects_ProjectIdentifierAccount_MatchesAppSettingsAccount()
        {
            var appSettings = Substitute.For<IAppSettings>();
            var expectedAccount = "this-would-never-be-right-by-accident";
            appSettings.Account.Returns(expectedAccount);
            appSettings.ProjectId.Returns(Guid.NewGuid());
            var systemUnderTest = new MonitorSettings(appSettings);

            // ReSharper disable once PossibleNullReferenceException
            var actualServerBaseUri = systemUnderTest.Projects.FirstOrDefault().Account;

            Assert.That(actualServerBaseUri, Is.EqualTo(expectedAccount));
        }

        [Test]
        public void TestProjects_ProjectIdentifierId_MatchesAppSettingsProjectId()
        {
            var appSettings = Substitute.For<IAppSettings>();
            var expectedProjectId = Guid.NewGuid();
            appSettings.Account.Returns("account");
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