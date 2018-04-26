using System.Threading;
using System.Windows;
using NUnit.Framework;
using PullRequestMonitor.View;

namespace PullRequestMonitor.UnitTest.View
{
    public class SettingsWindowTest
    {
        [Test, Explicit, Category("Explicit"), Apartment(ApartmentState.STA)]
        public void ShowWindow()
        {
            SettingsWindow window;

            var t = new Thread(() =>
            {
                window = new SettingsWindow
                {
                    WindowStyle = WindowStyle.SingleBorderWindow,
                };

                window.IsVisibleChanged += (sender, args) =>
                {
                    if ((bool) args.NewValue == false) System.Windows.Threading.Dispatcher.ExitAllFrames();
                };

                window.Show();

                System.Windows.Threading.Dispatcher.Run();
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }
    }
}