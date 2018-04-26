namespace PullRequestMonitor.Model
{
    public enum RetrievalStatus
    {
        Unstarted,
        Ongoing,
        Suceeded,
        FailedDueToConnection,
        FailedDueToAuth,
        FailedReasonUnknown,
    }
}