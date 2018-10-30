using System.Threading;
using NUnit.Framework;
using PullRequestMonitor.View;

namespace PullRequestMonitor.UnitTest.View
{
    public class AboutWindowTest
    {
        [Test, Explicit, Category("Explicit"), Apartment(ApartmentState.STA)]
        public void ShowWindow()
        {
            AboutWindow window;

            var t = new Thread(() =>
            {
                window = new AboutWindow();

                window.Closed += (sender, args) => System.Windows.Threading.Dispatcher.ExitAllFrames();

                window.Show();

                System.Windows.Threading.Dispatcher.Run();
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }
    }
}