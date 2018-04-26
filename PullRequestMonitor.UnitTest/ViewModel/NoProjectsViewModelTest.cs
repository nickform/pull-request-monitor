using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    [TestFixture]
    public class NoProjectsViewModelTest
    {
        [Test]
        public void TestShowSettingCommand_ReturnsApplicationActionsCommand()
        {
            var applicationActions = Substitute.For<IApplicationActions>();
            var delegateCommand = new DelegateCommand();
            applicationActions.ShowSettingsCommand.Returns(delegateCommand);
            var systemUnderTest = new NoProjectsViewModel(applicationActions);

            Assert.That(systemUnderTest.ShowSettingsCommand, Is.EqualTo(delegateCommand));
        }

        [Test]
        public void TestUpdate_DoesNothing()
        {
            var applicationActions = Substitute.For<IApplicationActions>();
            var systemUnderTest = new NoProjectsViewModel(applicationActions);

            systemUnderTest.Update();

            var temp1 = applicationActions.DidNotReceive().ExitApplicationCommand;
            var temp2 = applicationActions.DidNotReceive().ShowMonitorWindowCommand;
            var temp3 = applicationActions.DidNotReceive().ShowSettingsCommand;
            applicationActions.DidNotReceive().UpdateMonitorViewModel();
        }
    }
}