using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace FuturisticDashboard
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer? _updateTimer;
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _memoryCounter;
        private PerformanceCounter? _networkUpCounter;
        private PerformanceCounter? _networkDownCounter;
        private DateTime _startTime;
        private Queue<double> _cpuHistory = new Queue<double>();
        private Queue<double> _memHistory = new Queue<double>();
        private Queue<double> _netUpHistory = new Queue<double>();
        private Queue<double> _netDownHistory = new Queue<double>();
        private const int MaxDataPoints = 60;

        public class ProcessInfo
        {
            public string Name { get; set; } = "";
            public int Id { get; set; }
            public double CpuUsage { get; set; }
            public double MemoryMB { get; set; }
        }

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                InitializeColorScheme();
                InitializePerformanceCounters();
                InitializeCharts();
                _startTime = DateTime.Now;
                SetupUpdateTimer();
                PopulateColorSchemes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing: {ex.Message}", "Initialization Error");
                this.Close();
            }
        }

        private void InitializeColorScheme()
        {
            // Dark mode by default
            Resources["PrimaryColor"] = Color.FromRgb(13, 17, 23);
            Resources["SecondaryColor"] = Color.FromRgb(22, 27, 34);
        }

        private void PopulateColorSchemes()
        {
            ColorSchemeSelector.Items.Add("Cyan/Magenta");
            ColorSchemeSelector.Items.Add("Green/Red");
            ColorSchemeSelector.Items.Add("Blue/Orange");
            ColorSchemeSelector.SelectedIndex = 0;
        }

        private void InitializePerformanceCounters()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _memoryCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
                _networkUpCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", GetNetworkInterface());
                _networkDownCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", GetNetworkInterface());
            }
            catch
            {
                MessageBox.Show("Performance counters not available on this system.");
            }
        }

        private string GetNetworkInterface()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            return interfaces.FirstOrDefault(i => i.NetworkInterfaceType == NetworkInterfaceType.Ethernet)?.Name 
                ?? interfaces.FirstOrDefault(i => i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)?.Name 
                ?? "_Total";
        }

        private void InitializeCharts()
        {
            SetupLinePlot(CpuChart, "CPU Usage (%)");
            SetupLinePlot(MemoryChart, "Memory Usage (%)");
            SetupLinePlot(NetworkChart, "Network Speed (Mbps)");
            SetupPiePlot(DiskChart);
        }

        private void SetupLinePlot(OxyPlot.Wpf.PlotView plot, string title)
        {
            var model = new PlotModel { Title = title, Background = OxyColor.FromRgb(16, 27, 34) };
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, TextColor = OxyColor.FromRgb(232, 240, 255) });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, TextColor = OxyColor.FromRgb(232, 240, 255) });
            var series = new LineSeries { Title = title, Color = OxyColor.FromRgb(0, 217, 255) };
            model.Series.Add(series);
            plot.Model = model;
        }

        private void SetupPiePlot(OxyPlot.Wpf.PlotView plot)
        {
            var model = new PlotModel { Title = "Disk Usage", Background = OxyColor.FromRgb(22, 27, 34) };
            var series = new PieSeries();
            model.Series.Add(series);
            plot.Model = model;
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
                float cpuUsage = _cpuCounter != null ? _cpuCounter.NextValue() : 0;
                float memUsage = _memoryCounter != null ? _memoryCounter.NextValue() : 0;

                QuickCpu.Text = $"{cpuUsage:F1}%";
                QuickMem.Text = $"{memUsage:F1}%";

                _cpuHistory.Enqueue(cpuUsage);
                _memHistory.Enqueue(memUsage);

                if (_cpuHistory.Count > MaxDataPoints) _cpuHistory.Dequeue();
                if (_memHistory.Count > MaxDataPoints) _memHistory.Dequeue();

                UpdateChart(CpuChart, _cpuHistory, cpuUsage);
                UpdateChart(MemoryChart, _memHistory, memUsage);

                // Disk
                var drives = System.IO.DriveInfo.GetDrives();
                if (drives.Length > 0)
                {
                    var drive = drives[0];
                    double diskUsage = ((double)(drive.TotalSize - drive.AvailableFreeSpace) / drive.TotalSize) * 100;
                    QuickDisk.Text = $"{diskUsage:F1}%";
                    UpdateDiskChart(drive);
                }

                // System Load
                float systemLoad = (cpuUsage + memUsage) / 2;
                QuickLoad.Text = $"{systemLoad:F1}%";

                TimeDisplay.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update error: {ex.Message}");
            }
        }

        private void UpdateChart(OxyPlot.Wpf.PlotView plot, Queue<double> data, double currentValue)
        {
            if (plot.Model is PlotModel model && model.Series.FirstOrDefault() is LineSeries series)
            {
                series.Points.Clear();
                int index = 0;
                foreach (var value in data)
                {
                    series.Points.Add(new DataPoint(index++, value));
                }
                plot.InvalidatePlot();
            }
        }

        private void UpdateDiskChart(System.IO.DriveInfo drive)
        {
            if (DiskChart.Model is PlotModel model && model.Series.FirstOrDefault() is PieSeries series)
            {
                series.Slices.Clear();
                double used = (drive.TotalSize - drive.AvailableFreeSpace) / (1024d * 1024d * 1024d);
                double free = drive.AvailableFreeSpace / (1024d * 1024d * 1024d);

                series.Slices.Add(new PieSlice($"Used ({used:F0}GB)", used) { Fill = OxyColor.FromRgb(0, 217, 255) });
                series.Slices.Add(new PieSlice($"Free ({free:F0}GB)", free) { Fill = OxyColor.FromRgb(48, 54, 61) });
                DiskChart.InvalidatePlot();
            }
        }

        private void RefreshProcesses_Click(object sender, RoutedEventArgs e)
        {
            var processes = Process.GetProcesses()
                .OrderByDescending(p => p.WorkingSet64)
                .Take(15)
                .Select(p => new ProcessInfo
                {
                    Name = p.ProcessName,
                    Id = p.Id,
                    MemoryMB = p.WorkingSet64 / (1024d * 1024d),
                    CpuUsage = 0 // Note: Getting real CPU per-process requires more complex counters
                })
                .ToList();

            ProcessesGrid.ItemsSource = processes;
        }

        private void KillProcess_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessesGrid.SelectedItem is ProcessInfo proc)
            {
                try
                {
                    Process.GetProcessById(proc.Id).Kill();
                    MessageBox.Show($"Process {proc.Name} terminated.", "Success");
                    RefreshProcesses_Click(sender, e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            var isDark = (Color)Resources["PrimaryColor"] == Color.FromRgb(13, 17, 23);
            if (isDark)
            {
                Resources["PrimaryColor"] = Color.FromRgb(240, 240, 240);
                Resources["SecondaryColor"] = Color.FromRgb(220, 220, 220);
                Resources["TextPrimary"] = Color.FromRgb(30, 30, 30);
            }
            else
            {
                Resources["PrimaryColor"] = Color.FromRgb(13, 17, 23);
                Resources["SecondaryColor"] = Color.FromRgb(22, 27, 34);
                Resources["TextPrimary"] = Color.FromRgb(232, 240, 255);
            }
        }

        private void ToggleFullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
            }
            else
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
            }
        }

        private void ColorScheme_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Change accent colors based on selection
            string scheme = ColorSchemeSelector.SelectedItem?.ToString() ?? "";
            switch (scheme)
            {
                case "Green/Red":
                    Resources["AccentColor"] = Color.FromRgb(0, 255, 0);
                    Resources["AccentAlt"] = Color.FromRgb(255, 0, 0);
                    break;
                case "Blue/Orange":
                    Resources["AccentColor"] = Color.FromRgb(0, 122, 255);
                    Resources["AccentAlt"] = Color.FromRgb(255, 165, 0);
                    break;
                default:
                    Resources["AccentColor"] = Color.FromRgb(0, 217, 255);
                    Resources["AccentAlt"] = Color.FromRgb(255, 0, 110);
                    break;
            }
        }

        private void AlwaysOnTop_Checked(object sender, RoutedEventArgs e) => Topmost = true;
        private void AlwaysOnTop_Unchecked(object sender, RoutedEventArgs e) => Topmost = false;

        private void RefreshInterval_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updateTimer != null)
            {
                _updateTimer.Interval = TimeSpan.FromSeconds(RefreshIntervalSlider.Value);
                RefreshIntervalDisplay.Text = $"{RefreshIntervalSlider.Value:F1}s";
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11) ToggleFullscreen_Click(sender, e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _updateTimer?.Stop();
            _cpuCounter?.Dispose();
            _memoryCounter?.Dispose();
            base.OnClosed(e);
        }
    }
}
