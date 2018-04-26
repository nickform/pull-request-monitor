using NUnit.Framework;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.UnitTest.ViewModel
{
    [TestFixture]
    public class DelegateCommandTest
    {
        private static readonly bool[] TrueAndFalse = {true, false};

        [Test, TestCaseSource(nameof(TrueAndFalse))]
        public void TestCanExecute_ReturnsCanExecuteDelegateResult(bool canExecuteDelegateResult)
        {
            var delegateCommand = new DelegateCommand
            {
                CanExecuteFunc = () => canExecuteDelegateResult,
                CommandAction = () =>
                {
                }
            };

            Assert.That(delegateCommand.CanExecute(/*ignored by implementation*/this), Is.EqualTo(canExecuteDelegateResult));
        }

        [Test]
        public void TestExecute_ExecutesCommandAction()
        {
            int executedTimes = 0;
            var delegateCommand = new DelegateCommand
            {
                CanExecuteFunc = () => true,
                CommandAction = () =>
                {
                    ++executedTimes;
                }
            };

            delegateCommand.Execute(/*ignored by implementation*/this);

            Assert.That(executedTimes, Is.EqualTo(1));
        }
    }
}