using System;

namespace PullRequestMonitor.Model
{
    /// <summary>
    /// Contains settings for a single TFS project to 
    /// be monitored.
    /// </summary>
    public  sealed class MonitoredProjectSettings
    {
        /// <summary>
        /// The base URI for the server where the project is
        /// hosted.
        /// </summary>
        public string VstsAccount { get; set; }
        /// <summary>
        /// The Id of the project to be monitored.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// A regular expression to apply to the names of the repositories
        /// in the project to determine which to monitor (matching names 
        /// will be monitored). If null or empty, all repositories will
        /// be monitored.
        /// </summary>
        public string RepoNameRegexp { get; set; }

        private bool Equals(MonitoredProjectSettings other)
        {
            return Equals(VstsAccount, other.VstsAccount) && Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MonitoredProjectSettings) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((VstsAccount != null ? VstsAccount.GetHashCode() : 0)*397) ^ Id.GetHashCode();
            }
        }
    }
}