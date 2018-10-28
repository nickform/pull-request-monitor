using System;
using System.Windows;
using System.Windows.Navigation;

namespace PullRequestMonitor.View
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            HomepageLink.NavigateUri = new Uri(Properties.Resources.ProjectHomepage);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(Properties.Resources.ProjectHomepage);
            Hide();
        }
    }
}
