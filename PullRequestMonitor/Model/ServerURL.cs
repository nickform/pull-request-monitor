namespace PullRequestMonitor.Model
{
    public static class ServerUrl
    {
        public static string GetServerURL(string account)
        {
            return $"https://dev.azure.com/{account}/";
        }
    }
}
