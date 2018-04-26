using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using PullRequestMonitor.ViewModel;

namespace PullRequestMonitor.View
{
    /// <summary>
    /// Interaction logic for PullRequestView.xaml
    /// </summary>
    public partial class PullRequestView
    {
        public PullRequestView()
        {
            InitializeComponent();
        }
        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            OpenPullRequestWebView(e.Uri.ToString());
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as Grid;
            var viewModel = grid?.DataContext as PullRequestViewModel;
            if (viewModel == null) return;

            OpenPullRequestWebView(viewModel.WebViewUri.ToString());
        }

        private static void OpenPullRequestWebView(string webViewUri)
        {
            System.Diagnostics.Process.Start(webViewUri);
        }
    }
}
