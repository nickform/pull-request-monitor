﻿using System.Linq;
using NSubstitute;
using NUnit.Framework;
using PullRequestMonitor.Factories;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.UnitTest.Factories
{
    [TestFixture]
    public class TfProjectCollectionCacheTest
    {
        [Test]
        public void TestGetServer_ForEmptyAccount_ReturnsNullObject()
        {
            var tpcFactory = Substitute.For<ITfProjectCollectionFactory>();
            var systemUnderTest = new TfProjectCollectionCache(tpcFactory);

            var tpc = systemUnderTest.GetProjectCollection("");

            Assert.That(tpc, Is.InstanceOf<NullTeamProjectCollection>());
            tpcFactory.DidNotReceive().Create(Arg.Any<string>());
        }
        [Test]
        public void TestGetServer_ForAccountNotInCache_CallsServerFactoryWithUri()
        {
            const string account = "pullrequestmonitor";
            var serverUri = ServerUrl.GetServerURL(account);
            var tpcFactory = Substitute.For<ITfProjectCollectionFactory>();
            var systemUnderTest = new TfProjectCollectionCache(tpcFactory);

            systemUnderTest.GetProjectCollection(account);

            tpcFactory.Received().Create(serverUri);
        }

        [Test]
        public void TestGetServer_ForAccountInCache_DoesNotCallServerFactory()
        {
            const string account = "test-account";
            var expectedServerUrl = ServerUrl.GetServerURL(account);
            var tpcFactory = Substitute.For<ITfProjectCollectionFactory>();
            var systemUnderTest = new TfProjectCollectionCache(tpcFactory);
            systemUnderTest.GetProjectCollection(account);
            // Check that the factory was called once by now...
            tpcFactory.Received().Create(expectedServerUrl);
            Assert.That(tpcFactory.ReceivedCalls().Count(), Is.EqualTo(1));

            systemUnderTest.GetProjectCollection(account);

            Assert.That(tpcFactory.ReceivedCalls().Count(), Is.EqualTo(1));
        }

        [Test]
        public void TestGetServer_ForAccountInCache_ReturnsCachedValue()
        {
            const string account = "omnipave";
            var serverUri = ServerUrl.GetServerURL(account);
            var tpcFactory = Substitute.For<ITfProjectCollectionFactory>();
            var expected = Substitute.For<ITfProjectCollection>();
            tpcFactory.Create(serverUri).Returns(expected);
            var systemUnderTest = new TfProjectCollectionCache(tpcFactory);
            systemUnderTest.GetProjectCollection(account);
            // Check that the factory was called once by now...
            tpcFactory.Received().Create(serverUri);
            Assert.That(tpcFactory.ReceivedCalls().Count(), Is.EqualTo(1));

            var actual = systemUnderTest.GetProjectCollection(account);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}