using System;
using System.Diagnostics;
using log4net;

[assembly: log4net.Config.XmlConfigurator]

namespace PullRequestMonitor.Services
{
    public class FileLogger: ILogger
    {
        private readonly ILog _impl;

        public FileLogger()
        {
            log4net.GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
            _impl = LogManager.GetLogger("FileLogger");
        }

        public void Info(string message)
        {
            _impl.Info(message);
        }

        public void Info(string message, Exception e)
        {
            _impl.Info(message, e);
        }

        public void Warn(string message)
        {
            _impl.Warn(message);
        }

        public void Error(string message)
        {
            _impl.Error(message);
        }

        public void Error(string message, Exception e)
        {
            _impl.Error(message, e);
        }
    }
}