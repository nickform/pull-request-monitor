using System;
using System.Net.Http;
using Microsoft.VisualStudio.Services.Common;

namespace PullRequestMonitor.UnitTest.Exceptions
{
    class CommonExceptionExamples
    {
        public static Exception[] CouldNotReachServerExceptions = {
            new HttpRequestException(),
            new AggregateException(new HttpRequestException()),
            new AggregateException(new HttpRequestException(), new VssUnauthorizedException())
        };

        public static Exception[] UnauthorisedExceptions = {
            new VssUnauthorizedException(),
            new AggregateException(new VssUnauthorizedException()),
            new VssAuthenticationException(),
            new AggregateException(new VssAuthenticationException())
        };

        public static Exception[] UnrecognisedExceptions = {
            new Exception(),
            new AggregateException(new Exception())
        };
    }
}
