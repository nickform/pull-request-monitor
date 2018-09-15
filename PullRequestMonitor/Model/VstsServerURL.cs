namespace PullRequestMonitor.Model
{
    public static class VstsServerURL
    {
        public static string GetVstsServerURL(string vstsAccount)
        {
            return $"https://{vstsAccount}.visualstudio.com/";
        }
    }
}
