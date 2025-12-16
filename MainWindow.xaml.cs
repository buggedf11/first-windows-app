using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ServerLauncher
{
    public partial class MainWindow : Window
    {
        private Process? _apiServerProcess;
        private Process? _webServerProcess;
        private Process? _databaseProcess;

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
                LogOutput("Starting API Server...");
                _apiServerProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c echo API Server started successfully",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                _apiServerProcess.Start();
                LogOutput("✓ API Server started!");
                UpdateStatus(ApiServerStatus, ApiServerStatusText, true);
            }
            catch (Exception ex)
            {
                LogOutput($"✗ Failed to start API Server: {ex.Message}");
                UpdateStatus(ApiServerStatus, ApiServerStatusText, false);
            }
        }

        private void StopApiServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_apiServerProcess != null && !_apiServerProcess.HasExited)
                {
                    LogOutput("Stopping API Server...");
                    _apiServerProcess.Kill();
                    _apiServerProcess.WaitForExit();
                    LogOutput("✓ API Server stopped!");
                }
                else
                {
                    LogOutput("API Server is not running");
                }
                UpdateStatus(ApiServerStatus, ApiServerStatusText, false);
            }
            catch (Exception ex)
            {
                LogOutput($"✗ Failed to stop API Server: {ex.Message}");
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
                LogOutput("Starting Web Server...");
                _webServerProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c echo Web Server started successfully",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                _webServerProcess.Start();
                LogOutput("✓ Web Server started!");
                UpdateStatus(WebServerStatus, WebServerStatusText, true);
            }
            catch (Exception ex)
            {
                LogOutput($"✗ Failed to start Web Server: {ex.Message}");
                UpdateStatus(WebServerStatus, WebServerStatusText, false);
            }
        }

        private void StopWebServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_webServerProcess != null && !_webServerProcess.HasExited)
                {
                    LogOutput("Stopping Web Server...");
                    _webServerProcess.Kill();
                    _webServerProcess.WaitForExit();
                    LogOutput("✓ Web Server stopped!");
                }
                else
                {
                    LogOutput("Web Server is not running");
                }
                UpdateStatus(WebServerStatus, WebServerStatusText, false);
            }
            catch (Exception ex)
            {
                LogOutput($"✗ Failed to stop Web Server: {ex.Message}");
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
                LogOutput("Starting Database...");
                _databaseProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c echo Database started successfully",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                _databaseProcess.Start();
                LogOutput("✓ Database started!");
                UpdateStatus(DatabaseStatus, DatabaseStatusText, true);
            }
            catch (Exception ex)
            {
                LogOutput($"✗ Failed to start Database: {ex.Message}");
                UpdateStatus(DatabaseStatus, DatabaseStatusText, false);
            }
        }

        private void StopDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_databaseProcess != null && !_databaseProcess.HasExited)
                {
                    LogOutput("Stopping Database...");
                    _databaseProcess.Kill();
                    _databaseProcess.WaitForExit();
                    LogOutput("✓ Database stopped!");
                }
                else
                {
                    LogOutput("Database is not running");
                }
                UpdateStatus(DatabaseStatus, DatabaseStatusText, false);
            }
            catch (Exception ex)
            {
                LogOutput($"✗ Failed to stop Database: {ex.Message}");
            }
        }

        private void ClearOutput_Click(object sender, RoutedEventArgs e)
        {
            OutputText.Text = "";
            LogOutput("[Cleared]");
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            LogOutput("Settings: Configure server paths in the code or use a config file");
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
