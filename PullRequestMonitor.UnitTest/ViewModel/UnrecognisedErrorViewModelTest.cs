using NUnit.Framework;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    public class UnrecognisedErrorViewModelTest
    {
        [Test]
        public void TestUpdate_DoesNotCrash()
        {
            var systemUnderTest = new UnrecognisedErrorViewModel();

            systemUnderTest.Update();
        }
    }
}