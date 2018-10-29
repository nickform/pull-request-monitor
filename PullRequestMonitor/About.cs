using System;
using System.Diagnostics;
using Squirrel;

namespace PullRequestMonitor
{
    /// <summary>
    /// Provides bindable access to facts about the application.
    /// </summary>
    public class About
    {
        private readonly Lazy<Uri> _projectHomepage = new Lazy<Uri>(() => new Uri(Properties.Resources.ProjectHomepage));
        private readonly Lazy<string> _assemblyVersion = new Lazy<string>(() =>
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.ProductVersion;
        });

        public Uri ProjectHomepage => _projectHomepage.Value;

        public string ProjectHomepageString => _projectHomepage.Value.AbsoluteUri;

        public string Version => _assemblyVersion.Value;
    }
}