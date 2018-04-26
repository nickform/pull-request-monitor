using Microsoft.TeamFoundation.SourceControl.WebApi;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.UnitTest.Model
{
    [TestFixture]
    public class TfGitRepositoryTest
    {
        [Test]
        public void TestNameGetter_ReturnsNameOfRepoPassedToConstructor()
        {
            const string expectedName = "Any name would do";
            var gitRepository = new GitRepository{Name = expectedName};
            var systemUnderTest = new TfGitRepository(gitRepository, Substitute.For<ITfProject>());

            Assert.That(systemUnderTest.Name, Is.EqualTo(expectedName));
        }

        [Test]
        public void TestProjectGetter_ReturnsProjectPassedToConstructor()
        {
            var project = Substitute.For<ITfProject>();
            var systemUnderTest = new TfGitRepository(new GitRepository(), project);

            Assert.That(systemUnderTest.Project, Is.EqualTo(project));
        }
    }
}