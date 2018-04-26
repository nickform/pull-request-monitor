using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.UnitTest.Model
{
    public class NameRegexpRepositoryFilterTest
    {
        public static object[] IsIncludedTestCases =
        {
            new object[] {".*", "repo-name", true},
            new object[] {".*", "literally anything", true},
            new object[] {"repo-naim", "repo-name", false},
            new object[] {"repo-$", "repo-name", false},
            new object[] {"repo-.*", "repo-name", true},
            new object[] {"(app1|app2)-.*", "app1-repo1", true},
            new object[] {"(app1|app2)-.*", "app2-repo1", true},
            new object[] {"(app1|app2)-.*", "app3-repo1", false},
            new object[] {"app-[^d].*", "app-docs", false},
            new object[] {"app-[^d].*", "app-loggingService", true},
            new object[] {"app-[^d].*", "app-persistenceService", true}
        };

        [Test, TestCaseSource(nameof(IsIncludedTestCases))]
        public void TestIncludesRepo_ForRepoNameWithoutDotGit_ReturnsExpectedResults(string pattern, string repositoryName, bool expectedResult)
        {
            var systemUnderTest = new NameRegexpRepositoryFilter(pattern);
            var repo = Substitute.For<ITfGitRepository>();
            repo.Name.Returns(repositoryName);

            Assert.That(systemUnderTest.IncludesRepo(repo), Is.EqualTo(expectedResult));
        }

        [Test, TestCaseSource(nameof(IsIncludedTestCases))]
        public void TestIncludesRepo_ForRepoNameWithDotGit_ReturnsExpectedResults(string pattern, string repositoryName, bool expectedResult)
        {
            var systemUnderTest = new NameRegexpRepositoryFilter(pattern);
            var repo = Substitute.For<ITfGitRepository>();
            repo.Name.Returns(repositoryName + ".git");

            Assert.That(systemUnderTest.IncludesRepo(repo), Is.EqualTo(expectedResult));
        }

        [Test]
        public void Test()
        {
            Assert.True(Regex.IsMatch("repo-name", "repo-"));
        }
    }
}