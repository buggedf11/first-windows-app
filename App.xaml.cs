using System.Windows;

namespace FuturisticDashboard
{
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Unhandled Exception: {e.Exception.Message}\n\n{e.Exception.StackTrace}", "Error");
            e.Handled = true;
        }
    }
}
