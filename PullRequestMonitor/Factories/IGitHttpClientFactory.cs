using System;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;

namespace PullRequestMonitor.Factories
{
    public interface IGitHttpClientFactory
    {
        GitHttpClient Create(Uri baseUrl, VssCredentials credentials, VssHttpRequestSettings settings);
        void Release(GitHttpClient gitHttpClient);
    }
}