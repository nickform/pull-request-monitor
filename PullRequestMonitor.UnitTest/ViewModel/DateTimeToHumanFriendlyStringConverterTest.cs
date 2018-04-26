using System;
using System.Globalization;
using NUnit.Framework;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    [TestFixture]
    public class DateTimeToHumanFriendlyStringConverterTest
    {
        private readonly DateTimeToHumanFriendlyStringConverter _systemUnderTest = new DateTimeToHumanFriendlyStringConverter();

        [Test]
        public void TestConvert_WhenValueIsNotADateTime_ReturnsNull()
        {
            Assert.That(_systemUnderTest.Convert(this, typeof(string), null, CultureInfo.CurrentCulture), Is.Null);
        }

        private static readonly object[] TestCases =
        {
            new object[] {TimeSpan.FromDays(14), "14 days ago"},
            new object[] {TimeSpan.FromDays(1), "yesterday"},
            new object[] {TimeSpan.FromHours(10), "10 hours ago"},
            new object[] {TimeSpan.FromMinutes(36), "36 minutes ago"},
            new object[] {TimeSpan.FromSeconds(53), "53 seconds ago"}
        };

        [Test, TestCaseSource(nameof(TestCases))]
        public void TestConvert_WithValidArguments_ReturnsExpectedStrings(TimeSpan age, string expectedString)
        {
            var createdDateTime = DateTime.UtcNow - age;
            Assert.That(
                _systemUnderTest.Convert(createdDateTime, typeof(string),
                null, CultureInfo.CurrentCulture),
                Is.EqualTo(expectedString));
        }

        [Test]
        public void TestConvertBack_ThrowsNotImplementedException()
        {
            Assert.That(() => _systemUnderTest.ConvertBack(this, typeof(DateTime), null, CultureInfo.CurrentCulture),
                Throws.InstanceOf<NotImplementedException>());
        }
    }
}