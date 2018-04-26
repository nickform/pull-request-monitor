using System;
using NUnit.Framework;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.UnitTest.Model
{
    [TestFixture]
    class MonitoredProjectSettingsTest
    {
        [Test]
        public void TestCanSetAndGetVstsAccount()
        {
            var systemUnderTest = new MonitoredProjectSettings();
            Assert.That(systemUnderTest.VstsAccount, Is.Null);

            var testAccount = "madeup";
            systemUnderTest.VstsAccount = testAccount;

            Assert.That(systemUnderTest.VstsAccount, Is.EqualTo(testAccount));
        }

        [Test]
        public void TestCanSetAndGetProjectGuid()
        {
            var systemUnderTest = new MonitoredProjectSettings();
            Assert.That(systemUnderTest.Id, Is.EqualTo(Guid.Empty));

            var testProjectGuid = Guid.NewGuid();
            systemUnderTest.Id = testProjectGuid;

            Assert.That(systemUnderTest.Id, Is.EqualTo(testProjectGuid));
        }

        [Test]
        public void TestRepoNameRegexp_ForNewInstance_IsNull()
        {
            var systemUnderTest = new MonitoredProjectSettings();

            Assert.That(systemUnderTest.RepoNameRegexp, Is.Null);
        }

        [Test]
        public void TestCanSetAndGetRepoNameRegexp()
        {
            var systemUnderTest = new MonitoredProjectSettings();

            var repoNameRegexp = "a_pattern";
            systemUnderTest.RepoNameRegexp = repoNameRegexp;

            Assert.That(systemUnderTest.RepoNameRegexp, Is.EqualTo(repoNameRegexp));
        }

        [Test]
        public void TestEquals_WhenOtherValueIsNull_ReturnsFalse()
        {
            var systemUnderTest = new MonitoredProjectSettings();

            Assert.That(systemUnderTest.Equals(null), Is.False);
        }

        [Test]
        public void TestEquals_WhenOtherValueIsThis_ReturnsTrue()
        {
            var systemUnderTest = new MonitoredProjectSettings();

            // ReSharper disable once EqualExpressionComparison
            Assert.That(systemUnderTest.Equals(systemUnderTest), Is.True);
        }

        [Test]
        public void TestEquals_WhenOtherValueIsDifferentType_ReturnsFalse()
        {
            var systemUnderTest = new MonitoredProjectSettings();

            // ReSharper disable once SuspiciousTypeConversion.Global
            Assert.That(systemUnderTest.Equals(this), Is.False);
        }

        [Test]
        public void TestEquals_WhenVstsAccountsAreDifferent_ReturnsFalse()
        {
            var systemUnderTest1 = new MonitoredProjectSettings();
            var systemUnderTest2 = new MonitoredProjectSettings();

            systemUnderTest1.VstsAccount = "anything";
            systemUnderTest2.VstsAccount = "anything-else";
            systemUnderTest1.Id = systemUnderTest2.Id = Guid.NewGuid();

            Assert.That(systemUnderTest1.Equals(systemUnderTest2), Is.False);
        }

        [Test]
        public void TestEquals_WhenVstsAccountsAreSame_ReturnsTrue()
        {
            var systemUnderTest1 = new MonitoredProjectSettings();
            var systemUnderTest2 = new MonitoredProjectSettings();

            systemUnderTest1.VstsAccount = "anything";
            systemUnderTest2.VstsAccount = "anything";
            systemUnderTest1.Id = systemUnderTest2.Id = Guid.NewGuid();

            Assert.That(systemUnderTest1.Equals(systemUnderTest2), Is.True);
        }

        [Test]
        public void TestEquals_WhenProjectGuidsAreDifferent_ReturnsFalse()
        {
            var systemUnderTest1 = new MonitoredProjectSettings();
            var systemUnderTest2 = new MonitoredProjectSettings();

            systemUnderTest1.VstsAccount = systemUnderTest2.VstsAccount = "anything";
            systemUnderTest1.Id = Guid.NewGuid();
            systemUnderTest2.Id = Guid.NewGuid();

            Assert.That(systemUnderTest1.Equals(systemUnderTest2), Is.False);
        }

        [Test]
        public void TestEquals_WhenProjectGuidsAreSame_ReturnsTrue()
        {
            const string testGuidString = "84ACAF79-CA75-461C-B6EE-6DBAE5B93792";
            var systemUnderTest1 = new MonitoredProjectSettings();
            var systemUnderTest2 = new MonitoredProjectSettings();

            systemUnderTest1.VstsAccount = systemUnderTest2.VstsAccount = "anything";
            systemUnderTest1.Id = Guid.Parse(testGuidString);
            systemUnderTest2.Id = Guid.Parse(testGuidString);

            Assert.That(systemUnderTest1.Equals(systemUnderTest2), Is.True);
        }

    }
}
