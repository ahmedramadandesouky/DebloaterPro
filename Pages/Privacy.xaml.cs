using DebloaterPro.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace DebloaterPro.Pages
{
    public sealed partial class Privacy : Page
    {
        public List<PrivacyFeature> TelemetryFeatures { get; } =
        [
            new PrivacyFeature
            {
                Title = "Telemetry Level",
                Description = "Controls the amount of diagnostic and usage data sent to Microsoft",
                RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                ValueName = "AllowTelemetry",
                RecommendedValue = 0,
                Recommended = true,
                PossibleValues = new Dictionary<int, string>
                {
                    {0, "Security (Enterprise only)"},
                    {1, "Basic"},
                    {2, "Enhanced"},
                    {3, "Full"}
                }
            },
            new PrivacyFeature
            {
                Title = "CEIP (Customer Experience)",
                Description = "Participate in Microsoft's Customer Experience Improvement Program",
                RegistryPath = @"SOFTWARE\Microsoft\SQMClient\Windows",
                ValueName = "CEIPEnable",
                RecommendedValue = 0,
                Recommended = true
            },
            new PrivacyFeature
            {
                Title = "Advertising ID",
                Description = "Disables personalized ads in Windows",
                RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
                ValueName = "Enabled",
                RecommendedValue = 0,
                Recommended = true
            },
            new PrivacyFeature
            {
                Title = "Tailored Experiences",
                Description = "Disables personalized tips, ads and recommendations",
                RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy",
                ValueName = "TailoredExperiencesWithDiagnosticDataEnabled",
                RecommendedValue = 0,
                Recommended = true
            }
        ];

        public List<PrivacyFeature> ActivityHistoryFeatures { get; } =
        [
            new PrivacyFeature
            {
                Title = "Activity History",
                Description = "Tracks your activities across devices",
                RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows\System",
                ValueName = "PublishUserActivities",
                RecommendedValue = 0,
                Recommended = true
            },
            new PrivacyFeature
            {
                Title = "Timeline",
                Description = "Stores your activity history locally",
                RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows\System",
                ValueName = "UploadUserActivities",
                RecommendedValue = 0,
                Recommended = true
            },
            new PrivacyFeature
            {
                Title = "Clipboard History",
                Description = "Stores multiple items in clipboard history",
                RegistryPath = @"SOFTWARE\Microsoft\Clipboard",
                ValueName = "EnableClipboardHistory",
                RecommendedValue = 0,
                Recommended = false
            },
            new PrivacyFeature
            {
                Title = "Cloud Sync",
                Description = "Syncs activities across devices using Microsoft account",
                RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows\System",
                ValueName = "EnableActivityFeed",
                RecommendedValue = 0,
                Recommended = true
            }
        ];

        public List<PrivacyFeature> LocationSensorsFeatures { get; } =
        [
            new PrivacyFeature
            {
                Title = "Location Tracking",
                Description = "Disables device location tracking",
                RegistryPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Sensor\Overrides\{BFA794E4-F964-4FDB-90F6-51056BFE4B44}",
                ValueName = "SensorPermissionState",
                RecommendedValue = 0,
                Recommended = true
            },
            new PrivacyFeature
            {
                Title = "Location Service",
                Description = "Windows location service master switch",
                RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location",
                ValueName = "Value",
                RecommendedValue = "Deny",
                Recommended = true
            },
            new PrivacyFeature
            {
                Title = "Camera Access",
                Description = "Global camera access control",
                RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam",
                ValueName = "Value",
                RecommendedValue = "Deny",
                Recommended = false
            },
            new PrivacyFeature
            {
                Title = "Microphone Access",
                Description = "Global microphone access control",
                RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\microphone",
                ValueName = "Value",
                RecommendedValue = "Deny",
                Recommended = false
            }
        ];

        public List<PrivacyFeature> DiagnosticsFeatures { get; } =
        [
            new PrivacyFeature
            {
                Title = "Error Reporting",
                Description = "Controls whether Windows sends error reports to Microsoft",
                RegistryPath = @"SOFTWARE\Microsoft\Windows\Windows Error Reporting",
                ValueName = "Disabled",
                RecommendedValue = 1,
                Recommended = true
            },
            new PrivacyFeature
            {
                Title = "Feedback Frequency",
                Description = "How often Windows asks for feedback",
                RegistryPath = @"SOFTWARE\Microsoft\Siuf\Rules",
                ValueName = "NumberOfSIUFInPeriod",
                RecommendedValue = 0,
                Recommended = true
            },
            new PrivacyFeature
            {
                Title = "Automatic Feedback",
                Description = "Automatically send feedback without prompting",
                RegistryPath = @"SOFTWARE\Microsoft\Siuf\Rules",
                ValueName = "PeriodInNanoSeconds",
                RecommendedValue = 0,
                Recommended = true
            },
            new PrivacyFeature
            {
                Title = "Diagnostic Data Viewer",
                Description = "Access to view collected diagnostic data",
                RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Diagnostics\DiagTrack",
                ValueName = "ShowDiagnosticDataViewerButton",
                RecommendedValue = 0,
                Recommended = false
            }
        ];

        public List<PrivacyFeature> AppPermissionsFeatures { get; } =
        [
            new PrivacyFeature
            {
                Title = "App Diagnostics",
                Description = "Allow apps to access diagnostic information",
                RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appDiagnostics",
                ValueName = "Value",
                RecommendedValue = "Deny",
                Recommended = true
            },
            new PrivacyFeature
            {
                Title = "Account Info Access",
                Description = "Allow apps to access your account info",
                RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation",
                ValueName = "Value",
                RecommendedValue = "Deny",
                Recommended = true
            },
            new PrivacyFeature
            {
                Title = "Contacts Access",
                Description = "Allow apps to access your contacts",
                RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\contacts",
                ValueName = "Value",
                RecommendedValue = "Deny",
                Recommended = true
            },
            new PrivacyFeature
            {
                Title = "Calendar Access",
                Description = "Allow apps to access your calendar",
                RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\calendar",
                ValueName = "Value",
                RecommendedValue = "Deny",
                Recommended = true
            },
            new PrivacyFeature
            {
                Title = "Call History",
                Description = "Allow apps to access your call history",
                RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\phoneCall",
                ValueName = "Value",
                RecommendedValue = "Deny",
                Recommended = true
            },
            new PrivacyFeature
            {
                Title = "Email Access",
                Description = "Allow apps to access your email",
                RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\email",
                ValueName = "Value",
                RecommendedValue = "Deny",
                Recommended = true
            }
        ];

        public Privacy()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            LoadFeatureSettings(TelemetryFeatures);
            LoadFeatureSettings(ActivityHistoryFeatures);
            LoadFeatureSettings(LocationSensorsFeatures);
            LoadFeatureSettings(DiagnosticsFeatures);
            LoadFeatureSettings(AppPermissionsFeatures);
        }

        private static void LoadFeatureSettings(List<PrivacyFeature> features)
        {
            foreach (var feature in features)
            {
                try
                {
                    var key = feature.RegistryPath.StartsWith("HKEY_CURRENT_USER") ?
                        Registry.CurrentUser.OpenSubKey(feature.RegistryPath.Replace("HKEY_CURRENT_USER\\", "")) :
                        Registry.LocalMachine.OpenSubKey(feature.RegistryPath);

                    if (key != null)
                    {
                        var currentValue = key.GetValue(feature.ValueName);
                        if (currentValue != null)
                        {
                            if (feature.PossibleValues != null && currentValue is int intValue)
                            {
                                feature.CurrentValueDisplay = feature.PossibleValues.ContainsKey(intValue) ?
                                    feature.PossibleValues[intValue] : intValue.ToString();
                            }
                            else if (currentValue is string stringValue)
                            {
                                feature.CurrentValueDisplay = stringValue;
                            }
                            feature.CurrentValue = currentValue;
                        }
                    }
                }
                catch
                {
                    // Ignore errors
                }
            }
        }

        private void ApplyRecommendedSettings_Click(object sender, RoutedEventArgs e)
        {
            var allRecommendedFeatures = TelemetryFeatures.Concat(ActivityHistoryFeatures)
                .Concat(LocationSensorsFeatures)
                .Concat(DiagnosticsFeatures)
                .Concat(AppPermissionsFeatures)
                .Where(f => f.Recommended)
                .ToList();

            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.Maximum = allRecommendedFeatures.Count;
            ProgressBar.Value = 0;

            foreach (var feature in allRecommendedFeatures)
            {
                try
                {
                    var key = feature.RegistryPath.StartsWith("HKEY_CURRENT_USER") ?
                        Registry.CurrentUser.CreateSubKey(feature.RegistryPath.Replace("HKEY_CURRENT_USER\\", "")) :
                        Registry.LocalMachine.CreateSubKey(feature.RegistryPath);

                    if (key != null)
                    {
                        if (feature.RecommendedValue is int intValue)
                        {
                            key.SetValue(feature.ValueName, intValue, RegistryValueKind.DWord);
                        }
                        else if (feature.RecommendedValue is string stringValue)
                        {
                            key.SetValue(feature.ValueName, stringValue, RegistryValueKind.String);
                        }
                        feature.CurrentValue = feature.RecommendedValue;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to set {feature.Title}: {ex.Message}");
                    NotificationService.Instance.ShowError($"Failed to apply {feature.Title}: {ex.Message}");
                }
                finally
                {
                    ProgressBar.Value++;
                }
            }

            ProgressBar.Visibility = Visibility.Collapsed;
            LoadCurrentSettings();
            NotificationService.Instance.ShowSuccess("Recommended settings applied successfully!");

            RefreshLists();
        }

        private async void RestoreDefaultSettings_Click(object sender, RoutedEventArgs e)
        {
            var allFeatures = TelemetryFeatures.Concat(ActivityHistoryFeatures)
                .Concat(LocationSensorsFeatures)
                .Concat(DiagnosticsFeatures)
                .Concat(AppPermissionsFeatures)
                .ToList();

            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.Maximum = allFeatures.Count;
            ProgressBar.Value = 0;

            foreach (var feature in allFeatures)
            {
                try
                {
                    var key = feature.RegistryPath.StartsWith("HKEY_CURRENT_USER") ?
                        Registry.CurrentUser.CreateSubKey(feature.RegistryPath.Replace("HKEY_CURRENT_USER\\", "")) :
                        Registry.LocalMachine.CreateSubKey(feature.RegistryPath);

                    if (key != null)
                    {
                        // Default values are usually 1 for enable, 0 for disable
                        object defaultValue = feature.ValueName == "Value" ? "Allow" : 1;
                        if (defaultValue is int intValue)
                        {
                            key.SetValue(feature.ValueName, intValue, RegistryValueKind.DWord);
                        }
                        else if (defaultValue is string stringValue)
                        {
                            key.SetValue(feature.ValueName, stringValue, RegistryValueKind.String);
                        }
                        feature.CurrentValue = defaultValue;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to reset {feature.Title}: {ex.Message}");
                    NotificationService.Instance.ShowError($"Failed to restore {feature.Title}: {ex.Message}");
                }
                finally
                {
                    ProgressBar.Value++;
                }
            }

            ProgressBar.Visibility = Visibility.Collapsed;
            LoadCurrentSettings();
            ShowMessage("Default settings restored successfully!");
            NotificationService.Instance.ShowSuccess("Default settings restored successfully!");

            RefreshLists();
        }

        private void ToggleFeature_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PrivacyFeature feature)
            {
                try
                {
                    object newValue;
                    if (feature.CurrentValue is int currentInt)
                    {
                        newValue = currentInt == 0 ? 1 : 0;
                    }
                    else if (feature.CurrentValue is string currentString)
                    {
                        newValue = currentString == "Deny" ? "Allow" : "Deny";
                    }
                    else
                    {
                        newValue = feature.CurrentValue == null ? feature.RecommendedValue : null;
                    }

                    var key = feature.RegistryPath.StartsWith("HKEY_CURRENT_USER") ?
                        Registry.CurrentUser.CreateSubKey(feature.RegistryPath.Replace("HKEY_CURRENT_USER\\", "")) :
                        Registry.LocalMachine.CreateSubKey(feature.RegistryPath);

                    if (key != null)
                    {
                        if (newValue is int intValue)
                        {
                            key.SetValue(feature.ValueName, intValue, RegistryValueKind.DWord);
                        }
                        else if (newValue is string stringValue)
                        {
                            key.SetValue(feature.ValueName, stringValue, RegistryValueKind.String);
                        }
                        feature.CurrentValue = newValue;
                    }

                    RefreshLists();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to toggle {feature.Title}: {ex.Message}");
                    NotificationService.Instance.ShowError($"Failed to toggle {feature.Title}: {ex.Message}");
                }

                NotificationService.Instance.ShowSuccess($"{feature.Title} has been toggled.");
            }
        }

        private void RefreshLists()
        {
            TelemetryListView.ItemsSource = null;
            TelemetryListView.ItemsSource = TelemetryFeatures;

            ActivityHistoryListView.ItemsSource = null;
            ActivityHistoryListView.ItemsSource = ActivityHistoryFeatures;

            LocationSensorsListView.ItemsSource = null;
            LocationSensorsListView.ItemsSource = LocationSensorsFeatures;

            DiagnosticsListView.ItemsSource = null;
            DiagnosticsListView.ItemsSource = DiagnosticsFeatures;

            AppPermissionsListView.ItemsSource = null;
            AppPermissionsListView.ItemsSource = AppPermissionsFeatures;
        }

        private void ShowInfo_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PrivacyFeature feature)
            {
                string currentValue = feature.CurrentValueDisplay ??
                    (feature.CurrentValue?.ToString() ?? "Not set");

                string recommendedValue = feature.RecommendedValue is int i && feature.PossibleValues != null ?
                    feature.PossibleValues[i] :
                    feature.RecommendedValue?.ToString() ?? "Not set";

                InfoDialog.Title = feature.Title;
                InfoDialog.Content = $"{feature.Description}\n\n" +
                                    $"Current Value: {currentValue}\n" +
                                    $"Recommended Value: {recommendedValue}\n" +
                                    $"Registry Path: {feature.RegistryPath}\\{feature.ValueName}";
                InfoDialog.XamlRoot = Content.XamlRoot;
                InfoDialog.ShowAsync();
            }
        }

        private async void ShowMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Info",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    public class PrivacyFeature
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string RegistryPath { get; set; }
        public string ValueName { get; set; }
        public object RecommendedValue { get; set; }
        public object CurrentValue { get; set; }
        public string CurrentValueDisplay { get; set; }
        public bool Recommended { get; set; }
        public Dictionary<int, string> PossibleValues { get; set; }

        public string StatusIcon => (CurrentValue?.Equals(RecommendedValue) == true) ? "✅" : "⚠️";
        public string StatusText => (CurrentValue?.Equals(RecommendedValue) == true) ? "Optimized" : "Needs attention";
        public SolidColorBrush StatusColor => (CurrentValue?.Equals(RecommendedValue) == true ?
            new SolidColorBrush(Microsoft.UI.Colors.Green) :
            new SolidColorBrush(Microsoft.UI.Colors.Orange));
    }
}