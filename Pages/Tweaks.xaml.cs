using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using System.Threading.Tasks;


namespace DebloaterPro.Pages
{
    public sealed partial class Tweaks : Page
    {
        public Tweaks()
        {
            InitializeComponent();
            LoadInitialStates();
        }

        private async Task LoadInitialStates()
        {
            await Task.WhenAll(
                CheckHibernateStatus(),
                CheckSuperfetchStatus(),
                CheckPowerPlanStatus(),
                CheckGameBarStatus(),
                CheckTipsStatus(),
                CheckXboxServicesStatus(),
                CheckBackgroundAppsStatus(),
                CheckCortanaStatus(),
                CheckWindowsUpdateStatus(),
                CheckNetworkThrottlingStatus()
            );
        }

        #region Hibernate Functions
        private async Task CheckHibernateStatus()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powercfg",
                        Arguments = "/a",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                bool isEnabled = !output.Contains("Hibernation has not been enabled.");
                DispatcherQueue.TryEnqueue(() =>
                {
                    HibernateStatusText.Text = isEnabled ? "Status: Enabled" : "Status: Disabled";
                    DisableHibernateBtn.IsEnabled = isEnabled;
                    EnableHibernateBtn.IsEnabled = !isEnabled;
                });
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    HibernateStatusText.Text = $"Status: Error - {ex.Message}";
                });
            }
        }

        private async void CheckHibernateStatus_Click(object sender, RoutedEventArgs e)
        {
            await CheckHibernateStatus();
        }

        private async void ToggleHibernate_Click(object sender, RoutedEventArgs e)
        {
            string action = (sender as Button)?.Content.ToString()?.ToLower();
            if (string.IsNullOrEmpty(action)) return;

            try
            {
                Process.Start(new ProcessStartInfo("powercfg", $"/hibernate {action}") { Verb = "runas" });
                await CheckHibernateStatus();
                ShowMessage($"Hibernation has been {action}d successfully.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to {action} hibernation: {ex.Message}");
            }
        }
        #endregion

        #region Superfetch Functions
        private async Task CheckSuperfetchStatus()
        {
            try
            {
                var svc = new ServiceController("SysMain");
                bool isRunning = svc.Status == ServiceControllerStatus.Running;

                DispatcherQueue.TryEnqueue(() =>
                {
                    SuperfetchStatusText.Text = $"Status: {(isRunning ? "Running" : "Stopped")}";
                    DisableSuperfetchBtn.IsEnabled = isRunning;
                    EnableSuperfetchBtn.IsEnabled = !isRunning;
                });
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    SuperfetchStatusText.Text = $"Status: Error - {ex.Message}";
                });
            }
        }

        private async void CheckSuperfetchStatus_Click(object sender, RoutedEventArgs e)
        {
            await CheckSuperfetchStatus();
        }

        private async void ToggleSuperfetch_Click(object sender, RoutedEventArgs e)
        {
            string action = (sender as Button)?.Content.ToString()?.ToLower();
            if (string.IsNullOrEmpty(action)) return;

            try
            {
                var svc = new ServiceController("SysMain");
                if (action == "disable" && svc.Status != ServiceControllerStatus.Stopped)
                {
                    svc.Stop();
                }
                else if (action == "enable" && svc.Status != ServiceControllerStatus.Running)
                {
                    svc.Start();
                }

                await CheckSuperfetchStatus();
                ShowMessage($"Superfetch has been {action}d successfully.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to {action} Superfetch: {ex.Message}");
            }
        }
        #endregion

        #region Power Plan Functions
        private async Task CheckPowerPlanStatus()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powercfg",
                        Arguments = "/getactivescheme",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                DispatcherQueue.TryEnqueue(() =>
                {
                    PowerPlanStatusText.Text = $"Current Plan: {output.Split(':')[1].Trim()}";
                });
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    PowerPlanStatusText.Text = $"Status: Error - {ex.Message}";
                });
            }
        }

        private async void SetPowerPlan_Click(object sender, RoutedEventArgs e)
        {
            string guid = (sender as Button)?.Tag as string;
            if (string.IsNullOrEmpty(guid)) return;

            try
            {
                Process.Start(new ProcessStartInfo("powercfg", $"/setactive {guid}") { Verb = "runas" });
                await CheckPowerPlanStatus();
                ShowMessage("Power plan changed successfully.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to change power plan: {ex.Message}");
            }
        }
        #endregion

        #region Game Bar Functions
        private async Task CheckGameBarStatus()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\GameDVR"))
                {
                    int? value = (int?)key?.GetValue("AppCaptureEnabled");
                    bool isEnabled = value.HasValue && value.Value == 1;

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        GameBarStatusText.Text = $"Status: {(isEnabled ? "Enabled" : "Disabled")}";
                    });
                }
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    GameBarStatusText.Text = $"Status: Error - {ex.Message}";
                });
            }
        }

        private async void CheckGameBarStatus_Click(object sender, RoutedEventArgs e)
        {
            await CheckGameBarStatus();
        }

        private async void ToggleGameBar_Click(object sender, RoutedEventArgs e)
        {
            string action = (sender as Button)?.Tag as string;
            if (string.IsNullOrEmpty(action)) return;

            try
            {
                int value = action == "enable" ? 1 : 0;
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\GameDVR"))
                {
                    key.SetValue("AppCaptureEnabled", value, RegistryValueKind.DWord);
                }

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\GameDVR"))
                {
                    key.SetValue("AllowGameDVR", value, RegistryValueKind.DWord);
                }

                await CheckGameBarStatus();
                ShowMessage($"Game Bar has been {action}d successfully.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to {action} Game Bar: {ex.Message}");
            }
        }
        #endregion

        #region Windows Tips Functions
        private async Task CheckTipsStatus()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"))
                {
                    int? value = (int?)key?.GetValue("SystemPaneSuggestionsEnabled");
                    bool isEnabled = value.HasValue && value.Value == 1;

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        TipsStatusText.Text = $"Status: {(isEnabled ? "Enabled" : "Disabled")}";
                    });
                }
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    TipsStatusText.Text = $"Status: Error - {ex.Message}";
                });
            }
        }

        private async void CheckTipsStatus_Click(object sender, RoutedEventArgs e)
        {
            await CheckTipsStatus();
        }

        private async void ToggleTips_Click(object sender, RoutedEventArgs e)
        {
            string action = (sender as Button)?.Tag as string;
            if (string.IsNullOrEmpty(action)) return;

            try
            {
                int value = action == "enable" ? 1 : 0;
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"))
                {
                    key.SetValue("SystemPaneSuggestionsEnabled", value, RegistryValueKind.DWord);
                    key.SetValue("SubscribedContent-338388Enabled", value, RegistryValueKind.DWord);
                    key.SetValue("SubscribedContent-353694Enabled", value, RegistryValueKind.DWord);
                    key.SetValue("SubscribedContent-353696Enabled", value, RegistryValueKind.DWord);
                    key.SetValue("SoftLandingEnabled", value, RegistryValueKind.DWord);
                }

                await CheckTipsStatus();
                ShowMessage($"Windows Tips have been {action}d successfully.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to {action} Windows Tips: {ex.Message}");
            }
        }
        #endregion

        #region Taskbar Functions
        private void ToggleTaskbarSearch_Click(object sender, RoutedEventArgs e)
        {
            string action = (sender as Button)?.Tag as string;
            if (string.IsNullOrEmpty(action)) return;

            try
            {
                string value = action == "enable" ? "1" : "0";
                Process.Start(new ProcessStartInfo("reg",
                    $"add \"HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Search\" /v SearchboxTaskbarMode /t REG_DWORD /d {value} /f")
                { Verb = "runas" });

                ShowMessage($"Taskbar search has been {action}d. Restart Explorer to see changes.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to {action} taskbar search: {ex.Message}");
            }
        }

        private void ToggleTaskbarSize_Click(object sender, RoutedEventArgs e)
        {
            string? size = (sender as Button)?.Tag as string;
            if (string.IsNullOrEmpty(size)) return;

            try
            {
                string value = size == "small" ? "0" : "1";
                Process.Start(new ProcessStartInfo("reg",
                    $"add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced\" /v TaskbarSmallIcons /t REG_DWORD /d {value} /f")
                { Verb = "runas" });

                ShowMessage($"Taskbar icons set to {size}. Restart Explorer to see changes.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to change taskbar icon size: {ex.Message}");
            }
        }
        #endregion

        #region File Explorer Functions
        private void ToggleFileExplorerOption_Click(object sender, RoutedEventArgs e)
        {
            string? option = (sender as Button)?.Tag as string;
            if (string.IsNullOrEmpty(option)) return;

            try
            {
                switch (option)
                {
                    case "ShowExtensions":
                        Process.Start(new ProcessStartInfo("reg",
                            @"add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v HideFileExt /t REG_DWORD /d 0 /f")
                        { Verb = "runas" });
                        break;
                    case "ShowHidden":
                        Process.Start(new ProcessStartInfo("reg",
                            @"add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v Hidden /t REG_DWORD /d 1 /f")
                        { Verb = "runas" });
                        break;
                    case "CompactMode":
                        Process.Start(new ProcessStartInfo("reg",
                            @"add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v UseCompactMode /t REG_DWORD /d 1 /f")
                        { Verb = "runas" });
                        break;
                    case "DetailsView":
                        Process.Start(new ProcessStartInfo("reg",
                            @"add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v AlwaysShowMenus /t REG_DWORD /d 1 /f")
                        { Verb = "runas" });
                        break;
                }

                ShowMessage($"File Explorer option '{option}' has been changed. Restart Explorer to see changes.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to change File Explorer option: {ex.Message}");
            }
        }
        #endregion

        #region Xbox Services Functions
        private async Task CheckXboxServicesStatus()
        {
            try
            {
                string[] xboxServices = { "XblAuthManager", "XblGameSave", "XboxNetApiSvc" };
                int runningCount = 0;

                foreach (var serviceName in xboxServices)
                {
                    try
                    {
                        var svc = new ServiceController(serviceName);
                        if (svc.Status == ServiceControllerStatus.Running)
                            runningCount++;
                    }
                    catch { /* Ignore services that don't exist */ }
                }

                DispatcherQueue.TryEnqueue(() =>
                {
                    XboxServicesStatusText.Text = $"Status: {runningCount}/{xboxServices.Length} services running";
                });
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    XboxServicesStatusText.Text = $"Status: Error - {ex.Message}";
                });
            }
        }

        private async void CheckXboxServicesStatus_Click(object sender, RoutedEventArgs e)
        {
            await CheckXboxServicesStatus();
        }

        private async void ToggleXboxServices_Click(object sender, RoutedEventArgs e)
        {
            string action = (sender as Button)?.Tag as string;
            if (string.IsNullOrEmpty(action)) return;

            try
            {
                string[] xboxServices = { "XblAuthManager", "XblGameSave", "XboxNetApiSvc" };
                foreach (var serviceName in xboxServices)
                {
                    try
                    {
                        var svc = new ServiceController(serviceName);
                        if (action == "disable" && svc.Status != ServiceControllerStatus.Stopped)
                        {
                            svc.Stop();
                        }
                        else if (action == "enable" && svc.Status != ServiceControllerStatus.Running)
                        {
                            svc.Start();
                        }
                    }
                    catch { /* Ignore services that don't exist */ }
                }

                await CheckXboxServicesStatus();
                ShowMessage($"Xbox services have been {action}d successfully.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to {action} Xbox services: {ex.Message}");
            }
        }
        #endregion

        #region Background Apps Functions
        private async Task CheckBackgroundAppsStatus()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications"))
                {
                    int? value = (int?)key?.GetValue("GlobalUserDisabled");
                    bool isEnabled = !value.HasValue || value.Value == 0;

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        BackgroundAppsStatusText.Text = $"Status: {(isEnabled ? "Enabled" : "Disabled")}";
                    });
                }
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    BackgroundAppsStatusText.Text = $"Status: Error - {ex.Message}";
                });
            }
        }

        private async void CheckBackgroundAppsStatus_Click(object sender, RoutedEventArgs e)
        {
            await CheckBackgroundAppsStatus();
        }

        private async void ToggleBackgroundApps_Click(object sender, RoutedEventArgs e)
        {
            string action = (sender as Button)?.Tag as string;
            if (string.IsNullOrEmpty(action)) return;

            try
            {
                int value = action == "disable" ? 1 : 0;
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications"))
                {
                    key.SetValue("GlobalUserDisabled", value, RegistryValueKind.DWord);
                }

                await CheckBackgroundAppsStatus();
                ShowMessage($"Background apps have been {action}d successfully.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to {action} background apps: {ex.Message}");
            }
        }
        #endregion

        #region Cortana Functions
        private async Task CheckCortanaStatus()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search"))
                {
                    int? value = (int?)key?.GetValue("AllowCortana");
                    bool isEnabled = !value.HasValue || value.Value == 1;

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        CortanaStatusText.Text = $"Status: {(isEnabled ? "Enabled" : "Disabled")}";
                    });
                }
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    CortanaStatusText.Text = $"Status: Error - {ex.Message}";
                });
            }
        }

        private async void CheckCortanaStatus_Click(object sender, RoutedEventArgs e)
        {
            await CheckCortanaStatus();
        }

        private async void ToggleCortana_Click(object sender, RoutedEventArgs e)
        {
            string action = (sender as Button)?.Tag as string;
            if (string.IsNullOrEmpty(action)) return;

            try
            {
                int value = action == "disable" ? 0 : 1;
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search"))
                {
                    key.SetValue("AllowCortana", value, RegistryValueKind.DWord);
                }

                await CheckCortanaStatus();
                ShowMessage($"Cortana has been {action}d successfully.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to {action} Cortana: {ex.Message}");
            }
        }
        #endregion

        #region Windows Update Functions
        private async Task CheckWindowsUpdateStatus()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU"))
                {
                    int? value = (int?)key?.GetValue("NoAutoUpdate");
                    bool isEnabled = !value.HasValue || value.Value == 0;

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        WindowsUpdateStatusText.Text = $"Auto Updates: {(isEnabled ? "Enabled" : "Disabled")}";
                    });
                }
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    WindowsUpdateStatusText.Text = $"Status: Error - {ex.Message}";
                });
            }
        }

        private async void ToggleWindowsUpdate_Click(object sender, RoutedEventArgs e)
        {
            string action = (sender as Button)?.Tag as string;
            if (string.IsNullOrEmpty(action)) return;

            try
            {
                int value = action == "disable" ? 1 : 0;
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU"))
                {
                    key.SetValue("NoAutoUpdate", value, RegistryValueKind.DWord);
                }

                await CheckWindowsUpdateStatus();
                ShowMessage($"Windows Auto Update has been {action}d successfully.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to {action} Windows Auto Update: {ex.Message}");
            }
        }

        private async void ToggleWindowsUpdateP2P_Click(object sender, RoutedEventArgs e)
        {
            string action = (sender as Button)?.Tag as string;
            if (string.IsNullOrEmpty(action)) return;

            try
            {
                int value = action == "disable" ? 0 : 1;
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config"))
                {
                    key.SetValue("DODownloadMode", value, RegistryValueKind.DWord);
                }

                ShowMessage($"Windows Update P2P sharing has been {action}d successfully.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to {action} Windows Update P2P sharing: {ex.Message}");
            }
        }
        #endregion

        #region Network Functions
        private async Task CheckNetworkThrottlingStatus()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"))
                {
                    int? value = (int?)key?.GetValue("NetworkThrottlingIndex");
                    bool isEnabled = value.HasValue && value.Value == 0xFFFFFFFF;

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        NetworkThrottlingStatusText.Text = $"Status: {(isEnabled ? "Disabled" : "Enabled")}";
                    });
                }
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    NetworkThrottlingStatusText.Text = $"Status: Error - {ex.Message}";
                });
            }
        }

        private async void CheckNetworkThrottlingStatus_Click(object sender, RoutedEventArgs e)
        {
            await CheckNetworkThrottlingStatus();
        }

        private async void ToggleNetworkThrottling_Click(object sender, RoutedEventArgs e)
        {
            string action = (sender as Button)?.Tag as string;
            if (string.IsNullOrEmpty(action)) return;

            try
            {
                int value = (int)(action == "disable" ? 0xFFFFFFFF : 10); // 10 is default throttling index
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"))
                {
                    key.SetValue("NetworkThrottlingIndex", value, RegistryValueKind.DWord);
                }

                await CheckNetworkThrottlingStatus();
                ShowMessage($"Network throttling has been {action}d successfully.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to {action} network throttling: {ex.Message}");
            }
        }

        private void SetDNSServer_Click(object sender, RoutedEventArgs e)
        {
            string dnsServers = (sender as Button)?.Tag as string;
            if (string.IsNullOrEmpty(dnsServers)) return;

            try
            {
                string[] servers = dnsServers.Split(',');
                string primaryDNS = servers[0];
                string secondaryDNS = servers.Length > 1 ? servers[1] : "";

                // This is a simplified version - in a real app you'd want to:
                // 1. Get all network interfaces
                // 2. Let user choose which to configure
                // 3. Apply DNS settings to selected interface
                Process.Start(new ProcessStartInfo("netsh",
                    $"interface ip set dns name=\"Ethernet\" source=static addr={primaryDNS}")
                { Verb = "runas" });

                if (!string.IsNullOrEmpty(secondaryDNS))
                {
                    Process.Start(new ProcessStartInfo("netsh",
                        $"interface ip add dns name=\"Ethernet\" addr={secondaryDNS} index=2")
                    { Verb = "runas" });
                }

                DNSStatusText.Text = $"DNS set to: {primaryDNS}" +
                    (string.IsNullOrEmpty(secondaryDNS) ? "" : $", {secondaryDNS}");
                ShowMessage("DNS servers have been updated.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to set DNS servers: {ex.Message}");
            }
        }

        private void ResetDNSServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("netsh",
                    "interface ip set dns name=\"Ethernet\" source=dhcp")
                { Verb = "runas" });

                DNSStatusText.Text = "DNS reset to DHCP";
                ShowMessage("DNS settings have been reset to obtain automatically.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to reset DNS: {ex.Message}");
            }
        }
        #endregion

        #region One-Click Functions
        private async void ApplyPerformanceTweaks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disable Hibernation
                Process.Start(new ProcessStartInfo("powercfg", "/hibernate off") { Verb = "runas" });

                // Disable Superfetch
                var svc = new ServiceController("SysMain");
                if (svc.Status != ServiceControllerStatus.Stopped)
                    svc.Stop();

                // Set Ultimate Performance power plan
                Process.Start(new ProcessStartInfo("powercfg", "/setactive e9a42b02-d5df-448d-aa00-03f14749eb61") { Verb = "runas" });

                // Disable Game Bar
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\GameDVR"))
                {
                    key.SetValue("AppCaptureEnabled", 0, RegistryValueKind.DWord);
                }

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\GameDVR"))
                {
                    key.SetValue("AllowGameDVR", 0, RegistryValueKind.DWord);
                }

                // Disable Windows Tips
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"))
                {
                    key.SetValue("SystemPaneSuggestionsEnabled", 0, RegistryValueKind.DWord);
                    key.SetValue("SubscribedContent-338388Enabled", 0, RegistryValueKind.DWord);
                }

                // Disable Xbox services
                string[] xboxServices = { "XblAuthManager", "XblGameSave", "XboxNetApiSvc" };
                foreach (var serviceName in xboxServices)
                {
                    try
                    {
                        var service = new ServiceController(serviceName);
                        if (service.Status != ServiceControllerStatus.Stopped)
                            service.Stop();
                    }
                    catch { /* Ignore services that don't exist */ }
                }

                // Disable background apps
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications"))
                {
                    key.SetValue("GlobalUserDisabled", 1, RegistryValueKind.DWord);
                }

                // Disable network throttling
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"))
                {
                    key.SetValue("NetworkThrottlingIndex", 0xFFFFFFFF, RegistryValueKind.DWord);
                }

                await LoadInitialStates();
                ShowMessage("Performance tweaks applied successfully!");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to apply performance tweaks: {ex.Message}");
            }
        }

        private async void ApplyPrivacyTweaks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disable Telemetry
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection"))
                {
                    key.SetValue("AllowTelemetry", 0, RegistryValueKind.DWord);
                }

                // Disable Error Reporting
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\Windows Error Reporting"))
                {
                    key.SetValue("Disabled", 1, RegistryValueKind.DWord);
                }

                // Disable Advertising ID
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo"))
                {
                    key.SetValue("Enabled", 0, RegistryValueKind.DWord);
                }

                // Disable Location Tracking
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Sensor\Overrides\{BFA794E4-F964-4FDB-90F6-51056BFE4B44}"))
                {
                    key.SetValue("SensorPermissionState", 0, RegistryValueKind.DWord);
                }

                // Disable Cortana
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search"))
                {
                    key.SetValue("AllowCortana", 0, RegistryValueKind.DWord);
                }

                // Disable Windows Update P2P
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config"))
                {
                    key.SetValue("DODownloadMode", 0, RegistryValueKind.DWord);
                }

                await LoadInitialStates();
                ShowMessage("Privacy tweaks applied successfully!");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to apply privacy tweaks: {ex.Message}");
            }
        }

        private async void RestoreDefaults_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Enable Hibernation
                Process.Start(new ProcessStartInfo("powercfg", "/hibernate on") { Verb = "runas" });

                // Enable Superfetch
                var svc = new ServiceController("SysMain");
                if (svc.Status != ServiceControllerStatus.Running)
                    svc.Start();

                // Set Balanced power plan
                Process.Start(new ProcessStartInfo("powercfg", "/setactive 381b4222-f694-41f0-9685-ff5bb260df2e") { Verb = "runas" });

                // Enable Game Bar
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\GameDVR"))
                {
                    key.SetValue("AppCaptureEnabled", 1, RegistryValueKind.DWord);
                }

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\GameDVR"))
                {
                    key.SetValue("AllowGameDVR", 1, RegistryValueKind.DWord);
                }

                // Enable Windows Tips
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"))
                {
                    key.SetValue("SystemPaneSuggestionsEnabled", 1, RegistryValueKind.DWord);
                    key.SetValue("SubscribedContent-338388Enabled", 1, RegistryValueKind.DWord);
                }

                // Enable Xbox services
                string[] xboxServices = { "XblAuthManager", "XblGameSave", "XboxNetApiSvc" };
                foreach (var serviceName in xboxServices)
                {
                    try
                    {
                        var service = new ServiceController(serviceName);
                        if (service.Status != ServiceControllerStatus.Running)
                            service.Start();
                    }
                    catch { /* Ignore services that don't exist */ }
                }

                // Enable background apps
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications"))
                {
                    key.SetValue("GlobalUserDisabled", 0, RegistryValueKind.DWord);
                }

                // Enable network throttling
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"))
                {
                    key.SetValue("NetworkThrottlingIndex", 10, RegistryValueKind.DWord);
                }

                // Reset privacy settings
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection"))
                {
                    key.SetValue("AllowTelemetry", 1, RegistryValueKind.DWord);
                }

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\Windows Error Reporting"))
                {
                    key.SetValue("Disabled", 0, RegistryValueKind.DWord);
                }

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo"))
                {
                    key.SetValue("Enabled", 1, RegistryValueKind.DWord);
                }

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Sensor\Overrides\{BFA794E4-F964-4FDB-90F6-51056BFE4B44}"))
                {
                    key.SetValue("SensorPermissionState", 1, RegistryValueKind.DWord);
                }

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search"))
                {
                    key.SetValue("AllowCortana", 1, RegistryValueKind.DWord);
                }

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config"))
                {
                    key.SetValue("DODownloadMode", 1, RegistryValueKind.DWord);
                }

                await LoadInitialStates();
                ShowMessage("Default settings restored successfully!");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to restore defaults: {ex.Message}");
            }
        }
        #endregion

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
}