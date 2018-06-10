﻿using System;
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
            try
            {
                using (var mgr = GetUpdateManager())
                {
                    mgr.CreateShortcutsForExecutable(ExeName(), ShortcutLocations, false);
                    _logger.Info($"{nameof(App)}: returned cleanly from call to CreateShortcutsForExecutable");
                }
            }
            catch (Exception e)
            {
                _logger.Error($"{nameof(App)}: failed to create shortcuts due to an exception", e);
            }
        }

        private void AppUpdate(Version version)
        {
            _logger.Info($"{nameof(App)}: received call to {nameof(AppUpdate)} with {version}");
            using (var mgr = GetUpdateManager())
            {
                mgr.CreateShortcutsForExecutable(ExeName(), ShortcutLocations, true);
            }
        }

        private void AppUninstall(Version version)
        {
            _logger.Info($"{nameof(App)}: received call to {nameof(AppUninstall)} with {version}");
            using (var mgr = GetUpdateManager())
            {
                mgr.RemoveShortcutsForExecutable(ExeName(), ShortcutLocations);
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
            try
            {
                using (var mgr = GetUpdateManager())
                {
                    _logger.Info($"{nameof(App)}: attempting to update from current version ({mgr.CurrentlyInstalledVersion(ExeName())})...");
                    var version = await mgr.UpdateApp();
                    if (version == null)
                    {
                        _logger.Info($"{nameof(App)}: UpdateManager found no newer version");
                    } else {
                        _logger.Info($"{nameof(App)}: UpdateManager returned version {version.Version}");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"{nameof(App)}: failed to update app due to exception:", e);
            }
        }

        private static UpdateManager GetUpdateManager()
        {
            var gitHubUpdateManager = UpdateManager.GitHubUpdateManager(PullRequestMonitor.Properties.Resources.SquirrelUrlOrPath);
            gitHubUpdateManager.Wait();
            return gitHubUpdateManager.Result;
        }

        private static string ExeName()
        {
            return "PullRequestMonitor.exe";
        }
    }
}
