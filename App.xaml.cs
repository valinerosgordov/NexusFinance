using System.Windows;
using NexusFinance.Services;

namespace NexusFinance;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // CRITICAL: Register global exception handlers to prevent crashes
        // WHY: Unhandled exceptions in async code or UI thread will crash the app
        GlobalExceptionHandler.Instance.RegisterHandlers();
    }
}
