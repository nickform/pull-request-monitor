using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Factories;
using PullRequestMonitor.Model;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    [TestFixture]
    public class SettingsViewModelTest
    {
        [Test]
        public void TestAccountGetter_ReturnsAppSettingsAccount()
        {
            const string appSettingsAccount = "any-account";
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.Account.Returns(appSettingsAccount);
            var systemUnderTest = new SettingsViewModel(appSettings, Substitute.For<ITfProjectCollectionCache>());

            Assert.That(systemUnderTest.Account, Is.EqualTo(appSettingsAccount));
        }

        [Test]
        public void TestSelectedProjectGetter_WhenProjectIsNotInProjects_ReturnsNull()
        {
            var account = "fake-account";
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.Account.Returns(account);
            appSettings.ProjectId.Returns(Guid.NewGuid());
            var tpc = Substitute.For<ITfProjectCollection>();
            tpc.Projects.Returns(Enumerable.Empty<ITfProject>());
            var tpcCache = Substitute.For<ITfProjectCollectionCache>();
            tpcCache.GetProjectCollection(account).Returns(tpc);
            var systemUnderTest = new SettingsViewModel(appSettings, tpcCache);

            Assert.That(systemUnderTest.SelectedProject, Is.Null);
        }

        [Test]
        public void TestSelectedProjectSetter_WhenNewValueIsNull_SetsSettingsProjectIdToEmpty()
        {
            var currentProjectId = Guid.NewGuid();
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.ProjectId.Returns(currentProjectId);
            var systemUnderTest = new SettingsViewModel(appSettings, Substitute.For<ITfProjectCollectionCache>());

            systemUnderTest.SelectedProject = null;

            appSettings.Received().ProjectId = Guid.Empty;
        }

        [Test]
        public void TestSelectedProjectSetter_WhenNewValueHasDifferentProjectId_SetsAppSettingsProjectId()
        {
            var newValue = new ProjectViewModel(Substitute.For<ITfProject>());
            var newProjectId = Guid.NewGuid();
            newValue.Id.Returns(newProjectId);
            var appSettings = Substitute.For<IAppSettings>();
            var currentProjectId = Guid.NewGuid();
            appSettings.ProjectId.Returns(currentProjectId);
            var systemUnderTest = new SettingsViewModel(appSettings, Substitute.For<ITfProjectCollectionCache>());
            // This is a hack; no real client code ought to be able to add to Projects which should
            // refelect the underlying model...
            systemUnderTest.Projects.Add(newValue);

            systemUnderTest.SelectedProject = newValue;

            appSettings.Received().ProjectId = newProjectId;
        }

        [Test]
        public void TestSelectedProjectSetter_WhenNewValueHasDifferentProjectId_RaisesPropertyChanged()
        {
            var numSelectedProjectChangedNotifications = 0;
            var newValue = new ProjectViewModel(Substitute.For<ITfProject>());
            var newProjectId = Guid.NewGuid();
            newValue.Id.Returns(newProjectId);
            var appSettings = Substitute.For<IAppSettings>();
            var currentProjectId = Guid.NewGuid();
            appSettings.ProjectId.Returns(currentProjectId);
            var systemUnderTest = new SettingsViewModel(appSettings, Substitute.For<ITfProjectCollectionCache>());
            // This is a hack...
            systemUnderTest.Projects.Add(newValue);
            systemUnderTest.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SettingsViewModel.SelectedProject))
                {
                    numSelectedProjectChangedNotifications++;
                }
            };

            systemUnderTest.SelectedProject = newValue;

            Assert.That(numSelectedProjectChangedNotifications, Is.EqualTo(1));
        }

        [Test]
        public void TestSelectedProjectSetter_WhenNewValueHasDifferentProjectId_CallsRefreshRepositoriesOnSelectedProject()
        {
            var projectModel = Substitute.For<ITfProject>();
            var newValue = new ProjectViewModel(projectModel);
            var newProjectId = Guid.NewGuid();
            newValue.Id.Returns(newProjectId);
            var appSettings = Substitute.For<IAppSettings>();
            var currentProjectId = Guid.NewGuid();
            appSettings.ProjectId.Returns(currentProjectId);
            var systemUnderTest = new SettingsViewModel(appSettings, Substitute.For<ITfProjectCollectionCache>());
            // This is a hack...
            systemUnderTest.Projects.Add(newValue);

            systemUnderTest.SelectedProject = newValue;

            var repos = projectModel.Received().RetrieveRepositories();
        }

        [Test]
        public void TestRepoNamePatternGetter_ReturnsModelRepoNamePattern()
        {
            var expectedPattern = "appname-*";
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.RepoNamePattern.Returns(expectedPattern);
            var systemUnderTest = new SettingsViewModel(appSettings, Substitute.For<ITfProjectCollectionCache>());

            Assert.That(systemUnderTest.RepoNamePattern, Is.EqualTo(expectedPattern));
        }

        [Test]
        public void TestRepoNamePatternSetter_SetsValueOnAppSettings()
        {
            var expectedPattern = "appname-*";
            var appSettings = Substitute.For<IAppSettings>();
            var systemUnderTest = new SettingsViewModel(appSettings, Substitute.For<ITfProjectCollectionCache>());

            systemUnderTest.RepoNamePattern = expectedPattern;

            appSettings.Received().RepoNamePattern = expectedPattern;
        }

        [Test]
        public void TestRepoNamePatternSetter_RaisesPropertyChangedForRepoNamePattern()
        {
            var numPropertyChangedCalls = 0;
            var expectedPattern = "appname-*";
            var appSettings = Substitute.For<IAppSettings>();
            var systemUnderTest = new SettingsViewModel(appSettings, Substitute.For<ITfProjectCollectionCache>());
            systemUnderTest.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SettingsViewModel.RepoNamePattern))
                    numPropertyChangedCalls++;
            };

            systemUnderTest.RepoNamePattern = expectedPattern;

            Assert.That(numPropertyChangedCalls, Is.EqualTo(1));
        }

        [Test]
        public void TestRefreshProjects_WhenServerIsRefreshing_DoesNotCallTpcRetrieveProjects()
        {
            const string serverUri = "https//Not.A.Server";
            var tpc = Substitute.For<ITfProjectCollection>();
            tpc.ProjectRetrievalStatus.Returns(RetrievalStatus.Ongoing);
            var tpcCache = Substitute.For<ITfProjectCollectionCache>();
            tpcCache.GetProjectCollection(serverUri).Returns(tpc);
            var systemUnderTest = new SettingsViewModel(Substitute.For<IAppSettings>(), tpcCache);

            systemUnderTest.RefreshProjects();

            tpc.DidNotReceive().RetrieveProjects();
        }

        [Test]
        public async Task TestRefreshProjects_CallsTpcRetrieveProjects()
        {
            const string account = "say-what";
            var tpc = Substitute.For<ITfProjectCollection>();
            tpc.ProjectRetrievalStatus.Returns(RetrievalStatus.Unstarted);
            var tpcCache = Substitute.For<ITfProjectCollectionCache>();
            tpcCache.GetProjectCollection(account).Returns(tpc);
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.Account.Returns(account);
            var systemUnderTest = new SettingsViewModel(appSettings, tpcCache);

            systemUnderTest.RefreshProjects();

            await tpc.Received().RetrieveProjects();
        }

        public static object[] UpdateProjectViewModelsCases
        {
            get
            {
                var alphabet = BuildAnITfProject("Alphabet");
                var apocalypse = BuildAnITfProject("Apocalypse");
                var byzantine = BuildAnITfProject("byzantine");
                var puddle = BuildAnITfProject("puddle");
                var zebra = BuildAnITfProject("Zebra");
                var zygote = BuildAnITfProject("Zygote");

                // Test the case when the view model collection is initially empty...
                var fromEmptyModels = new List<ITfProject>
                {
                    zygote, alphabet, zebra
                };
                var fromEmptyToUpdate = new ObservableCollection<ProjectViewModel>();
                var fromEmptyExpectedCollection = new ObservableCollection<ProjectViewModel>
                {
                    new ProjectViewModel(alphabet), new ProjectViewModel(zebra), new ProjectViewModel(zygote)
                };
                var fromEmptyTestCase = new object[]
                {
                    "from empty", fromEmptyModels, fromEmptyToUpdate, fromEmptyExpectedCollection
                };

                // Test the case when none of the existing view models match items in the new list of models...
                var completeReplacementModels = new List<ITfProject>
                {
                    zygote, zebra, puddle
                };
                var completeReplacementToUpdate = new ObservableCollection<ProjectViewModel>
                {
                    new ProjectViewModel(alphabet), new ProjectViewModel(apocalypse), new ProjectViewModel(byzantine)
                };
                var completeReplacementExpectedCollection = new ObservableCollection<ProjectViewModel>
                {
                    new ProjectViewModel(puddle), new ProjectViewModel(zebra), new ProjectViewModel(zygote)
                };
                var completeReplacementTestCase = new object[]
                {
                    "complete replacement", completeReplacementModels, completeReplacementToUpdate, completeReplacementExpectedCollection
                };

                // Test the case when none of the existing view models match items in the new list of models...
                var completeReplacementTwoModels = new List<ITfProject>
                {
                    alphabet, byzantine, apocalypse
                };
                var completeReplacementTwoToUpdate = new ObservableCollection<ProjectViewModel>
                {
                    new ProjectViewModel(puddle), new ProjectViewModel(zebra), new ProjectViewModel(zygote)
                };
                var completeReplacementTwoExpectedCollection = new ObservableCollection<ProjectViewModel>
                {
                    new ProjectViewModel(alphabet), new ProjectViewModel(apocalypse), new ProjectViewModel(byzantine)
                };
                var completeReplacementTwoTestCase = new object[]
                {
                    "complete replacement two", completeReplacementTwoModels, completeReplacementTwoToUpdate, completeReplacementTwoExpectedCollection
                };

                // Test the case when one new model should be added in an intermediate position...
                var intermediateInsertionModels = new List<ITfProject>
                {
                    alphabet, byzantine, puddle, zygote
                };
                var intermediateInsertionToUpdate = new ObservableCollection<ProjectViewModel>
                {
                    new ProjectViewModel(alphabet), new ProjectViewModel(byzantine), new ProjectViewModel(zygote)
                };
                var intermediateInsertionExpectedCollection = new ObservableCollection<ProjectViewModel>
                {
                    new ProjectViewModel(alphabet), new ProjectViewModel(byzantine), new ProjectViewModel(puddle), new ProjectViewModel(zygote)
                };
                var intermediateInsertionTestCase = new object[]
                {
                    "intermediate insertion", intermediateInsertionModels, intermediateInsertionToUpdate, intermediateInsertionExpectedCollection
                };

                // Test the case when one existing view model should be remove from an intermediate position...
                var intermediateDeletionModels = new List<ITfProject>
                {
                    alphabet, byzantine, zygote
                };
                var intermediateDeletionToUpdate = new ObservableCollection<ProjectViewModel>
                {
                    new ProjectViewModel(alphabet), new ProjectViewModel(byzantine), new ProjectViewModel(puddle), new ProjectViewModel(zygote)
                };
                var intermediateDeletionExpectedCollection = new ObservableCollection<ProjectViewModel>
                {
                    new ProjectViewModel(alphabet), new ProjectViewModel(byzantine), new ProjectViewModel(zygote)
                };
                var intermediateDeletionTestCase = new object[]
                {
                    "intermediate deletion", intermediateDeletionModels, intermediateDeletionToUpdate, intermediateDeletionExpectedCollection
                };


                return new object[]
                {
                    fromEmptyTestCase,
                    completeReplacementTestCase,
                    completeReplacementTwoTestCase,
                    intermediateInsertionTestCase,
                    intermediateDeletionTestCase
                };
            }
        }

        private static ITfProject BuildAnITfProject(string projectName)
        {
            var project = Substitute.For<ITfProject>();
            project.Id.Returns(Guid.NewGuid());
            project.Name.Returns(projectName);
            return project;
        }

        [Test, TestCaseSource(nameof(UpdateProjectViewModelsCases))]
        public void TestUpdateProjectViewModels_PassesAllTestCases(string testCaseName, List<ITfProject> models,
            ObservableCollection<ProjectViewModel> toUpdate, ObservableCollection<ProjectViewModel> expectedCollection)
        {
            SettingsViewModel.UpdateProjectViewModels(models, toUpdate);

            CollectionAssert.AreEqual(expectedCollection, toUpdate, new ProjectViewModelComparer(), testCaseName);
        }
    }

    class ProjectViewModelComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            var xViewModel = x as ProjectViewModel;
            var yViewModel = y as ProjectViewModel;

            if (xViewModel == null || yViewModel == null)
                throw new InvalidOperationException($"Can only compare {nameof(ProjectViewModel)}s");

            return String.CompareOrdinal(xViewModel.Id.ToString(), yViewModel.Id.ToString());
        }
    }
}