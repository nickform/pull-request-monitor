using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    [TestFixture]
    public class RepositoryViewModelTest
    {
        [Test]
        public void TestNameGetter_ReturnsModelName()
        {
            const string testName = "Any name here";
            var repository = Substitute.For<ITfGitRepository>();
            repository.Name.Returns(testName);
            var systemUnderTest = new RepositoryViewModel(repository);

            Assert.That(systemUnderTest.Name, Is.EqualTo(testName));
        }
    }
}