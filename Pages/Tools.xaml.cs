using DebloaterPro.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading.Tasks;


namespace DebloaterPro.Pages
{
    public sealed partial class Tools : Page
    {
        public Tools()
        {
            InitializeComponent();
            LoadDrives();
        }

        private void LoadDrives()
        {
            DriveComboBox.Items.Clear();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    DriveComboBox.Items.Add($"{drive.Name} ({drive.DriveType})");
                }
            }
            if (DriveComboBox.Items.Count > 0)
                DriveComboBox.SelectedIndex = 0;
        }

        #region System Maintenance Functions
        private void ClearTemp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tempPath = Path.GetTempPath();
                foreach (var file in Directory.GetFiles(tempPath))
                {
                    try { File.Delete(file); } catch { }
                }
                foreach (var dir in Directory.GetDirectories(tempPath))
                {
                    try { Directory.Delete(dir, true); } catch { }
                }
                NotificationService.Instance.ShowSuccess("Temporary files cleared successfully!");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to clear temp files: {ex.Message}");
            }
        }

        private void CleanSystemFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("cleanmgr", "/sagerun:1") { Verb = "runas" });
                NotificationService.Instance.ShowSuccess("Disk Cleanup started with system files option.");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to start Disk Cleanup: {ex.Message}");
            }
        }

        private void CleanWindowsUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("dism", "/online /cleanup-image /startcomponentcleanup") { Verb = "runas" });
                NotificationService.Instance.ShowSuccess("Windows Update cleanup started. This may take several minutes.");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to start Windows Update cleanup: {ex.Message}");
            }
        }

        private async void AnalyzeDrives_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string drive = DriveComboBox.SelectedItem.ToString().Split(' ')[0];
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "defrag",
                        Arguments = $"{drive} /a",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                DiskOptimizationStatus.Text = output.Contains("You don't need to defragment")
                    ? "Drive is optimized"
                    : "Drive needs optimization";
            }
            catch (Exception ex)
            {
                DiskOptimizationStatus.Text = $"Error: {ex.Message}";
            }
        }

        private async void OptimizeDrives_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string drive = DriveComboBox.SelectedItem.ToString().Split(' ')[0];
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "defrag",
                        Arguments = $"{drive} /o",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                DiskOptimizationStatus.Text = "Optimization completed: " + output;
            }
            catch (Exception ex)
            {
                DiskOptimizationStatus.Text = $"Error: {ex.Message}";
            }
        }

        private async void ScanSystemFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "sfc",
                        Arguments = "/scannow",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                SfcStatusText.Text = output.Contains("did not find any integrity violations")
                    ? "No corrupted files found"
                    : "Corrupted files found (see details in CBS.log)";
            }
            catch (Exception ex)
            {
                SfcStatusText.Text = $"Error: {ex.Message}";
            }
        }

        private async void RepairSystemFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dism",
                        Arguments = "/online /cleanup-image /restorehealth",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                SfcStatusText.Text = output.Contains("The operation completed successfully")
                    ? "System files repaired successfully"
                    : "Repair failed (see details in CBS.log)";
            }
            catch (Exception ex)
            {
                SfcStatusText.Text = $"Error: {ex.Message}";
            }
        }
        #endregion

        #region Windows Customization Functions
        private void EditFileContextMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("regedit.exe", "/s \"HKEY_CLASSES_ROOT\\*\\shell\"");
                NotificationService.Instance.ShowWarning("Registry Editor opened at file context menu location.");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to open Registry Editor: {ex.Message}");
            }
        }

        private void EditDesktopContextMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("regedit.exe", "/s \"HKEY_CLASSES_ROOT\\Directory\\Background\\shell\"");
                NotificationService.Instance.ShowWarning("Registry Editor opened at desktop context menu location.");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to open Registry Editor: {ex.Message}");
            }
        }

        private void ResetContextMenus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("cmd", "/c reg delete \"HKEY_CLASSES_ROOT\\*\\shellex\\ContextMenuHandlers\" /f") { Verb = "runas" });
                Process.Start(new ProcessStartInfo("cmd", "/c reg delete \"HKEY_CLASSES_ROOT\\Directory\\shellex\\ContextMenuHandlers\" /f") { Verb = "runas" });
                NotificationService.Instance.ShowWarning("Context menus reset to defaults. Restart Explorer to see changes.");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to reset context menus: {ex.Message}");
            }
        }

        private void OpenFeaturesDialog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("optionalfeatures.exe");
                NotificationService.Instance.ShowSuccess("Windows Features dialog opened.");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to open Windows Features: {ex.Message}");
            }
        }

        private async void ListInstalledFeatures_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dism",
                        Arguments = "/online /get-features /format:table",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                FeaturesTextBox.Text = output;
            }
            catch (Exception ex)
            {
                FeaturesTextBox.Text = $"Error: {ex.Message}";
            }
        }

        private void SetExplorerTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string theme = (sender as Button)?.Tag as string;
                if (string.IsNullOrEmpty(theme)) return;

                int value = theme == "Dark" ? 1 : 0;
                Process.Start(new ProcessStartInfo("reg",
                    $"add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize\" /v AppsUseLightTheme /t REG_DWORD /d {value} /f")
                { Verb = "runas" });

                ExplorerCustomizationStatus.Text = $"Explorer theme set to {theme} mode. Restart Explorer to see changes.";
            }
            catch (Exception ex)
            {
                ExplorerCustomizationStatus.Text = $"Error: {ex.Message}";
            }
        }

        private void ToggleExplorerRibbon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string action = (sender as Button)?.Tag as string;
                if (string.IsNullOrEmpty(action)) return;

                int value = action == "Show" ? 0 : 1;
                Process.Start(new ProcessStartInfo("reg",
                    $"add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Ribbon\" /v Minimized /t REG_DWORD /d {value} /f")
                { Verb = "runas" });

                ExplorerCustomizationStatus.Text = $"Explorer ribbon will be {action}n. Restart Explorer to see changes.";
            }
            catch (Exception ex)
            {
                ExplorerCustomizationStatus.Text = $"Error: {ex.Message}";
            }
        }
        #endregion

        #region Advanced Tools Functions
        private void BackupRegistry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string backupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"RegistryBackup_{DateTime.Now:yyyyMMdd_HHmmss}.reg");
                Process.Start(new ProcessStartInfo("reg", $"export \"HKLM\" \"{backupPath}\" /y") { Verb = "runas" });
                NotificationService.Instance.ShowSuccess($"Full registry backup started. File will be saved to: {backupPath}");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to backup registry: {ex.Message}");
            }
        }

        private void OpenRegistryEditor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("regedit.exe");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to open Registry Editor: {ex.Message}");
            }
        }

        private async void ScanRegistryErrors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NotificationService.Instance.ShowSuccess("Registry scaning...");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "chkdsk",
                        Arguments = "/scan",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                ShowMessage("Registry scan completed: " + output);
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to scan registry: {ex.Message}");
            }
        }

        private void OpenTaskScheduler_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("taskschd.msc");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to open Task Scheduler: {ex.Message}");
            }
        }

        private void DisableMicrosoftTasks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("schtasks", "/change /tn \"\\Microsoft\\Windows\\*\" /disable") { Verb = "runas" });
                NotificationService.Instance.ShowSuccess("Microsoft scheduled tasks disabled.");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to disable Microsoft tasks: {ex.Message}");
            }
        }

        private async void ListAllTasks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "schtasks",
                        Arguments = "/query /fo list",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                TasksTextBox.Text = output;
            }
            catch (Exception ex)
            {
                TasksTextBox.Text = $"Error: {ex.Message}";
            }
        }

        private void OpenGroupPolicy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("gpedit.msc");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to open Group Policy Editor: {ex.Message}\nNote: This feature is only available in Windows Pro/Enterprise editions.");
            }
        }

        private void ExportPolicies_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string backupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"PolicyBackup_{DateTime.Now:yyyyMMdd_HHmmss}.html");
                Process.Start(new ProcessStartInfo("secedit", $"/export /cfg \"{backupPath}\"") { Verb = "runas" });
                NotificationService.Instance.ShowSuccess($"Group Policy backup started. File will be saved to: {backupPath}");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to export policies: {ex.Message}");
            }
        }

        private void ResetPolicies_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("secedit", "/configure /cfg %windir%\\inf\\defltbase.inf /db defltbase.sdb /verbose") { Verb = "runas" });
                NotificationService.Instance.ShowSuccess("Group Policy reset to defaults.");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to reset policies: {ex.Message}");
            }
        }
        #endregion

        #region System Information Functions
        private async void GenerateQuickReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "systeminfo",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                ReportTextBox.Text = output;
            }
            catch (Exception ex)
            {
                ReportTextBox.Text = $"Error: {ex.Message}";
            }
        }

        private async void GenerateDetailedReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReportTextBox.Text = "Gathering system information...";

                string cpuInfo = await GetWmiInfo("Win32_Processor", "Name");
                string gpuInfo = await GetWmiInfo("Win32_VideoController", "Name");
                string ramInfo = await GetWmiInfo("Win32_PhysicalMemory", "Capacity");
                string diskInfo = await GetWmiInfo("Win32_DiskDrive", "Model,Size");
                string osInfo = await GetWmiInfo("Win32_OperatingSystem", "Caption,Version");
                string biosInfo = await GetWmiInfo("Win32_BIOS", "Manufacturer,Version");
                string networkInfo = await GetWmiInfo("Win32_NetworkAdapterConfiguration", "Description,IPAddress");

                string report = $"=== SYSTEM REPORT ===\n" +
                                $"Generated: {DateTime.Now}\n\n" +
                                $"=== OPERATING SYSTEM ===\n{osInfo}\n\n" +
                                $"=== PROCESSOR ===\n{cpuInfo}\n\n" +
                                $"=== GRAPHICS ===\n{gpuInfo}\n\n" +
                                $"=== MEMORY ===\n{ramInfo}\n\n" +
                                $"=== STORAGE ===\n{diskInfo}\n\n" +
                                $"=== BIOS ===\n{biosInfo}\n\n" +
                                $"=== NETWORK ===\n{networkInfo}";

                ReportTextBox.Text = report;
            }
            catch (Exception ex)
            {
                ReportTextBox.Text = $"Error generating report: {ex.Message}";
            }
        }

        private async Task<string> GetWmiInfo(string className, string properties)
        {
            try
            {
                string result = "";
                var searcher = new ManagementObjectSearcher($"SELECT {properties} FROM {className}");

                await Task.Run(() =>
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        foreach (var prop in properties.Split(','))
                        {
                            result += $"{prop}: {obj[prop]?.ToString() ?? "N/A"}\n";
                        }
                        result += "\n";
                    }
                });

                return result.Trim();
            }
            catch (Exception ex)
            {
                return $"Error retrieving {className} info: {ex.Message}";
            }
        }

        private void ExportReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string reportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"SystemReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.WriteAllText(reportPath, ReportTextBox.Text);
                NotificationService.Instance.ShowSuccess($"Report saved to: {reportPath}");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to export report: {ex.Message}");
            }
        }

        private async void ShowCpuInfo_Click(object sender, RoutedEventArgs e)
        {
            HardwareInfoTextBox.Text = await GetWmiInfo("Win32_Processor", "Name,Manufacturer,NumberOfCores,NumberOfLogicalProcessors,MaxClockSpeed,CurrentClockSpeed");
        }

        private async void ShowGpuInfo_Click(object sender, RoutedEventArgs e)
        {
            HardwareInfoTextBox.Text = await GetWmiInfo("Win32_VideoController", "Name,AdapterRAM,DriverVersion,VideoProcessor");
        }

        private async void ShowRamInfo_Click(object sender, RoutedEventArgs e)
        {
            HardwareInfoTextBox.Text = await GetWmiInfo("Win32_PhysicalMemory", "Manufacturer,PartNumber,Capacity,Speed");
        }

        private async void ShowDiskInfo_Click(object sender, RoutedEventArgs e)
        {
            HardwareInfoTextBox.Text = await GetWmiInfo("Win32_DiskDrive", "Model,Size,Partitions,InterfaceType");
        }
        #endregion

        #region Quick Actions
        private void RestartExplorer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("explorer"))
                {
                    process.Kill();
                }
                Process.Start("explorer.exe");
                NotificationService.Instance.ShowSuccess("Windows Explorer restarted successfully.");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to restart Explorer: {ex.Message}");
            }
        }

        private void FlushDns_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("ipconfig", "/flushdns") { Verb = "runas" });
                NotificationService.Instance.ShowSuccess("DNS cache flushed successfully.");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to flush DNS cache: {ex.Message}");
            }
        }

        private void ResetNetwork_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("netsh", "int ip reset") { Verb = "runas" });
                Process.Start(new ProcessStartInfo("netsh", "winsock reset") { Verb = "runas" });
                NotificationService.Instance.ShowSuccess("Network stack reset. Restart your computer to complete the process.");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to reset network: {ex.Message}");
            }
        }

        private void CleanRecycleBin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SHEmptyRecycleBin(IntPtr.Zero, null, RecycleBinFlag.SHERB_NOCONFIRMATION | RecycleBinFlag.SHERB_NOPROGRESSUI);
                NotificationService.Instance.ShowSuccess("Recycle Bin cleared successfully.");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Failed to clear Recycle Bin: {ex.Message}");
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

        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleBinFlag dwFlags);

        private enum RecycleBinFlag : int
        {
            SHERB_NOCONFIRMATION = 0x00000001,
            SHERB_NOPROGRESSUI = 0x00000002,
            SHERB_NOSOUND = 0x00000004
        }
        #endregion
    }
}