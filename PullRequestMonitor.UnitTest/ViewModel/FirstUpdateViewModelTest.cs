using NUnit.Framework;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    public class FirstUpdateViewModelTest
    {
        [Test]
        public void TestUpdate_DoesNotCrash()
        {
            var systemUnderTest = new FirstUpdateViewModel();

            systemUnderTest.Update();
        }
    }
}