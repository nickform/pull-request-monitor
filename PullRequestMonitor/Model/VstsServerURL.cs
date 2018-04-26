namespace PullRequestMonitor.Model
{
    static class VstsServerURL
    {
        public static string GetVstsServerURL(string vstsAccount)
        {
            return $"https://{vstsAccount}.visualstudio.com/";
        }
    }
}
