using DebloaterPro.Pages;
using DebloaterPro.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace DebloaterPro
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Initialize NavigationService with ContentFrame and NavView
            NavigationService.Instance.Initialize(ContentFrame, NavView);

            this.Title = "DebloaterPro";
            this.AppWindow.SetIcon("Assets/DebloaterPro.ico");

            NotificationService.Instance.Initialize(MainInfoBar);
            ContentFrame.Navigate(typeof(Dashboard));
            NavView.SelectedItem = DashboardItem;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag)
                {
                    case "DashboardPage":
                        ContentFrame.Navigate(typeof(Dashboard));
                        break;
                    case "DebloaterPage":
                        ContentFrame.Navigate(typeof(Debloater));
                        break;
                    case "TweaksPage":
                        ContentFrame.Navigate(typeof(Tweaks));
                        break;
                    case "PrivacyPage":
                        ContentFrame.Navigate(typeof(Privacy));
                        break;
                    case "WindowsServicesPage":
                        ContentFrame.Navigate(typeof(WindowsServices));
                        break;
                    case "ToolsPage":
                        ContentFrame.Navigate(typeof(Tools));
                        break;
                    case "AboutPage":
                        ContentFrame.Navigate(typeof(About));
                        break;
                }
            }
        }
    }
}
