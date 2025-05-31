using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System.Diagnostics;


namespace DebloaterPro.Pages
{
    public sealed partial class About : Page
    {
        public About()
        {
            this.InitializeComponent();
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            AppVersionTextBlock.Text = $"Version v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var link = (sender as Hyperlink)?.NavigateUri?.ToString();
            if (!string.IsNullOrEmpty(link))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = link,
                    UseShellExecute = true
                });
            }
        }
    }
}
