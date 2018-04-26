using System;
using System.Globalization;
using System.IO.Packaging;
using System.Windows.Media.Imaging;
using NUnit.Framework;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    [TestFixture]
    public class CountToIconConverterTest
    {
        private CountToIconConverter _systemUnderTest;

        private static readonly object[] CountToUriCases = {
            new object[] {null, Unknown},
            new object[] {new object(), Unknown},
            new object[] {0, Zero},
            new object[] {1, One},
            new object[] {2, Two},
            new object[] {3, Three},
            new object[] {4, Four},
            new object[] {5, Five},
            new object[] {6, Six},
            new object[] {7, Seven},
            new object[] {8, Eight},
            new object[] {9, NinePlus},
            new object[] {100, NinePlus},
            new object[] {1000, NinePlus},
        };

        private const string Unknown = "pack://application:,,,/Resources/unknown.ico";
        private const string Zero = "pack://application:,,,/Resources/zero.ico";
        private const string One = "pack://application:,,,/Resources/one.ico";
        private const string Two = "pack://application:,,,/Resources/two.ico";
        private const string Three = "pack://application:,,,/Resources/three.ico";
        private const string Four = "pack://application:,,,/Resources/four.ico";
        private const string Five = "pack://application:,,,/Resources/five.ico";
        private const string Six = "pack://application:,,,/Resources/six.ico";
        private const string Seven = "pack://application:,,,/Resources/seven.ico";
        private const string Eight = "pack://application:,,,/Resources/eight.ico";
        private const string NinePlus = "pack://application:,,,/Resources/nineplus.ico";

        [SetUp]
        public void SetUp()
        {
            // Needed because http://stackoverflow.com/questions/6005398/uriformatexception-invalid-uri-invalid-port-specified...
            string s = PackUriHelper.UriSchemePack;
            _systemUnderTest = new CountToIconConverter();
        }

        [Test, TestCaseSource(nameof(CountToUriCases))]
        public void TestConvertCore(object valueToConvert, string expectedUri)
        {
            Assert.That(
                _systemUnderTest.ConvertCore(valueToConvert, typeof(BitmapImage), null, CultureInfo.CurrentCulture).AbsoluteUri,
                Is.EqualTo(expectedUri));
        }

        [Test]
        public void TestConvertBack_ThrowsNotImplementedException()
        {
            Assert.That(() => _systemUnderTest.ConvertBack(this, typeof(int?), null, CultureInfo.CurrentCulture),
                Throws.InstanceOf<NotImplementedException>());
        }
    }
}