using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    [TestFixture]
    public class ProjectViewModelTest
    {
        [Test]
        public void TestConstructor_RegistersHandlerForRepositoriesUpdated()
        {
            var tfProject = Substitute.For<ITfProject>();
            var systemUnderTest = new ProjectViewModel(tfProject);

            tfProject.RepositoriesUpdated += Raise.Event();

            var projects = tfProject.Received().Repositories;
        }

        [Test]
        public void TestProjectName_ReturnsModelName()
        {
            var expectedName = "name of the project model object";
            var tfProject = Substitute.For<ITfProject>();
            tfProject.Name.Returns(expectedName);
            var systemUnderTest = new ProjectViewModel(tfProject);

            Assert.That(systemUnderTest.ProjectName, Is.EqualTo(expectedName));
        }
        [Test]
        public void TestId_ReturnsModelId()
        {
            var expectedId = Guid.NewGuid();
            var tfProject = Substitute.For<ITfProject>();
            tfProject.Id.Returns(expectedId);
            var systemUnderTest = new ProjectViewModel(tfProject);

            Assert.That(systemUnderTest.Id, Is.EqualTo(expectedId));
        }

        [Test]
        public void TestRepositories_BeforeFirstRefreshRepositories_IsNotNull()
        {
            var systemUnderTest = new ProjectViewModel(Substitute.For<ITfProject>());

            Assert.That(systemUnderTest.Repositories, Is.Not.Null);
        }

        [Test]
        public void TestRepositories_BeforeFirstRefreshRepositories_IsEmpty()
        {
            var systemUnderTest = new ProjectViewModel(Substitute.For<ITfProject>());

            Assert.That(systemUnderTest.Repositories, Is.Empty);
        }

        [Test]
        public void TestRepositories_AfterRepositoriesUpdated_ContainsOneViewModelPerModel()
        {
            var tfProject = Substitute.For<ITfProject>();
            var repositoryNames = new[] {"these", "names", "must", "be", "unique", "for", "this", "test", "to", "work"};
            var repositoryModels = new List<ITfGitRepository>();
            foreach (var repositoryName in repositoryNames)
            {
                var repoModel = Substitute.For<ITfGitRepository>();
                repoModel.Name.Returns(repositoryName);
                repositoryModels.Add(repoModel);
            }
            tfProject.Repositories.Returns(repositoryModels);
            var systemUnderTest = new ProjectViewModel(tfProject);

            tfProject.RepositoriesUpdated += Raise.Event();

            foreach (var repoModel in repositoryModels)
            {
                Assert.That(systemUnderTest.Repositories.Count(repoVm => repoVm.Name == repoModel.Name), Is.EqualTo(1));
            }
        }

        [Test]
        public void TestRepositories_AfterRepositoriesUpdated_ReturnsViewModelsSortedByRepoName()
        {
            var tfProject = Substitute.For<ITfProject>();
            var repositoryNames = new[] { "Aaron", "Shakespeare", "Ungerade", "Gerade", "X-ray", "Aardvark" };
            var repositoryModels = new List<ITfGitRepository>();
            foreach (var repositoryName in repositoryNames)
            {
                var repoModel = Substitute.For<ITfGitRepository>();
                repoModel.Name.Returns(repositoryName);
                repositoryModels.Add(repoModel);
            }
            tfProject.Repositories.Returns(repositoryModels);
            var systemUnderTest = new ProjectViewModel(tfProject);

            tfProject.RepositoriesUpdated += Raise.Event();

            var previousName = "";
            foreach (var repoName in systemUnderTest.Repositories.Select(repo => repo.Name))
            {
                Assert.That(repoName, Is.GreaterThan(previousName));
                previousName = repoName;
            }
        }

        [Test]
        public void TestRefreshRepositories_CallsModelRetrieveRepositories()
        {
            var tfProject = Substitute.For<ITfProject>();
            var systemUnderTest = new ProjectViewModel(tfProject);

            systemUnderTest.RefreshRepositories();

            tfProject.Received().RetrieveRepositories();
        }

        [Test]
        public void TestTypedEquals_WhenOtherIsNull_ReturnsFalse()
        {
            var firstProjectModel = Substitute.For<ITfProject>();
            firstProjectModel.Id.Returns(Guid.NewGuid());
            var systemUnderTest = new ProjectViewModel(firstProjectModel);

            Assert.That(systemUnderTest.Equals(null), Is.False);
        }

        [Test]
        public void TestTypedEquals_WhenOtherHasDifferentId_ReturnsFalse()
        {
            var firstProjectModel = Substitute.For<ITfProject>();
            firstProjectModel.Id.Returns(Guid.NewGuid());
            var systemUnderTest = new ProjectViewModel(firstProjectModel);
            var secondProjectModel = Substitute.For<ITfProject>();
            secondProjectModel.Id.Returns(Guid.NewGuid());
            var other = new ProjectViewModel(secondProjectModel);

            Assert.That(systemUnderTest.Equals(other), Is.False);
        }

        [Test]
        public void TestTypedEquals_WhenOtherHasSameId_ReturnsTrue()
        {
            var sharedGuid = Guid.NewGuid();
            var firstProjectModel = Substitute.For<ITfProject>();
            firstProjectModel.Id.Returns(sharedGuid);
            var systemUnderTest = new ProjectViewModel(firstProjectModel);
            var secondProjectModel = Substitute.For<ITfProject>();
            secondProjectModel.Id.Returns(sharedGuid);
            var other = new ProjectViewModel(secondProjectModel);

            Assert.That(systemUnderTest.Equals(other), Is.True);
        }

        [Test]
        public void TestUnyypedEquals_WhenOtherIsNull_ReturnsFalse()
        {
            var firstProjectModel = Substitute.For<ITfProject>();
            firstProjectModel.Id.Returns(Guid.NewGuid());
            var systemUnderTest = new ProjectViewModel(firstProjectModel);

            Assert.That(systemUnderTest.Equals(null as object), Is.False);
        }

        [Test]
        public void TestUnyypedEquals_WhenOtherIsThis_ReturnsTrue()
        {
            var firstProjectModel = Substitute.For<ITfProject>();
            firstProjectModel.Id.Returns(Guid.NewGuid());
            var systemUnderTest = new ProjectViewModel(firstProjectModel);

            Assert.That(systemUnderTest.Equals(systemUnderTest as object), Is.True);
        }

        [Test]
        public void TestUnyypedEquals_WhenOtherHasDifferentId_ReturnsFalse()
        {
            var firstProjectModel = Substitute.For<ITfProject>();
            firstProjectModel.Id.Returns(Guid.NewGuid());
            var systemUnderTest = new ProjectViewModel(firstProjectModel);
            var secondProjectModel = Substitute.For<ITfProject>();
            secondProjectModel.Id.Returns(Guid.NewGuid());
            var other = new ProjectViewModel(secondProjectModel);

            Assert.That(systemUnderTest.Equals(other as object), Is.False);
        }

        [Test]
        public void TestUntypedEquals_WhenOtherHasSameId_ReturnsTrue()
        {
            var sharedGuid = Guid.NewGuid();
            var firstProjectModel = Substitute.For<ITfProject>();
            firstProjectModel.Id.Returns(sharedGuid);
            var systemUnderTest = new ProjectViewModel(firstProjectModel);
            var secondProjectModel = Substitute.For<ITfProject>();
            secondProjectModel.Id.Returns(sharedGuid);
            var other = new ProjectViewModel(secondProjectModel);

            Assert.That(systemUnderTest.Equals(other as object), Is.True);
        }

        [Test]
        public void TestGetHashCode_ReturnsHashCodeOfModelId()
        {
            var modelId = Guid.NewGuid();
            var tfProject = Substitute.For<ITfProject>();
            tfProject.Id.Returns(modelId);
            var systemUnderTest = new ProjectViewModel(tfProject);

            Assert.That(systemUnderTest.GetHashCode(), Is.EqualTo(modelId.GetHashCode()));
        }
    }
}