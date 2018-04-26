using System;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using PullRequestMonitor.Model;
using PullRequestMonitor.Properties;

namespace PullRequestMonitor.Factories
{
    public interface ITfsConnectionFactory
    {
        ITfsConnection Create(string baseUri);
        void Release(ITfsConnection server);
    }

    public class TfsConnectionFactory : ITfsConnectionFactory
    {
        private readonly IPullRequestFactory _pullRequestFactory;
        private readonly ITfProjectFactory _tfProjectFactory;
        private readonly ITfGitRepositoryFactory _tfGitRepositoryFactory;

        public TfsConnectionFactory(IPullRequestFactory pullRequestFactory, ITfProjectFactory tfProjectFactory, ITfGitRepositoryFactory tfGitRepositoryFactory)
        {
            _pullRequestFactory = pullRequestFactory;
            _tfProjectFactory = tfProjectFactory;
            _tfGitRepositoryFactory = tfGitRepositoryFactory;
        }

        public ITfsConnection Create(string baseUri)
        {
            // The first use of the connection created below launches an interactive login dialog on first run...
            var connection = new VssConnection(new Uri(baseUri), new VssClientCredentials(), RequestSettings);
            // The token it generates is stored under: HKEY_CURRENT_USER\SOFTWARE\Microsoft\VSCommon\14.0\ClientServices\TokenStorage\VisualStudio

            return new TfsConnection(connection, _pullRequestFactory, _tfProjectFactory, _tfGitRepositoryFactory);
        }

        public void Release(ITfsConnection server)
        { }

        private static readonly VssHttpRequestSettings RequestSettings = new VssClientHttpRequestSettings
        {
            SendTimeout = TimeSpan.FromSeconds(Resources.TfsServer_Timeout_Seconds)
        };
    }
}