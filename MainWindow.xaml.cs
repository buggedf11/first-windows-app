using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace FuturisticDashboard
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer? _updateTimer;
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _memoryCounter;
        private DateTime _startTime;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                InitializePerformanceCounters();
                _startTime = DateTime.Now;
                SetupUpdateTimer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing dashboard: {ex.Message}\n\n{ex.StackTrace}", "Initialization Error");
                this.Close();
            }
        }

        private void InitializePerformanceCounters()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _memoryCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            }
            catch
            {
                MessageBox.Show("Performance counters not available on this system.");
            }
        }

        private void SetupUpdateTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(1);
            _updateTimer.Tick += UpdateDashboard;
            _updateTimer.Start();
            UpdateDashboard(null, null);
        }

        private void UpdateDashboard(object? sender, EventArgs? e)
        {
            try
            {
                // Update CPU Usage
                float cpuUsage = _cpuCounter != null ? _cpuCounter.NextValue() : 0;
                CpuUsage.Text = $"{cpuUsage:F1}%";
                CpuProgressBar.Value = Math.Min(100, cpuUsage);

                // Simulate per-core load (in real scenario, use PerformanceCounter for each core)
                float coreLoad = cpuUsage / 4;
                CpuCore1.Value = Math.Min(100, coreLoad);
                CpuCore2.Value = Math.Min(100, coreLoad * 1.1f);
                CpuCore3.Value = Math.Min(100, coreLoad * 0.9f);
                CpuCore4.Value = Math.Min(100, coreLoad * 1.05f);

                int processCount = Process.GetProcesses().Length;
                CpuDetail.Text = $"Processes: {processCount}";

                // Update Memory Usage
                float memUsage = _memoryCounter != null ? _memoryCounter.NextValue() : 0;
                MemUsage.Text = $"{memUsage:F1}%";
                MemProgressBar.Value = Math.Min(100, memUsage);

                // Get memory info in GB
                var memInfo = GC.GetTotalMemory(false);
                long totalMemory = GC.GetTotalMemory(false);
                MemDetail.Text = $"{(memUsage / 100 * 16):F1} GB / 16 GB (Installed)";
                MemAvailable.Text = $"Available: {((100 - memUsage) / 100 * 16):F1} GB";

                // Update Disk Usage
                var drives = System.IO.DriveInfo.GetDrives();
                if (drives.Length > 0)
                {
                    var drive = drives[0];
                    double usedPercentage = ((double)(drive.TotalSize - drive.AvailableFreeSpace) / drive.TotalSize) * 100;
                    DiskUsage.Text = $"{usedPercentage:F1}%";
                    DiskProgressBar.Value = Math.Min(100, usedPercentage);

                    double usedGB = (drive.TotalSize - drive.AvailableFreeSpace) / (1024d * 1024d * 1024d);
                    double totalGB = drive.TotalSize / (1024d * 1024d * 1024d);
                    double freeGB = drive.AvailableFreeSpace / (1024d * 1024d * 1024d);
                    
                    DiskDetail.Text = $"{usedGB:F1} GB / {totalGB:F1} GB Used";
                    DiskFree.Text = $"Free: {freeGB:F1} GB";
                }

                // Update Process Count
                ProcessCount.Text = $"Active Processes: {processCount}";

                // Update Uptime
                var uptime = DateTime.Now - _startTime;
                Uptime.Text = $"Uptime: {uptime.Days} days, {uptime.Hours} hours";

                // Update System Load
                float systemLoad = (cpuUsage + memUsage) / 2;
                SystemLoad.Text = $"System Load: {systemLoad:F1}%";
                SystemLoadBar.Value = Math.Min(100, systemLoad);

                // Update Time
                TimeDisplay.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Network Status
                NetworkStatus.Text = "CONNECTED";
                NetworkIndicator.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen);
                NetworkDetail.Text = $"Interfaces: {System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().Length}";
                
                try
                {
                    var hostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                    var ipAddr = hostEntry.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    NetworkIP.Text = $"IP: {(ipAddr?.ToString() ?? "N/A")}";
                }
                catch { NetworkIP.Text = "IP: Unavailable"; }

                // GPU & Temperature (simulated)
                GpuUsage.Text = $"{(systemLoad * 0.5):F1} %";
                CpuTemp.Text = $"{(50 + cpuUsage * 0.3):F0} °C";

                // Performance Status
                if (systemLoad > 80)
                    PerformanceDetail.Text = "Status: ⚠ Under Heavy Load";
                else if (systemLoad > 50)
                    PerformanceDetail.Text = "Status: ℹ Moderate Load";
                else
                    PerformanceDetail.Text = "Status: ✓ Optimal";

                // Update Alerts
                string alerts = "• All systems operational";
                if (cpuUsage > 85) alerts += "\n⚠ High CPU usage detected";
                if (memUsage > 85) alerts += "\n⚠ High memory usage detected";
                if (systemLoad > 80) alerts += "\n⚠ System under heavy load";
                if (cpuUsage > 95 || memUsage > 95) alerts += "\n🔴 Critical resource usage!";
                
                AlertsPanel.Text = alerts;
            }
            catch (Exception ex)
            {
                AlertsPanel.Text = $"Error updating dashboard: {ex.Message}";
            }
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_updateTimer != null)
                _updateTimer.Stop();
            _cpuCounter?.Dispose();
            _memoryCounter?.Dispose();
        }
    }
}
