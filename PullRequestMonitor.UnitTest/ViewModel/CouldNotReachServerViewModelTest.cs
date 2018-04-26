using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    public class CouldNotReachServerViewModelTest
    {
        [Test]
        public void TestUpdate_DoesNotCrash()
        {
            var systemUnderTest = new CouldNotReachServerViewModel(Substitute.For<IApplicationActions>(), Substitute.For<IAppSettings>());

            systemUnderTest.Update();
        }

        [Test]
        public void TestServerWebViewUrl_ReturnsSettingsVstsUrl()
        {
            var testVstsAccount = "my-account";
            var exptectedServerWebViewUrl = VstsServerURL.GetVstsServerURL(testVstsAccount);
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.VstsAccount.Returns(testVstsAccount);
            var systemUnderTest = new CouldNotReachServerViewModel(Substitute.For<IApplicationActions>(), appSettings);

            Assert.That(systemUnderTest.ServerWebViewUrl, Is.EqualTo(exptectedServerWebViewUrl));
        }

        [Test]
        public void TestShowSettingsCommand_ReturnsApplicationActionsShowSettingsCommand()
        {
            var applicationActions = Substitute.For<IApplicationActions>();
            var testCommand = new DelegateCommand();
            applicationActions.ShowSettingsCommand.Returns(testCommand);
            var systemUnderTest = new CouldNotReachServerViewModel(applicationActions, Substitute.For<IAppSettings>());

            Assert.That(systemUnderTest.ShowSettingsCommand, Is.EqualTo(testCommand));
        }
    }
}