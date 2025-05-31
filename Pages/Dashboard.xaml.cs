using DebloaterPro.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Management.Deployment;


namespace DebloaterPro.Pages
{
    public sealed partial class Dashboard : Page
    {
        public DashboardMetrics Metrics { get; set; } = new();
        public ObservableCollection<DashboardCard> SummaryCards { get; } = new();
        public ObservableCollection<RiskItem> RiskDistribution { get; } = new();
        public ObservableCollection<RecommendationItem> TopRecommendations { get; } = new();

        public int PrivacyScore => CalculatePrivacyScore();

        public string PrivacyStatusText => PrivacyScore switch
        {
            >= 90 => "Excellent - Your privacy is well protected",
            >= 70 => "Good - Some improvements possible",
            >= 50 => "Fair - Significant privacy risks",
            _ => "Poor - Immediate attention needed"
        };

        public DoubleCollection PrivacyScoreDashArray
        {
            get
            {
                double circumference = 2 * Math.PI * 100; // Approximate for our visual
                double dashLength = circumference * PrivacyScore / 100;
                return new DoubleCollection { dashLength, circumference };
            }
        }

        public Dashboard()
        {
            InitializeComponent();
            Loaded += Dashboard_Loaded;
        }

        private async void Dashboard_Loaded(object sender, RoutedEventArgs e)
        {
            // Show progress ring when loading starts
            progressRing.IsActive = true;

            // Load data asynchronously
            await LoadDashboardDataAsync();

            // Hide progress ring when loading is finished
            progressRing.IsActive = false;
        }

        private async Task LoadDashboardDataAsync()
        {
            // Start all data loading tasks concurrently
            var installedAppsTask = Task.Run(() => GetInstalledAppsCount());
            var dangerousAppsTask = Task.Run(() => GetDangerousAppsCount());
            var reclaimableSpaceTask = Task.Run(() => GetReclaimableSpaceMB());
            var privacyIssuesTask = Task.Run(() => GetPrivacyIssuesCount());
            var performanceTweaksTask = Task.Run(() => GetAvailablePerformanceTweaks());
            var nonOptimalServicesTask = Task.Run(() => GetNonOptimalServicesCount());

            // Wait for all tasks to complete
            await Task.WhenAll(
                installedAppsTask,
                dangerousAppsTask,
                reclaimableSpaceTask,
                privacyIssuesTask,
                performanceTweaksTask,
                nonOptimalServicesTask
            );

            // Update metrics with results
            Metrics.TotalApps = await installedAppsTask;
            Metrics.DangerousApps = await dangerousAppsTask;
            Metrics.ReclaimableSpace = await reclaimableSpaceTask;
            Metrics.PrivacyIssues = await privacyIssuesTask;
            Metrics.PerformanceTweaks = await performanceTweaksTask;
            Metrics.NonOptimalServices = await nonOptimalServicesTask;

            // Update risk distribution with real data
            UpdateRiskDistribution();

            // Update summary cards
            UpdateSummaryCards();

            // Generate recommendations based on real data
            GenerateRecommendations();
        }

        private int GetInstalledAppsCount()
        {
            try
            {
                var packageManager = new PackageManager();
                return packageManager.FindPackagesForUser(string.Empty)
                    .Count(pkg => !pkg.IsFramework && !string.IsNullOrEmpty(pkg.Id.Name));
            }
            catch
            {
                return 0;
            }
        }

        private int GetDangerousAppsCount()
        {
            try
            {
                var dangerousList = new Debloater().dangerousList;
                var packageManager = new PackageManager();
                return packageManager.FindPackagesForUser(string.Empty)
                    .Count(pkg => !pkg.IsFramework &&
                                !string.IsNullOrEmpty(pkg.Id.Name) &&
                                dangerousList.Any(d => pkg.Id.Name.Contains(d, StringComparison.OrdinalIgnoreCase)));
            }
            catch
            {
                return 0;
            }
        }

        private long GetReclaimableSpaceMB()
        {
            try
            {
                // Calculate temp files size
                var tempPath = Path.GetTempPath();
                long tempSize = Directory.GetFiles(tempPath).Sum(file => new FileInfo(file).Length);
                tempSize += Directory.GetDirectories(tempPath).Sum(dir => GetDirectorySize(dir));

                // Calculate Windows Update cleanup size
                long updateSize = 0;
                var updatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");
                if (Directory.Exists(updatePath))
                {
                    updateSize = GetDirectorySize(updatePath);
                }

                return (tempSize + updateSize) / (1024 * 1024); // Convert to MB
            }
            catch
            {
                return 0;
            }
        }

        private long GetDirectorySize(string path)
        {
            try
            {
                return Directory.GetFiles(path).Sum(file => new FileInfo(file).Length) +
                       Directory.GetDirectories(path).Sum(dir => GetDirectorySize(dir));
            }
            catch
            {
                return 0;
            }
        }

        private int GetPrivacyIssuesCount()
        {
            try
            {
                // Count privacy-related registry issues
                int issues = 0;

                // Check telemetry settings
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection"))
                {
                    if (key?.GetValue("AllowTelemetry") is int telemetry && telemetry > 0)
                        issues++;
                }

                // Check diagnostic data settings
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Diagnostics\DiagTrack"))
                {
                    if (key?.GetValue("ShowDiagnosticDataViewerButton") is int diag && diag == 1)
                        issues++;
                }

                // Check advertising ID
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo"))
                {
                    if (key?.GetValue("Enabled") is int enabled && enabled == 1)
                        issues++;
                }

                return issues;
            }
            catch
            {
                return 0;
            }
        }

        private int GetAvailablePerformanceTweaks()
        {
            try
            {
                int tweaks = 0;

                // Check Superfetch status
                var superfetch = new ServiceController("SysMain");
                if (superfetch.Status == ServiceControllerStatus.Running)
                    tweaks++;

                // Check hibernation status
                var psi = new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = "/a",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    if (output.Contains("Hibernation has not been enabled"))
                        tweaks++;
                }

                // Check power plan
                using (var process = Process.Start(new ProcessStartInfo("powercfg.exe", "/getactivescheme")))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    if (!output.Contains("High performance") && !output.Contains("e9a42b02-d5df-448d-aa00-03f14749eb61"))
                        tweaks++;
                }

                return tweaks;
            }
            catch
            {
                return 0;
            }
        }

        private int GetNonOptimalServicesCount()
        {
            try
            {
                var nonOptimalServices = new[]
                {
                    "DiagTrack",  // Connected User Experiences and Telemetry
                    "WSearch",    // Windows Search (if SSD)
                    "XblAuthManager", // Xbox Live Auth Manager
                    "XblGameSave",    // Xbox Live Game Save
                    "XboxNetApiSvc",   // Xbox Live Networking Service
                    "OneDrive",        // OneDrive sync
                    "MapsBroker",      // Downloaded Maps Manager
                    "lfsvc",           // Geolocation Service
                    "RemoteRegistry"   // Remote Registry
                };

                return nonOptimalServices.Count(service =>
                {
                    try
                    {
                        var svc = new ServiceController(service);
                        return svc.StartType != ServiceStartMode.Disabled;
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
            catch
            {
                return 0;
            }
        }

        private void UpdateRiskDistribution()
        {
            RiskDistribution.Clear();

            try
            {
                var packageManager = new PackageManager();
                var packages = packageManager.FindPackagesForUser(string.Empty)
                    .Where(pkg => !pkg.IsFramework && !string.IsNullOrEmpty(pkg.Id.Name))
                    .ToList();

                var criticalSystemApps = new Debloater().criticalSystemApps;
                var recommendedList = new Debloater().recommendedList;
                var dangerousList = new Debloater().dangerousList;

                int criticalCount = packages.Count(pkg =>
                    criticalSystemApps.Any(c => pkg.Id.Name.Contains(c, StringComparison.OrdinalIgnoreCase)));

                int dangerousCount = packages.Count(pkg =>
                    dangerousList.Any(d => pkg.Id.Name.Contains(d, StringComparison.OrdinalIgnoreCase)));

                int moderateCount = packages.Count(pkg =>
                    recommendedList.Any(r => pkg.Id.Name.Contains(r, StringComparison.OrdinalIgnoreCase)));

                int safeCount = packages.Count - criticalCount - dangerousCount - moderateCount;

                RiskDistribution.Add(new RiskItem
                {
                    RiskLevel = "Critical",
                    Count = criticalCount,
                    RiskBrush = new SolidColorBrush(Colors.Red)
                });

                RiskDistribution.Add(new RiskItem
                {
                    RiskLevel = "Dangerous",
                    Count = dangerousCount,
                    RiskBrush = new SolidColorBrush(Colors.Orange)
                });

                RiskDistribution.Add(new RiskItem
                {
                    RiskLevel = "Moderate",
                    Count = moderateCount,
                    RiskBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 204, 102))
                });

                RiskDistribution.Add(new RiskItem
                {
                    RiskLevel = "Safe",
                    Count = safeCount,
                    RiskBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 102, 204, 102))
                });
            }
            catch
            {
                // Fallback to default values if something goes wrong
                RiskDistribution.Add(new RiskItem { RiskLevel = "Critical", Count = 0, RiskBrush = new SolidColorBrush(Colors.Red) });
                RiskDistribution.Add(new RiskItem { RiskLevel = "Dangerous", Count = 0, RiskBrush = new SolidColorBrush(Colors.Orange) });
                RiskDistribution.Add(new RiskItem { RiskLevel = "Moderate", Count = 0, RiskBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 204, 102)) });
                RiskDistribution.Add(new RiskItem { RiskLevel = "Safe", Count = 0, RiskBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 102, 204, 102)) });
            }
        }

        private void UpdateSummaryCards()
        {
            SummaryCards.Clear();

            SummaryCards.Add(new DashboardCard
            {
                Title = "Bloatware Apps",
                Value = Metrics.DangerousApps.ToString(),
                StatusText = $"{Metrics.DangerousApps} to remove",
                StatusBrush = new SolidColorBrush(Colors.Orange),
                ProgressValue = Metrics.TotalApps > 0 ? (double)Metrics.DangerousApps / Metrics.TotalApps * 100 : 0
            });

            SummaryCards.Add(new DashboardCard
            {
                Title = "Privacy Score",
                Value = $"{PrivacyScore}/100",
                StatusText = PrivacyStatusText,
                StatusBrush = PrivacyScore >= 70 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Orange),
                ProgressValue = PrivacyScore
            });

            SummaryCards.Add(new DashboardCard
            {
                Title = "Reclaimable Space",
                Value = $"{Metrics.ReclaimableSpace} MB",
                StatusText = "Potential savings",
                StatusBrush = new SolidColorBrush(Colors.Green),
                ProgressValue = Metrics.ReclaimableSpace > 0 ? Math.Min(100, Metrics.ReclaimableSpace / 10) : 0
            });

            SummaryCards.Add(new DashboardCard
            {
                Title = "Performance Tweaks",
                Value = Metrics.PerformanceTweaks.ToString(),
                StatusText = "Available optimizations",
                StatusBrush = new SolidColorBrush(Colors.Orange),
                ProgressValue = Metrics.PerformanceTweaks > 0 ? Metrics.PerformanceTweaks * 20 : 0
            });

            SummaryCards.Add(new DashboardCard
            {
                Title = "Services",
                Value = Metrics.NonOptimalServices.ToString(),
                StatusText = "Non-optimal services",
                StatusBrush = new SolidColorBrush(Colors.Orange),
                ProgressValue = Metrics.NonOptimalServices > 0 ? Metrics.NonOptimalServices * 15 : 0
            });
        }

        private static int CalculatePrivacyScore()
        {
            int score = 100; // Start with perfect score

            // Deduct points for each privacy issue
            try
            {
                // Check telemetry settings
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection"))
                {
                    if (key?.GetValue("AllowTelemetry") is int telemetry)
                    {
                        score -= telemetry switch
                        {
                            0 => 0,   // Security (Enterprise only) - no deduction
                            1 => 10,  // Basic - small deduction
                            2 => 20,  // Enhanced - medium deduction
                            3 => 30,  // Full - large deduction
                            _ => 0
                        };
                    }
                }

                // Check diagnostic data settings
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Diagnostics\DiagTrack"))
                {
                    if (key?.GetValue("ShowDiagnosticDataViewerButton") is int diag && diag == 1)
                        score -= 15;
                }

                // Check advertising ID
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo"))
                {
                    if (key?.GetValue("Enabled") is int enabled && enabled == 1)
                        score -= 10;
                }

                // Check location tracking
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Sensor\Overrides\{BFA794E4-F964-4FDB-90F6-51056BFE4B44}"))
                {
                    if (key?.GetValue("SensorPermissionState") is int location && location == 1)
                        score -= 10;
                }

                // Check Cortana
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search"))
                {
                    if (key?.GetValue("AllowCortana") is int cortana && cortana == 1)
                        score -= 5;
                }
            }
            catch
            {
                // If we can't check settings, assume worst case
                score = 50;
            }

            return Math.Max(0, Math.Min(100, score));
        }

        private void GenerateRecommendations()
        {
            TopRecommendations.Clear();

            if (Metrics.DangerousApps > 0)
            {
                TopRecommendations.Add(new RecommendationItem
                {
                    Icon = "\uE74C", // Remove icon
                    Description = $"Remove {Metrics.DangerousApps} dangerous apps to improve security",
                    ApplyCommand = new DelegateCommand(() => NavigationService.Instance.Navigate(typeof(Debloater), "DebloaterPage"))
                });
            }

            if (PrivacyScore < 70)
            {
                TopRecommendations.Add(new RecommendationItem
                {
                    Icon = "\uE72E", // Privacy icon
                    Description = "Apply recommended privacy settings to increase your privacy score",
                    ApplyCommand = new DelegateCommand(() => NavigationService.Instance.Navigate(typeof(Privacy), "PrivacyPage"))
                });
            }

            if (Metrics.PerformanceTweaks > 0)
            {
                TopRecommendations.Add(new RecommendationItem
                {
                    Icon = "\uE9A9", // Performance icon
                    Description = "Apply performance tweaks for better system responsiveness",
                    ApplyCommand = new DelegateCommand(() => NavigationService.Instance.Navigate(typeof(Tweaks), "TweaksPage"))
                });
            }

            if (Metrics.NonOptimalServices > 0)
            {
                TopRecommendations.Add(new RecommendationItem
                {
                    Icon = "\uE7F4", // Services icon
                    Description = $"Optimize {Metrics.NonOptimalServices} services for better performance",
                    ApplyCommand = new DelegateCommand(() => NavigationService.Instance.Navigate(typeof(WindowsServices), "WindowsServicesPage"))
                });
            }

            if (Metrics.ReclaimableSpace > 500) // Only recommend if significant space can be reclaimed
            {
                TopRecommendations.Add(new RecommendationItem
                {
                    Icon = "\uE9CE", // Storage icon
                    Description = $"Clean up {Metrics.ReclaimableSpace} MB of temporary files",
                    ApplyCommand = new DelegateCommand(() => NavigationService.Instance.Navigate(typeof(Tools), "ToolsPage"))
                });
            }

            // Add at least one recommendation if none were added
            if (TopRecommendations.Count == 0)
            {
                TopRecommendations.Add(new RecommendationItem
                {
                    Icon = "\uE73E", // Checkmark icon
                    Description = "Your system is in good condition. No critical issues found.",
                    ApplyCommand = new DelegateCommand(() => { })
                });
            }
        }
    }

    public class DashboardMetrics
    {
        public int TotalApps { get; set; }
        public int DangerousApps { get; set; }
        public int PrivacyIssues { get; set; }
        public long ReclaimableSpace { get; set; } // in MB
        public int NonOptimalServices { get; set; }
        public int PerformanceTweaks { get; set; }
    }

    public class DashboardCard
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string StatusText { get; set; }
        public SolidColorBrush StatusBrush { get; set; }
        public double ProgressValue { get; set; }
    }

    public class RiskItem
    {
        public string RiskLevel { get; set; }
        public int Count { get; set; }
        public SolidColorBrush RiskBrush { get; set; }
    }

    public class RecommendationItem
    {
        public string Icon { get; set; }
        public string Description { get; set; }
        public ICommand ApplyCommand { get; set; }
    }
}