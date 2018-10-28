using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace PullRequestMonitor.View
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void SettingsWindow_OnClosing(object sender, CancelEventArgs e)
        {
            // Hide the window instead of closing it so that it can be re-used...
            e.Cancel = true;
            Hide();
        }

        private void SettingsWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
            var accountBinding = accountTextBox.GetBindingExpression(TextBox.TextProperty);
            accountBinding.UpdateSource();
            var repoNameBinding = repoNamePatternTextBox.GetBindingExpression(TextBox.TextProperty);
            repoNameBinding.UpdateSource();
        }
    }
}
