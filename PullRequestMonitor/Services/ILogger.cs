using System;

namespace PullRequestMonitor.Services
{
    public interface ILogger
    {
        void Info(string message);
        void Info(string message, Exception e);
        void Warn(string message);
        void Error(string message);
        void Error(string message, Exception e);
    }
}