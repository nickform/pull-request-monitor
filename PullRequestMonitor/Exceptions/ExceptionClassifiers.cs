using System;
using System.Net.Http;
using Microsoft.VisualStudio.Services.Common;

namespace PullRequestMonitor.Exceptions
{
    static class ExceptionClassifiers
    {
        public static bool IsConnectivityException(Exception e)
        {
            var isHttpRequestException = e.GetType().ToString() == typeof(HttpRequestException).ToString();
            return isHttpRequestException || e is TimeoutException;
        }

        public static bool IsAuthorisationException(Exception e)
        {
            return e is VssAuthenticationException || e is VssUnauthorizedException;
        }
    }
}
