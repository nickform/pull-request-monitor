using System;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Factories;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.UnitTest
{
    [TestFixture]
    public class TfsConnectionTest
    {
        private IPullRequestFactory _pullRequestFactory;
        private string _testUri;
        private ITfProjectFactory _tfProjectFactory;
        private ITfGitRepositoryFactory _tfGitRepositoryFactory;

        [SetUp]
        public void Setup()
        {
            _testUri = "http://a.test.uri";
            _pullRequestFactory = Substitute.For<IPullRequestFactory>();
            _tfProjectFactory = Substitute.For<ITfProjectFactory>();
            _tfGitRepositoryFactory = Substitute.For<ITfGitRepositoryFactory>();
        }

        [Test]
        public void TestReleasePullRequest_CallsFactoryRelease()
        {
            var systemUnderTest = new TfsConnection(new VssConnection(new Uri(_testUri), new VssBasicCredential()),
                _pullRequestFactory,
                _tfProjectFactory,
                _tfGitRepositoryFactory);
            var pullRequest = Substitute.For<IPullRequest>();

            systemUnderTest.ReleasePullRequest(pullRequest);

            _pullRequestFactory.Received().Release(pullRequest);
        }
    }
}