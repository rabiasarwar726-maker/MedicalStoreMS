// Program.cs — Application Entry Point
using System;
using System.Windows.Forms;

namespace MedicalStoreMS
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show(
                    $"MESSAGE: {e.Exception.Message}\n\n" +
                    $"INNER: {e.Exception.InnerException?.Message}\n\n" +
                    $"STACK: {e.Exception.StackTrace}",
                    "Thread Error");
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = (Exception)e.ExceptionObject;
                MessageBox.Show($"CRASH:\n{ex.Message}\n\n{ex.StackTrace}", "Unhandled");
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            try
            {
                Application.Run(new UI.Forms.LoginForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"TYPE: {ex.GetType().FullName}\n\n" +
                    $"MESSAGE: {ex.Message}\n\n" +
                    $"INNER: {ex.InnerException?.Message}\n\n" +
                    $"STACK: {ex.StackTrace}",
                    "CRASH DETAILS");
            }
        }
    }
}