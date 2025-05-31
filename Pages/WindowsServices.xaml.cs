using DebloaterPro.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Threading.Tasks;


namespace DebloaterPro.Pages
{
    public sealed partial class WindowsServices : Page
    {
        public ObservableCollection<ServiceFeature> FilteredServiceFeatures { get; set; }
        private DispatcherQueue dispatcherQueue;
        private bool _isInitializing = true;

        public ObservableCollection<ServiceFeature> ServiceFeatures { get; } =
        [
            // Windows Update Services
            new ServiceFeature
            {
                Title = "Windows Update",
                Description = "Provides Windows updates and drivers. Disabling may prevent system updates.",
                ServiceName = "wuauserv",
                RecommendedStartup = ServiceStartMode.Manual
            },
            new ServiceFeature
            {
                Title = "Update Orchestrator",
                Description = "Manages Windows Updates. Required for automatic updates.",
                ServiceName = "UsoSvc",
                RecommendedStartup = ServiceStartMode.Manual
            },
            new ServiceFeature
            {
                Title = "Windows Medic Service",
                Description = "Windows Update health monitoring and remediation.",
                ServiceName = "WaaSMedicSvc",
                RecommendedStartup = ServiceStartMode.Manual
            },

            // Privacy Related Services
            new ServiceFeature
            {
                Title = "Diagnostic Policy Service",
                Description = "Enables problem detection and troubleshooting. Sends data to Microsoft.",
                ServiceName = "DPS",
                RecommendedStartup = ServiceStartMode.Disabled
            },
            new ServiceFeature
            {
                Title = "Connected User Experiences",
                Description = "Collects usage data for diagnostics and personalized experiences.",
                ServiceName = "DiagTrack",
                RecommendedStartup = ServiceStartMode.Disabled
            },
            new ServiceFeature
            {
                Title = "Customer Experience Improvement",
                Description = "Collects anonymous usage data for Microsoft improvement programs.",
                ServiceName = "CEIP",
                RecommendedStartup = ServiceStartMode.Disabled
            },

            // Networking Services
            new ServiceFeature
            {
                Title = "DNS Client",
                Description = "Caches DNS lookups for performance. Required for most network operations.",
                ServiceName = "Dnscache",
                RecommendedStartup = ServiceStartMode.Automatic
            },
            new ServiceFeature
            {
                Title = "DHCP Client",
                Description = "Manages network configuration via DHCP. Required for most networks.",
                ServiceName = "Dhcp",
                RecommendedStartup = ServiceStartMode.Automatic
            },
            new ServiceFeature
            {
                Title = "SSDP Discovery",
                Description = "Discovers networked devices via UPnP. Often unnecessary.",
                ServiceName = "SSDPSRV",
                RecommendedStartup = ServiceStartMode.Manual
            },

            // System Services
            new ServiceFeature
            {
                Title = "Superfetch (SysMain)",
                Description = "Prefetches frequently used apps into memory. Can improve performance.",
                ServiceName = "SysMain",
                RecommendedStartup = ServiceStartMode.Automatic
            },
            new ServiceFeature
            {
                Title = "Windows Search",
                Description = "Indexes files for faster searching. Disabling may slow searches.",
                ServiceName = "WSearch",
                RecommendedStartup = ServiceStartMode.Automatic
            },
            new ServiceFeature
            {
                Title = "Print Spooler",
                Description = "Manages printing jobs. Disable if you don't use printers.",
                ServiceName = "Spooler",
                RecommendedStartup = ServiceStartMode.Disabled
            },

            // Security Services
            new ServiceFeature
            {
                Title = "Windows Defender",
                Description = "Provides antivirus protection. Only disable if using alternative AV.",
                ServiceName = "WinDefend",
                RecommendedStartup = ServiceStartMode.Automatic
            },
            new ServiceFeature
            {
                Title = "Windows Firewall",
                Description = "Provides network security filtering. Recommended to keep enabled.",
                ServiceName = "MpsSvc",
                RecommendedStartup = ServiceStartMode.Automatic
            },
            new ServiceFeature
            {
                Title = "Security Center",
                Description = "Monitors security settings and provides alerts.",
                ServiceName = "wscsvc",
                RecommendedStartup = ServiceStartMode.Automatic
            },

            // Remote Access Services
            new ServiceFeature
            {
                Title = "Remote Registry",
                Description = "Allows remote registry modification. Security risk if enabled.",
                ServiceName = "RemoteRegistry",
                RecommendedStartup = ServiceStartMode.Disabled
            },
            new ServiceFeature
            {
                Title = "Remote Desktop Services",
                Description = "Allows remote connections to this computer.",
                ServiceName = "TermService",
                RecommendedStartup = ServiceStartMode.Disabled
            },
            new ServiceFeature
            {
                Title = "Windows Remote Management",
                Description = "Provides remote management via WS-Management protocol.",
                ServiceName = "WinRM",
                RecommendedStartup = ServiceStartMode.Disabled
            },

            // Xbox Services
            new ServiceFeature
            {
                Title = "Xbox Live Auth Manager",
                Description = "Provides authentication for Xbox Live services.",
                ServiceName = "XblAuthManager",
                RecommendedStartup = ServiceStartMode.Disabled
            },
            new ServiceFeature
            {
                Title = "Xbox Live Game Save",
                Description = "Syncs game saves with Xbox Live cloud.",
                ServiceName = "XblGameSave",
                RecommendedStartup = ServiceStartMode.Disabled
            },
            new ServiceFeature
            {
                Title = "Xbox Live Networking",
                Description = "Supports Xbox Live networking functionality.",
                ServiceName = "XboxNetApiSvc",
                RecommendedStartup = ServiceStartMode.Disabled
            },

            // Other Microsoft Services
            new ServiceFeature
            {
                Title = "Cortana",
                Description = "Supports Cortana voice assistant functionality.",
                ServiceName = "Cortana",
                RecommendedStartup = ServiceStartMode.Disabled
            },
            new ServiceFeature
            {
                Title = "Microsoft Edge Update",
                Description = "Keeps Microsoft Edge browser updated.",
                ServiceName = "edgeupdate",
                RecommendedStartup = ServiceStartMode.Manual
            },
            new ServiceFeature
            {
                Title = "OneDrive Sync",
                Description = "Syncs files with OneDrive cloud storage.",
                ServiceName = "OneDrive",
                RecommendedStartup = ServiceStartMode.Disabled
            }
        ];

        public WindowsServices()
        {
            InitializeComponent();
            dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            LoadServiceStatus();
            FilteredServiceFeatures = new ObservableCollection<ServiceFeature>(ServiceFeatures);
            ServiceFeaturesListView.ItemsSource = FilteredServiceFeatures;

            // Initialization done, now allow SelectionChanged to act
            _isInitializing = false;
        }

        private void LoadServiceStatus()
        {
            bool hasErrors = false;

            foreach (var feature in ServiceFeatures)
            {
                try
                {
                    var svc = new ServiceController(feature.ServiceName);
                    feature.IsEnabled = svc.Status == ServiceControllerStatus.Running;
                    feature.StartupType = svc.StartType;
                }
                catch (Exception ex)
                {
                    feature.IsEnabled = false;
                    feature.StartupType = ServiceStartMode.Disabled;
                    hasErrors = true;

                    Debug.WriteLine($"Could not read status for {feature.Title}: {ex.Message}");
                }
            }

            if (hasErrors)
            {
                NotificationService.Instance.ShowWarning(
                    "Some services could not be loaded. They may not exist or require elevated permissions.",
                    "Partial Load Warning");
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = (sender as TextBox)?.Text?.ToLower();

            FilteredServiceFeatures.Clear();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (var item in ServiceFeatures)
                {
                    FilteredServiceFeatures.Add(item);
                }
            }
            else
            {
                foreach (var item in ServiceFeatures.Where(s =>
                    s.Title.ToLower().Contains(searchText) ||
                    s.Description.ToLower().Contains(searchText) ||
                    s.ServiceName.ToLower().Contains(searchText)))
                {
                    FilteredServiceFeatures.Add(item);
                }
            }
        }

        private async void ToggleService_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is ServiceFeature feature)
            {
                bool success = false;
                bool newState = !feature.IsEnabled;
                string action = newState ? "started" : "stopped";

                try
                {
                    await Task.Run(() =>
                    {
                        using var svc = new ServiceController(feature.ServiceName);
                        if (newState)
                        {
                            if (svc.Status != ServiceControllerStatus.Running)
                            {
                                svc.Start();
                                svc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                            }
                        }
                        else
                        {
                            if (svc.Status != ServiceControllerStatus.Stopped)
                            {
                                svc.Stop();
                                svc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                            }
                        }
                        success = true;
                    });

                    if (success)
                    {
                        // Refresh the service status
                        using (var svc = new ServiceController(feature.ServiceName))
                        {
                            await RunOnUiThreadAsync(() =>
                            {
                                feature.IsEnabled = svc.Status == ServiceControllerStatus.Running;
                                feature.StartupType = svc.StartType;
                            });
                        }

                        NotificationService.Instance.ShowSuccess(
                            $"{feature.Title} successfully {action}.",
                            "Service Toggled");
                    }
                }
                catch (Exception ex)
                {
                    await RunOnUiThreadAsync(() =>
                    {
                        NotificationService.Instance.ShowError(
                            $"Failed to toggle {feature.Title}. Error: {ex.Message}",
                            "Operation Failed");
                    });
                }
            }
        }

        private async void StartupTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
                return;  // Ignore during initialization

            if (sender is ComboBox comboBox &&
                comboBox.DataContext is ServiceFeature feature &&
                comboBox.SelectedValue is ServiceStartMode newStartupType)
            {
                try
                {
                    var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                        $@"SYSTEM\CurrentControlSet\Services\{feature.ServiceName}", true);

                    if (key != null)
                    {
                        int value = newStartupType switch
                        {
                            ServiceStartMode.Automatic => 2,
                            ServiceStartMode.Manual => 3,
                            ServiceStartMode.Disabled => 4,
                            _ => 3
                        };

                        key.SetValue("Start", value, Microsoft.Win32.RegistryValueKind.DWord);
                        key.Close();

                        await RunOnUiThreadAsync(() =>
                        {
                            feature.StartupType = newStartupType;
                            NotificationService.Instance.ShowSuccess(
                                $"Startup type for {feature.Title} set to {newStartupType}.",
                                "Startup Type Changed");
                        });
                    }
                    else
                    {
                        NotificationService.Instance.ShowError(
                            $"Could not find service registry for {feature.ServiceName}.",
                            "Registry Access Failed");
                    }
                }
                catch (Exception ex)
                {
                    NotificationService.Instance.ShowError(
                        $"Failed to set startup type for {feature.Title}: {ex.Message}",
                        "Error");
                }
            }
        }

        private Task RunOnUiThreadAsync(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            if (dispatcherQueue == null)
            {
                tcs.SetException(new InvalidOperationException("DispatcherQueue is null."));
                return tcs.Task;
            }

            if (!dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }))
            {
                tcs.SetException(new InvalidOperationException("Failed to enqueue action."));
            }

            return tcs.Task;
        }

        private void ShowInfo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: ServiceFeature feature })
            {
                InfoDialog.Title = feature.Title;
                InfoDialog.Content = $"{feature.Description}\n\n" +
                                    $"Service Name: {feature.ServiceName}\n" +
                                    $"Current Status: {(feature.IsEnabled ? "Running" : "Stopped")}\n" +
                                    $"Startup Type: {feature.StartupType}\n" +
                                    $"Recommended Startup: {feature.RecommendedStartup}";
                InfoDialog.XamlRoot = Content.XamlRoot;
                InfoDialog.ShowAsync();
            }
        }
    }

    public class ServiceFeature : INotifyPropertyChanged
    {
        private bool _isEnabled;
        private ServiceStartMode _startupType;

        public string Title { get; set; }
        public string Description { get; set; }
        public string ServiceName { get; set; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ToggleLabel));
                    OnPropertyChanged(nameof(StatusText));
                    OnPropertyChanged(nameof(StatusColor));
                }
            }
        }

        public ServiceStartMode StartupType
        {
            get => _startupType;
            set
            {
                if (_startupType != value)
                {
                    _startupType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StartupText));
                    OnPropertyChanged(nameof(StartupColor));
                }
            }
        }

        public ServiceStartMode RecommendedStartup { get; set; }

        public List<KeyValuePair<ServiceStartMode, string>> StartupTypes { get; } = new()
        {
            new(ServiceStartMode.Automatic, "Automatic"),
            new(ServiceStartMode.Manual, "Manual"),
            new(ServiceStartMode.Disabled, "Disabled")
        };

        public string ToggleLabel => IsEnabled ? "Stop" : "Start";
        public string StatusText => IsEnabled ? "Running" : "Stopped";
        public SolidColorBrush StatusColor => IsEnabled ?
            new SolidColorBrush(Microsoft.UI.Colors.Green) :
            new SolidColorBrush(Microsoft.UI.Colors.Red);

        public string StartupText => StartupType.ToString();
        public SolidColorBrush StartupColor => StartupType == RecommendedStartup ?
            new SolidColorBrush(Microsoft.UI.Colors.Green) :
            new SolidColorBrush(Microsoft.UI.Colors.Orange);

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}