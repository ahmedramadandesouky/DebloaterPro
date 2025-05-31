using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;


namespace DebloaterPro.Pages
{
    public sealed partial class Debloater : Page
    {
        public ObservableCollection<AppEntry> AllApps { get; } = [];
        public ObservableCollection<AppEntry> FilteredApps { get; } = [];

        public readonly string[] recommendedList =
        [
            "Microsoft.Xbox", "Microsoft.People", "Microsoft.Skype", "Microsoft.3DBuilder",
            "Microsoft.MicrosoftSolitaireCollection", "Microsoft.ZuneMusic", "Microsoft.ZuneVideo",
            "Microsoft.GetHelp", "Microsoft.Getstarted", "Microsoft.MicrosoftOfficeHub", "Microsoft.Todos",
            "Microsoft.BingNews", "Microsoft.BingWeather", "Microsoft.WindowsFeedbackHub", "Microsoft.WindowsMaps",
            "Microsoft.WindowsAlarms", "Microsoft.WindowsCamera", "Microsoft.WindowsSoundRecorder", "Microsoft.MixedReality.Portal"
        ];

        public readonly string[] criticalSystemApps =
        [
            "Microsoft.WindowsStore", "Microsoft.WindowsCalculator", "Microsoft.Windows.Photos",
            "Microsoft.MicrosoftEdge", "Microsoft.WindowsNotepad", "Microsoft.WindowsTerminal"
        ];

        public readonly string[] dangerousList =
        [
            "Microsoft.XboxGamingOverlay",
            "Microsoft.Xbox.TCUI",
            "Microsoft.XboxGameOverlay",
            "Microsoft.XboxSpeechToTextOverlay",
            "Microsoft.XboxIdentityProvider",
            "Microsoft.YourPhone",
            "Microsoft.Microsoft3DViewer",
            "Microsoft.MixedReality.Portal",
            "Microsoft.MSPaint",
            "Microsoft.ScreenSketch",
            "Microsoft.StickyNotes",
            "Microsoft.OneConnect",
            "Microsoft.Wallet",
            "Microsoft.Advertising.Xaml",
            "Microsoft.Office.OneNote",
            "Microsoft.BingFinance",
            "Microsoft.BingFoodAndDrink",
            "Microsoft.BingHealthAndFitness",
            "Microsoft.BingTravel",
            "Microsoft.CommsPhone",
            "Microsoft.SkypeApp",
            "Microsoft.ZuneMusic",
            "Microsoft.ZuneVideo",
            "Microsoft.WindowsReadingList",
            "Microsoft.Messaging",
            "Microsoft.WindowsPhone",
            "Microsoft.OneNote",
            "Microsoft.ConnectivityStore",
            "Microsoft.3DBuilder",
            "Microsoft.Appconnector",
            "Microsoft.WindowsStorePreview",
            "Microsoft.XboxApp",
            "Microsoft.GamingApp",
            "Microsoft.People"
        ];

        public Debloater()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadAppsAsync();
        }

        private async Task LoadAppsAsync()
        {
            try
            {
                LoadingIndicator.IsActive = true;
                AllApps.Clear();
                FilteredApps.Clear();

                await Task.Run(() =>
                {
                    var packageManager = new PackageManager();
                    var packages = packageManager.FindPackagesForUser(string.Empty);

                    foreach (var pkg in packages)
                    {
                        if (!pkg.IsFramework && !string.IsNullOrEmpty(pkg.Id.Name))
                        {
                            var name = pkg.Id.Name;
                            var riskLevel = AppRiskLevel.Safe;

                            if (criticalSystemApps.Any(c => name.Contains(c, StringComparison.OrdinalIgnoreCase)))
                                riskLevel = AppRiskLevel.Critical;
                            else if (recommendedList.Any(r => name.Contains(r, StringComparison.OrdinalIgnoreCase)))
                                riskLevel = AppRiskLevel.Moderate;
                            else if (dangerousList.Any(d => name.Contains(d, StringComparison.OrdinalIgnoreCase)))
                                riskLevel = AppRiskLevel.Dangerous;

                            var entry = new AppEntry
                            {
                                DisplayName = name,
                                PackageFullName = pkg.Id.FullName,
                                IsRecommended = recommendedList.Any(r => name.Contains(r, StringComparison.OrdinalIgnoreCase)),
                                IsCritical = criticalSystemApps.Any(c => name.Contains(c, StringComparison.OrdinalIgnoreCase)),
                                IsDangerous = dangerousList.Any(d => name.Contains(d, StringComparison.OrdinalIgnoreCase)),
                                RiskLevel = riskLevel,
                                Version = pkg.Id.Version,
                                InstallDate = pkg.InstalledDate.DateTime
                            };

                            DispatcherQueue.TryEnqueue(() => AllApps.Add(entry));
                        }
                    }

                });

                ApplyFilter(SearchBox.Text);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading apps: {ex.Message}");
                await ShowMessageDialog("Error", "Failed to load installed apps. Please try again.");
            }
            finally
            {
                LoadingIndicator.IsActive = false;
            }
        }

        private void ApplyFilter(string filter)
        {
            FilteredApps.Clear();
            var filtered = string.IsNullOrWhiteSpace(filter)
                ? AllApps
                : AllApps.Where(app => app.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                      app.PackageFullName.Contains(filter, StringComparison.OrdinalIgnoreCase));

            foreach (var app in filtered)
            {
                FilteredApps.Add(app);
            }
        }

        private async void RemoveSelectedApps_Click(object sender, RoutedEventArgs e)
        {
            var selected = AppListView.SelectedItems.Cast<AppEntry>().ToList();
            if (selected.Count == 0)
            {
                await ShowMessageDialog("No Selection", "Please select at least one app to remove.");
                return;
            }

            var criticalSelected = selected.Where(app => app.IsCritical).ToList();
            if (criticalSelected.Count > 0)
            {
                var result = await ShowConfirmationDialog("Warning",
                    $"You are about to remove critical system apps: {string.Join(", ", criticalSelected.Select(a => a.DisplayName))}\n\nAre you sure you want to continue?");
                if (result != ContentDialogResult.Primary) return;
            }

            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.Value = 0;
            double step = 100.0 / selected.Count;

            bool anyFailed = false;
            foreach (var app in selected)
            {
                try
                {
                    await RemoveAppAsync(app.DisplayName);
                    ProgressBar.Value += step;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to remove {app.DisplayName}: {ex.Message}");
                    anyFailed = true;
                }
            }

            ProgressBar.Value = 100;
            ProgressBar.Visibility = Visibility.Collapsed;

            if (anyFailed)
            {
                await ShowMessageDialog("Partial Success", "Some apps couldn't be removed. Check debug logs for details.");
            }

            await LoadAppsAsync();
        }

        private static async Task RemoveAppAsync(string appName)
        {
            await Task.Run(() =>
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-WindowStyle Hidden -Command \"Get-AppxPackage -Name '{appName}' | Remove-AppxPackage -ErrorAction Stop\"",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    Verb = "runas"
                };

                using var process = Process.Start(psi);
                process?.WaitForExit();

                if (process?.ExitCode != 0)
                {
                    throw new Exception($"PowerShell exited with code {process?.ExitCode}");
                }
            });
        }

        private async Task<ContentDialogResult> ShowConfirmationDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Continue",
                SecondaryButtonText = "Cancel",
                XamlRoot = Content.XamlRoot
            };
            return await dialog.ShowAsync();
        }

        private async Task ShowMessageDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(SearchBox.Text);
        }

        private void SelectRecommended_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in AppListView.Items.Cast<AppEntry>().Where(app => app.IsRecommended))
            {
                AppListView.SelectedItems.Add(item);
            }
        }

        private void SelectLTSCStyle_Click(object sender, RoutedEventArgs e)
        {
            foreach (var app in AppListView.Items.Cast<AppEntry>().Where(app => app.RiskLevel == AppRiskLevel.Dangerous || app.IsRecommended))
            {
                if (!AppListView.SelectedItems.Contains(app))
                    AppListView.SelectedItems.Add(app);
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadAppsAsync();
        }

        private void RiskFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RiskFilterCombo.SelectedItem is ComboBoxItem selectedItem)
            {
                var tag = selectedItem.Tag as string;

                if (string.IsNullOrEmpty(tag) || tag == "All")
                {
                    ApplyFilter(SearchBox.Text);
                    return;
                }

                var filteredByRisk = AllApps.Where(app =>
                {
                    return tag switch
                    {
                        "Recommended" => app.IsRecommended,
                        "Critical" => app.RiskLevel == AppRiskLevel.Critical,
                        "Dangerous" => app.RiskLevel == AppRiskLevel.Dangerous,
                        _ => true
                    };
                });

                FilteredApps.Clear();
                foreach (var app in filteredByRisk)
                    FilteredApps.Add(app);
            }
        }
    }

    public enum AppRiskLevel
    {
        Safe,
        Moderate,
        Critical,
        Dangerous
    }

    public class AppEntry
    {
        public string DisplayName { get; set; }
        public string PackageFullName { get; set; }
        public bool IsRecommended { get; set; }
        public bool IsCritical { get; set; }
        public bool IsDangerous { get; set; }
        public AppRiskLevel RiskLevel { get; set; }
        public string Description { get; set; }
        public PackageVersion Version { get; set; }
        public DateTime InstallDate { get; set; }
    }
}