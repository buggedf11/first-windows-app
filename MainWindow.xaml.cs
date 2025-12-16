using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace ServerLauncher
{
    public partial class MainWindow : Window
    {
        private Process? _apiServerProcess;
        private Process? _webServerProcess;
        private Process? _databaseProcess;

        private string _apiPath = string.Empty;
        private string _webPath = string.Empty;
        private string _dbPath = string.Empty;
        private string _apiName = "API Server";
        private string _webName = "Web Server";
        private string _dbName = "Database";

        public MainWindow()
        {
            InitializeComponent();
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

        // API Server
        private void StartApiServer_Click(object sender, RoutedEventArgs e)
        {
            if (_apiServerProcess?.HasExited == false)
            {
                LogOutput("API Server is already running!");
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(_apiPath))
                {
                    LogOutput("API path is empty. Set it in Settings.");
                    return;
                }

                LogOutput($"Starting {_apiName}...");
                _apiServerProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"{_apiPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                _apiServerProcess.Start();
                LogOutput($"✓ {_apiName} started!");
                UpdateStatus(ApiServerStatus, ApiServerStatusText, true);
            }
            catch (Exception ex)
            {
                LogOutput($"✗ Failed to start {_apiName}: {ex.Message}");
                UpdateStatus(ApiServerStatus, ApiServerStatusText, false);
            }
        }

        private void StopApiServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_apiServerProcess != null && !_apiServerProcess.HasExited)
                {
                    LogOutput($"Stopping {_apiName}...");
                    _apiServerProcess.Kill();
                    _apiServerProcess.WaitForExit();
                    LogOutput($"✓ {_apiName} stopped!");
                }
                else
                {
                    LogOutput($"{_apiName} is not running");
                }
                UpdateStatus(ApiServerStatus, ApiServerStatusText, false);
            }
            catch (Exception ex)
            {
                LogOutput($"✗ Failed to stop {_apiName}: {ex.Message}");
            }
        }

        // Web Server
        private void StartWebServer_Click(object sender, RoutedEventArgs e)
        {
            if (_webServerProcess?.HasExited == false)
            {
                LogOutput("Web Server is already running!");
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(_webPath))
                {
                    LogOutput("Web path is empty. Set it in Settings.");
                    return;
                }

                LogOutput($"Starting {_webName}...");
                _webServerProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"{_webPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                _webServerProcess.Start();
                LogOutput($"✓ {_webName} started!");
                UpdateStatus(WebServerStatus, WebServerStatusText, true);
            }
            catch (Exception ex)
            {
                LogOutput($"✗ Failed to start {_webName}: {ex.Message}");
                UpdateStatus(WebServerStatus, WebServerStatusText, false);
            }
        }

        private void StopWebServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_webServerProcess != null && !_webServerProcess.HasExited)
                {
                    LogOutput($"Stopping {_webName}...");
                    _webServerProcess.Kill();
                    _webServerProcess.WaitForExit();
                    LogOutput($"✓ {_webName} stopped!");
                }
                else
                {
                    LogOutput($"{_webName} is not running");
                }
                UpdateStatus(WebServerStatus, WebServerStatusText, false);
            }
            catch (Exception ex)
            {
                LogOutput($"✗ Failed to stop {_webName}: {ex.Message}");
            }
        }

        // Database
        private void StartDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (_databaseProcess?.HasExited == false)
            {
                LogOutput("Database is already running!");
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(_dbPath))
                {
                    LogOutput("Database path is empty. Set it in Settings.");
                    return;
                }

                LogOutput($"Starting {_dbName}...");
                _databaseProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"{_dbPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                _databaseProcess.Start();
                LogOutput($"✓ {_dbName} started!");
                UpdateStatus(DatabaseStatus, DatabaseStatusText, true);
            }
            catch (Exception ex)
            {
                LogOutput($"✗ Failed to start {_dbName}: {ex.Message}");
                UpdateStatus(DatabaseStatus, DatabaseStatusText, false);
            }
        }

        private void StopDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_databaseProcess != null && !_databaseProcess.HasExited)
                {
                    LogOutput($"Stopping {_dbName}...");
                    _databaseProcess.Kill();
                    _databaseProcess.WaitForExit();
                    LogOutput($"✓ {_dbName} stopped!");
                }
                else
                {
                    LogOutput($"{_dbName} is not running");
                }
                UpdateStatus(DatabaseStatus, DatabaseStatusText, false);
            }
            catch (Exception ex)
            {
                LogOutput($"✗ Failed to stop {_dbName}: {ex.Message}");
            }
        }

        private void ClearOutput_Click(object sender, RoutedEventArgs e)
        {
            OutputText.Text = "";
            LogOutput("[Cleared]");
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            LogOutput("Settings opened. Update names and paths as needed.");
        }

        private void BrowseApi_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Title = "Select API executable" };
            if (dialog.ShowDialog() == true)
            {
                ApiPathInput.Text = dialog.FileName;
            }
        }

        private void BrowseWeb_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Title = "Select Web executable" };
            if (dialog.ShowDialog() == true)
            {
                WebPathInput.Text = dialog.FileName;
            }
        }

        private void BrowseDb_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Title = "Select Database executable" };
            if (dialog.ShowDialog() == true)
            {
                DbPathInput.Text = dialog.FileName;
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _apiName = string.IsNullOrWhiteSpace(ApiNameInput.Text) ? "API Server" : ApiNameInput.Text.Trim();
            _webName = string.IsNullOrWhiteSpace(WebNameInput.Text) ? "Web Server" : WebNameInput.Text.Trim();
            _dbName = string.IsNullOrWhiteSpace(DbNameInput.Text) ? "Database" : DbNameInput.Text.Trim();

            _apiPath = ApiPathInput.Text.Trim();
            _webPath = WebPathInput.Text.Trim();
            _dbPath = DbPathInput.Text.Trim();

            ApiNameText.Text = _apiName;
            WebNameText.Text = _webName;
            DbNameText.Text = _dbName;

            SettingsInfo.Text = "Saved.";
            LogOutput("Settings saved. Updated names and paths.");
        }

        protected override void OnClosed(EventArgs e)
        {
            _apiServerProcess?.Kill();
            _webServerProcess?.Kill();
            _databaseProcess?.Kill();
            base.OnClosed(e);
        }
    }
}
