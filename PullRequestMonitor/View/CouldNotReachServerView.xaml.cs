using System.Windows.Navigation;

namespace PullRequestMonitor.View
{
    /// <summary>
    /// Interaction logic for CouldNotReachServer.xaml
    /// </summary>
    public partial class CouldNotReachServerView
    {
        public CouldNotReachServerView()
        {
            InitializeComponent();
        }
        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}
