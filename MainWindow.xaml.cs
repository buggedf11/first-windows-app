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

                // Update Memory Usage
                float memUsage = _memoryCounter != null ? _memoryCounter.NextValue() : 0;
                MemUsage.Text = $"{memUsage:F1}%";
                MemProgressBar.Value = Math.Min(100, memUsage);

                // Update Disk Usage
                var drives = System.IO.DriveInfo.GetDrives();
                if (drives.Length > 0)
                {
                    var drive = drives[0];
                    double usedPercentage = ((double)(drive.TotalSize - drive.AvailableFreeSpace) / drive.TotalSize) * 100;
                    DiskUsage.Text = $"{usedPercentage:F1}%";
                    DiskProgressBar.Value = Math.Min(100, usedPercentage);
                }

                // Update Process Count
                int processCount = Process.GetProcesses().Length;
                ProcessCount.Text = processCount.ToString();

                // Update Uptime
                var uptime = DateTime.Now - _startTime;
                Uptime.Text = $"{uptime.Days} days";

                // Update Time
                TimeDisplay.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Update Alerts
                string alerts = "• All systems operational";
                if (cpuUsage > 80) alerts += "\n⚠ High CPU usage detected";
                if (memUsage > 80) alerts += "\n⚠ High memory usage detected";
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
