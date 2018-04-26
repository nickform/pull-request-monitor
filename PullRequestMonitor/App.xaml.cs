using System.Windows;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Hardcodet.Wpf.TaskbarNotification;
using PullRequestMonitor.Model;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly WindsorContainer _container;
        private readonly ITrayIcon _trayIcon;
        private ITrayIconViewModel _trayIconViewModel;
        private TaskbarIcon _trayIconView;

        public App()
        {
            _container = new WindsorContainer();
            _container.Install(FromAssembly.This());

            _trayIcon = _container.Resolve<ITrayIcon>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _trayIconViewModel = _container.Resolve<ITrayIconViewModel>();
            _trayIconViewModel.Model = _trayIcon;
            _trayIconView = (TaskbarIcon) FindResource("TrayIcon");
            // ReSharper disable once PossibleNullReferenceException - Happy to crash here if this happens
            _trayIconView.DataContext = _trayIconViewModel;

            _trayIcon.RunMonitor();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIconView.Dispose();
            _container.Dispose();
            base.OnExit(e);
        }
    }
}
