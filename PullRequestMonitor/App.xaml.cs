using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Hardcodet.Wpf.TaskbarNotification;
using PullRequestMonitor.Model;
using PullRequestMonitor.Services;
using PullRequestMonitor.ViewModel;
using Squirrel;

namespace PullRequestMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private const ShortcutLocation ShortcutLocations = ShortcutLocation.StartMenu | ShortcutLocation.Startup;
        private readonly WindsorContainer _container;
        private readonly ITrayIcon _trayIcon;
        private ITrayIconViewModel _trayIconViewModel;
        private TaskbarIcon _trayIconView;
        private readonly ILogger _logger;

        public App()
        {
            _container = new WindsorContainer();
            _container.Install(FromAssembly.This());

            _trayIcon = _container.Resolve<ITrayIcon>();
            _logger = _container.Resolve<ILogger>();


            SquirrelAwareApp.HandleEvents(
                onInitialInstall: InitialInstall,
                onAppUpdate: AppUpdate,
                onAppUninstall: AppUninstall);
        }

        private void InitialInstall(Version version)
        {
            _logger.Info($"{nameof(App)}: received call to {nameof(InitialInstall)} with {version}");
            using (var mgr = GetUpdateManager())
            {
                try
                {
                    mgr.Result.CreateShortcutsForExecutable(ExeName(), ShortcutLocations, false);
                    _logger.Info($"{nameof(App)}: returned cleanly from call to CreateShortcutsForExecutable");
                }
                catch (System.Exception e)
                {
                    _logger.Error($"{nameof(App)}: failed to create shortcuts due to an exception", e);
                }
            }
        }

        private void AppUpdate(Version version)
        {
            _logger.Info($"{nameof(App)}: received call to {nameof(AppUpdate)} with {version}");
            using (var mgr = GetUpdateManager())
            {
                mgr.Result.CreateShortcutsForExecutable(ExeName(), ShortcutLocations, true);
            }
        }

        private void AppUninstall(Version version)
        {
            _logger.Info($"{nameof(App)}: received call to {nameof(AppUninstall)} with {version}");
            using (var mgr = GetUpdateManager())
            {
                mgr.Result.RemoveShortcutsForExecutable(ExeName(), ShortcutLocations);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _trayIconViewModel = _container.Resolve<ITrayIconViewModel>();
            _trayIconViewModel.Model = _trayIcon;
            _trayIconView = (TaskbarIcon) FindResource("TrayIcon");
            // ReSharper disable once PossibleNullReferenceException - Happy to crash here if this happens
            _trayIconView.DataContext = _trayIconViewModel;

            Task.Run(() => Update());

            _trayIcon.RunMonitor();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIconView.Dispose();
            _container.Dispose();
            base.OnExit(e);
        }

        private async void Update()
        {
            using (var mgr = GetUpdateManager())
            {
                try
                {
                    _logger.Info($"{nameof(App)}: attempting to update...");
                    var version = await mgr.Result.UpdateApp();
                    _logger.Info($"{nameof(App)}: UpdateManager returned version {version}");
                }
                catch (Exception e)
                {
                    _logger.Error($"{nameof(App)}: failed to update app due to exception:", e);
                }
            }
        }

        private static Task<UpdateManager> GetUpdateManager()
        {
            return UpdateManager.GitHubUpdateManager(PullRequestMonitor.Properties.Resources.SquirrelUrlOrPath);
        }

        private static string ExeName()
        {
            return "PullRequestMonitor.exe";
        }
    }
}
