using System.Threading;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Factories;
using PullRequestMonitor.Model;
using PullRequestMonitor.View;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ApplicationActionsTest
    {
        [Test]
        public void TestShowSettingsWindowCommand_IsNotNull()
        {
            var systemUnderTest = new ApplicationActions(Substitute.For<IMonitor>(), new MonitorWindow(),
                Substitute.For<IMonitorViewModelFactory>(), new SettingsWindow(),
                new SettingsViewModel(Substitute.For<IAppSettings>(), Substitute.For<ITfProjectCollectionCache>()));

            Assert.That(systemUnderTest.ShowSettingsCommand, Is.Not.Null);
        }

        [Test]
        public void TestShowMonitorWindowCommand_IsNotNull()
        {
            var systemUnderTest = new ApplicationActions(Substitute.For<IMonitor>(), new MonitorWindow(),
                Substitute.For<IMonitorViewModelFactory>(), new SettingsWindow(),
                new SettingsViewModel(Substitute.For<IAppSettings>(), Substitute.For<ITfProjectCollectionCache>()));

            Assert.That(systemUnderTest.ShowMonitorWindowCommand, Is.Not.Null);
        }

        [Test]
        public void TestExitApplicationCommand_IsNotNull()
        {
            var systemUnderTest = new ApplicationActions(Substitute.For<IMonitor>(), new MonitorWindow(),
                Substitute.For<IMonitorViewModelFactory>(), new SettingsWindow(),
                new SettingsViewModel(Substitute.For<IAppSettings>(), Substitute.For<ITfProjectCollectionCache>()));

            Assert.That(systemUnderTest.ExitApplicationCommand, Is.Not.Null);
        }
    }
}