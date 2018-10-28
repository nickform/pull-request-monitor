using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using PullRequestMonitor.Factories;
using PullRequestMonitor.Model;
using PullRequestMonitor.Services;
using PullRequestMonitor.View;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor
{
    public sealed class WindsorInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.AddFacility<TypedFactoryFacility>();
            container.Register(
                Component.For<IApplicationActions>().ImplementedBy<ApplicationActions>(),
                Component.For<ITrayIcon>().ImplementedBy<TrayIcon>(),
                Component.For<ITrayIconViewModel>().ImplementedBy<TrayIconViewModel>(),
                Component.For<IMonitor>().ImplementedBy<Monitor>().LifestyleSingleton(),
                Component.For<IMonitorViewModelFactory>().AsFactory(),
                Component.For<MonitorWindowViewModel>().LifestyleTransient(),
                Component.For<MonitorWindow>(),
                Component.For<FirstUpdateViewModel>(),
                Component.For<CouldNotReachServerViewModel>(),
                Component.For<UnrecognisedErrorViewModel>(),
                Component.For<INoProjectsViewModel>().ImplementedBy<NoProjectsViewModel>(),
                Component.For<SingleProjectViewModel>(),
                Component.For<ActivePullRequestListViewModel>().LifestyleTransient(),
                Component.For<CompletedPullRequestListViewModel>().LifestyleTransient(),
                Component.For<IAppSettings>().ImplementedBy<AppSettings>(),
                Component.For<IMonitorSettings>().ImplementedBy<MonitorSettings>(),
                Component.For<SettingsViewModel>(),
                Component.For<SettingsWindow>().LifestyleTransient(),
                Component.For<AboutWindow>().LifestyleSingleton(),
                Component.For<NameRegexpRepositoryFilter>().LifestyleTransient(),
                Component.For<INameRegexpRepositoryFilterFactory>().AsFactory(),
                Component.For<IPullRequestFactory>().AsFactory(),
                Component.For<IPullRequest>().ImplementedBy<PullRequest>().LifestyleTransient(),
                Component.For<ITfGitRepositoryFactory>().AsFactory(),
                Component.For<ITfGitRepository>().ImplementedBy<TfGitRepository>().LifestyleTransient(),
                Component.For<ITfProjectFactory>().AsFactory(),
                Component.For<ITfProject>().ImplementedBy<TfProject>().LifestyleTransient(),
                Component.For<ITfsConnectionFactory>().ImplementedBy<TfsConnectionFactory>(),
                Component.For<ITfProjectCollectionFactory>().AsFactory(),
                Component.For<ITfProjectCollectionCache>().ImplementedBy<TfProjectCollectionCache>(),
                Component.For<ITfProjectCollection>().ImplementedBy<TfProjectCollection>().LifestyleTransient(),
                Component.For<GitHttpClient>().LifestyleTransient(),
                Component.For<IGitHttpClientFactory>().AsFactory(),
                Component.For<ILogger>().ImplementedBy<FileLogger>().LifestyleSingleton()
            );
        }
    }
}