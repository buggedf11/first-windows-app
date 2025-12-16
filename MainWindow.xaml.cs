using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Media.Effects;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ServerLauncher
{
    public partial class MainWindow : Window
    {
        public class ServerEntry : INotifyPropertyChanged
        {
            public string Name { get; set; } = "New Server";
            public string Path { get; set; } = string.Empty;
            public Process? Process { get; set; }
            private bool _running;
            public bool Running
            {
                get => _running;
                set { _running = value; OnPropertyChanged(nameof(Running)); OnPropertyChanged(nameof(StatusText)); OnPropertyChanged(nameof(StatusBrush)); }
            }

            public string StatusText => Running ? "Running ✓" : "Offline";
            public SolidColorBrush StatusBrush => Running ? (SolidColorBrush)Application.Current.Resources["SuccessBrush"] : (SolidColorBrush)Application.Current.Resources["ErrorBrush"];

            public event PropertyChangedEventHandler? PropertyChanged;
            private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<ServerEntry> Servers { get; } = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            // Seed with three servers
            Servers.Add(new ServerEntry { Name = "API Server" });
            Servers.Add(new ServerEntry { Name = "Web Server" });
            Servers.Add(new ServerEntry { Name = "Database" });
            ServersList.ItemsSource = Servers;
        }

        private void LogOutput(string message)
        {
            OutputText.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            // Auto-scroll to bottom
            if (OutputText.Parent is ScrollViewer sv)
            {
                sv.ScrollToEnd();
            }
        }

        private void UpdateStatus(Ellipse statusIndicator, TextBlock statusText, bool isRunning)
        {
            statusIndicator.Fill = isRunning ? 
                (SolidColorBrush)Resources["SuccessBrush"] : 
                (SolidColorBrush)Resources["ErrorBrush"];
            statusText.Text = isRunning ? "Running ✓" : "Offline";
            statusText.Foreground = isRunning ? 
                (SolidColorBrush)Resources["SuccessBrush"] : 
                (SolidColorBrush)Resources["ErrorBrush"];
        }

        // Dynamic server controls
        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ServerEntry entry)
            {
                if (entry.Process?.HasExited == false)
                {
                    LogOutput($"{entry.Name} is already running!");
                    return;
                }
                if (string.IsNullOrWhiteSpace(entry.Path))
                {
                    LogOutput($"Path for {entry.Name} is empty. Set it in Settings.");
                    return;
                }

                try
                {
                    LogOutput($"Starting {entry.Name}...");
                    entry.Process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c \"{entry.Path}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        },
                        EnableRaisingEvents = true
                    };
                    entry.Process.Exited += (_, __) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LogOutput($"{entry.Name} exited.");
                            entry.Running = false;
                        });
                    };

                    entry.Process.Start();
                    entry.Running = true;
                    LogOutput($"✓ {entry.Name} started!");
                }
                catch (Exception ex)
                {
                    entry.Running = false;
                    LogOutput($"✗ Failed to start {entry.Name}: {ex.Message}");
                }
            }
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ServerEntry entry)
            {
                try
                {
                    if (entry.Process != null && !entry.Process.HasExited)
                    {
                        LogOutput($"Stopping {entry.Name}...");
                        entry.Process.Kill();
                        entry.Process.WaitForExit();
                        LogOutput($"✓ {entry.Name} stopped!");
                    }
                    else
                    {
                        LogOutput($"{entry.Name} is not running");
                    }
                    entry.Running = false;
                }
                catch (Exception ex)
                {
                    LogOutput($"✗ Failed to stop {entry.Name}: {ex.Message}");
                }
            }
        }

        private void RemoveServer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ServerEntry entry)
            {
                if (entry.Process != null && !entry.Process.HasExited)
                {
                    try
                    {
                        entry.Process.Kill();
                        entry.Process.WaitForExit();
                    }
                    catch { }
                }
                Servers.Remove(entry);
                LogOutput($"Removed server: {entry.Name}");
            }
        }

        private void ClearOutput_Click(object sender, RoutedEventArgs e)
        {
            OutputText.Text = "";
            LogOutput("[Cleared]");
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsPanel.Visibility = Visibility.Visible;
            Backdrop.Visibility = Visibility.Visible;
            OutputPanel.Effect = new BlurEffect { Radius = 6 };
            LogOutput("Settings opened. Update names and paths as needed.");
        }

        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsPanel.Visibility = Visibility.Collapsed;
            Backdrop.Visibility = Visibility.Collapsed;
            OutputPanel.Effect = null;
            LogOutput("Settings closed.");
        }

        private void BrowseApi_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Title = "Select API executable" };
            if (dialog.ShowDialog() == true)
            {
                EnsureServerIndex(0);
                Servers[0].Path = dialog.FileName;
                ApiPathInput.Text = dialog.FileName;
            }
        }

        private void BrowseWeb_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Title = "Select Web executable" };
            if (dialog.ShowDialog() == true)
            {
                EnsureServerIndex(1);
                Servers[1].Path = dialog.FileName;
                WebPathInput.Text = dialog.FileName;
            }
        }

        private void BrowseDb_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Title = "Select Database executable" };
            if (dialog.ShowDialog() == true)
            {
                EnsureServerIndex(2);
                Servers[2].Path = dialog.FileName;
                DbPathInput.Text = dialog.FileName;
            }
        }

        private void EnsureServerIndex(int index)
        {
            while (Servers.Count <= index)
            {
                Servers.Add(new ServerEntry());
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            EnsureServerIndex(0);
            EnsureServerIndex(1);
            EnsureServerIndex(2);
            Servers[0].Name = string.IsNullOrWhiteSpace(ApiNameInput.Text) ? "API Server" : ApiNameInput.Text.Trim();
            Servers[1].Name = string.IsNullOrWhiteSpace(WebNameInput.Text) ? "Web Server" : WebNameInput.Text.Trim();
            Servers[2].Name = string.IsNullOrWhiteSpace(DbNameInput.Text) ? "Database" : DbNameInput.Text.Trim();

            Servers[0].Path = ApiPathInput.Text.Trim();
            Servers[1].Path = WebPathInput.Text.Trim();
            Servers[2].Path = DbPathInput.Text.Trim();

            SettingsInfo.Text = "Saved.";
            LogOutput("Settings saved. Updated names and paths.");
        }

        private void AddServer_Click(object sender, RoutedEventArgs e)
        {
            Servers.Add(new ServerEntry { Name = "New Server" });
            LogOutput("Added a new server entry.");
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (var s in Servers)
            {
                try
                {
                    s.Process?.Kill();
                }
                catch { }
            }
            base.OnClosed(e);
        }
    }
}
