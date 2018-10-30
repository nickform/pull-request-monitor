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
                new SettingsViewModel(Substitute.For<IAppSettings>(), Substitute.For<ITfProjectCollectionCache>()),
                new AboutWindow(), Substitute.For<About>());

            Assert.That(systemUnderTest.ShowSettingsCommand, Is.Not.Null);
        }

        [Test]
        public void TestShowMonitorWindowCommand_IsNotNull()
        {
            var systemUnderTest = new ApplicationActions(Substitute.For<IMonitor>(), new MonitorWindow(),
                Substitute.For<IMonitorViewModelFactory>(), new SettingsWindow(),
                new SettingsViewModel(Substitute.For<IAppSettings>(), Substitute.For<ITfProjectCollectionCache>()),
                new AboutWindow(), Substitute.For<About>());

            Assert.That(systemUnderTest.ShowMonitorWindowCommand, Is.Not.Null);
        }

        [Test]
        public void TestShowAboutWindowCommand_IsNotNull()
        {
            var systemUnderTest = new ApplicationActions(Substitute.For<IMonitor>(), new MonitorWindow(),
                Substitute.For<IMonitorViewModelFactory>(), new SettingsWindow(),
                new SettingsViewModel(Substitute.For<IAppSettings>(), Substitute.For<ITfProjectCollectionCache>()),
                new AboutWindow(), Substitute.For<About>());

            Assert.That(systemUnderTest.ShowAboutWindowCommand, Is.Not.Null);
        }

        [Test]
        public void TestExitApplicationCommand_IsNotNull()
        {
            var systemUnderTest = new ApplicationActions(Substitute.For<IMonitor>(), new MonitorWindow(),
                Substitute.For<IMonitorViewModelFactory>(), new SettingsWindow(),
                new SettingsViewModel(Substitute.For<IAppSettings>(), Substitute.For<ITfProjectCollectionCache>()),
                new AboutWindow(), Substitute.For<About>());

            Assert.That(systemUnderTest.ExitApplicationCommand, Is.Not.Null);
        }
    }
}