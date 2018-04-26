using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using PullRequestMonitor.Factories;
using PullRequestMonitor.Model;
using PullRequestMonitor.Services;
using PullRequestMonitor.UnitTest.Exceptions;

namespace PullRequestMonitor.UnitTest.Model
{
    public class TfProjectCollectionTest
    {
        public static Exception[] CouldNotReachServerExceptions = CommonExceptionExamples.CouldNotReachServerExceptions;
        public static Exception[] UnauthorisedExceptions = CommonExceptionExamples.UnauthorisedExceptions;
        public static Exception[] UnrecognisedExceptions = CommonExceptionExamples.UnrecognisedExceptions;

        public static Exception[] AllExceptions
        {
            get
            {
                var allExceptions = new List<Exception>();
                allExceptions.AddRange(CouldNotReachServerExceptions);
                allExceptions.AddRange(UnauthorisedExceptions);
                allExceptions.AddRange(UnrecognisedExceptions);
                return allExceptions.ToArray();
            }
        }

        private string _testUri;
        private ITfsConnectionFactory _connectionFactory;
        private ILogger _logger;

        [SetUp]
        public void Setup()
        {
            _testUri = "http://a.test.uri";
            _connectionFactory = Substitute.For<ITfsConnectionFactory>();
            _logger = Substitute.For<ILogger>();
        }

        [Test]
        public void TestConstructor_SetsRetrievalStatusToUnstarted()
        {
            var systemUnderTest = new TfProjectCollection(_testUri, _connectionFactory, _logger);

            Assert.That(systemUnderTest.ProjectRetrievalStatus, Is.EqualTo(RetrievalStatus.Unstarted));
        }

        [Test]
        public void TestProjects_ImmediatelyAfterConstruction_IsNotNull()
        {
            var systemUnderTest = new TfProjectCollection(_testUri, _connectionFactory, _logger);

            Assert.That(systemUnderTest.Projects, Is.Not.Null);
        }

        [Test]
        public async Task TestRetrieveProjects_WhenStatusIsUnstarted_SetsStatusToRetrieving()
        {
            var tfsConnection = Substitute.For<ITfsConnection>();
            _connectionFactory.Create(_testUri).Returns(tfsConnection);
            var systemUnderTest = new TfProjectCollection(_testUri, _connectionFactory, _logger);
            tfsConnection.GetProjects().Returns(info =>
            {
                Assert.That(systemUnderTest.ProjectRetrievalStatus, Is.EqualTo(RetrievalStatus.Ongoing));
                return Enumerable.Empty<ITfProject>();
            });

            await systemUnderTest.RetrieveProjects();
        }

        [Test]
        public async Task TestRetrieveProjects_WhenStatusIsUnstarted_GetsProjectsFromConnection()
        {
            var tfsConnection = Substitute.For<ITfsConnection>();
            _connectionFactory.Create(_testUri).Returns(tfsConnection);
            var systemUnderTest = new TfProjectCollection(_testUri, _connectionFactory, _logger);
            var firstProject = Substitute.For<ITfProject>();
            firstProject.Id.Returns(Guid.NewGuid());
            var secondProject = Substitute.For<ITfProject>();
            secondProject.Id.Returns(Guid.NewGuid());
            var projects = new[] {firstProject, secondProject};
            tfsConnection.GetProjects().Returns(projects);

            await systemUnderTest.RetrieveProjects();

            foreach (var tfProject in projects)
            {
                Assert.That(systemUnderTest.Projects, Contains.Item(tfProject));
            }
        }

        [Test]
        public async Task TestRetrieveProjects_WhenGettingProjectsSucceeds_SetsStatusToSucceeded()
        {
            var tfsConnection = Substitute.For<ITfsConnection>();
            _connectionFactory.Create(_testUri).Returns(tfsConnection);
            var systemUnderTest = new TfProjectCollection(_testUri, _connectionFactory, _logger);
            var firstProject = Substitute.For<ITfProject>();
            firstProject.Id.Returns(Guid.NewGuid());
            var projects = new[] { firstProject };
            tfsConnection.GetProjects().Returns(projects);

            await systemUnderTest.RetrieveProjects();

            Assert.That(systemUnderTest.ProjectRetrievalStatus, Is.EqualTo(RetrievalStatus.Suceeded));
        }

        [Test, TestCaseSource(nameof(CouldNotReachServerExceptions))]
        public async Task TestRetrieveProjects_WhenGettingProjectsFailsDueToConnection_SetsStatusToFailedDueToConnection(Exception exception)
        {
            var tfsConnection = Substitute.For<ITfsConnection>();
            _connectionFactory.Create(_testUri).Returns(tfsConnection);
            var systemUnderTest = new TfProjectCollection(_testUri, _connectionFactory, _logger);
            tfsConnection.GetProjects().Throws(callInfo => exception);

            await systemUnderTest.RetrieveProjects();

            Assert.That(systemUnderTest.ProjectRetrievalStatus, Is.EqualTo(RetrievalStatus.FailedDueToConnection));
        }

        [Test, TestCaseSource(nameof(UnauthorisedExceptions))]
        public async Task TestRetrieveProjects_WhenGettingProjectsFailsDueToAuth_SetsStatusToFailedDueToAuth(Exception exception)
        {
            var tfsConnection = Substitute.For<ITfsConnection>();
            _connectionFactory.Create(_testUri).Returns(tfsConnection);
            var systemUnderTest = new TfProjectCollection(_testUri, _connectionFactory, _logger);
            tfsConnection.GetProjects().Throws(callInfo => exception);

            await systemUnderTest.RetrieveProjects();

            Assert.That(systemUnderTest.ProjectRetrievalStatus, Is.EqualTo(RetrievalStatus.FailedDueToAuth));
        }

        [Test, TestCaseSource(nameof(UnrecognisedExceptions))]
        public async Task TestRetrieveProjects_WhenGettingProjectsFailsForUnknownReason_SetsStatusToFailedReasonUnknown(Exception exception)
        {
            var tfsConnection = Substitute.For<ITfsConnection>();
            _connectionFactory.Create(_testUri).Returns(tfsConnection);
            var systemUnderTest = new TfProjectCollection(_testUri, _connectionFactory, _logger);
            tfsConnection.GetProjects().Throws(callInfo => exception);

            await systemUnderTest.RetrieveProjects();

            Assert.That(systemUnderTest.ProjectRetrievalStatus, Is.EqualTo(RetrievalStatus.FailedReasonUnknown));
        }

        [Test]
        public void TestGetProject_WhenStatusIsUnstarted_ThrowsInvalidOperation()
        {
            var systemUnderTest = new TfProjectCollection(_testUri, _connectionFactory, _logger);

            Assert.That(() => systemUnderTest.GetProject(Arg.Any<Guid>()), Throws.InvalidOperationException);
        }

        [Test]
        public async Task TestGetProject_WhenStatusIsSucceededButProjectIsAbsent_ReturnsNull()
        {
            var connection = Substitute.For<ITfsConnection>();
            connection.GetProjects().Returns(Enumerable.Empty<ITfProject>());
            _connectionFactory.Create(_testUri).Returns(connection);
            var systemUnderTest = new TfProjectCollection(_testUri, _connectionFactory, _logger);

            await systemUnderTest.RetrieveProjects();

            Assert.That(systemUnderTest.GetProject(Guid.NewGuid()), Is.Null);
        }

        [Test]
        public async Task TestGetProject_WhenStatusIsSucceededAndProjectIsPresent_ReturnsProject()
        {
            var connection = Substitute.For<ITfsConnection>();
            var expectedProject = Substitute.For<ITfProject>();
            var projectId = Guid.NewGuid();
            expectedProject.Id.Returns(projectId);
            var projects = new [] { expectedProject};
            connection.GetProjects().Returns(projects);
            _connectionFactory.Create(_testUri).Returns(connection);
            var systemUnderTest = new TfProjectCollection(_testUri, _connectionFactory, _logger);
            await systemUnderTest.RetrieveProjects();

            var actualProject = systemUnderTest.GetProject(projectId);

            Assert.That(actualProject, Is.EqualTo(expectedProject));
        }
    }
}