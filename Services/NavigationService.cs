using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;


namespace DebloaterPro.Services
{
    public class NavigationService
    {
        private static NavigationService? _instance;
        public static NavigationService Instance => _instance ??= new NavigationService();

        public Frame? MainFrame { get; private set; }
        public NavigationView? NavView { get; private set; }

        // Private constructor to prevent direct instantiation
        private NavigationService() { }

        // Initialize method to set the MainFrame and NavView
        public void Initialize(Frame mainFrame, NavigationView navView)
        {
            MainFrame = mainFrame ?? throw new ArgumentNullException(nameof(mainFrame));
            NavView = navView ?? throw new ArgumentNullException(nameof(navView));
        }

        public void Navigate(Type pageType, string tag)
        {
            MainFrame?.Navigate(pageType);
            SelectNavigationItem(tag);
        }

        public void SelectNavigationItem(string tag)
        {
            var selectedItem = NavView?
                .MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(item => item.Tag?.ToString() == tag);

            if (selectedItem != null)
            {
                NavView.SelectedItem = selectedItem;
            }
        }
    }
}
