using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace NexusFinance.Services;

/// <summary>
/// Global exception handler that prevents application crashes by catching unhandled exceptions.
/// Implements "Fail-Safe" pattern from Clean Architecture.
/// WHY: Uncaught exceptions in async code or dispatcher threads can crash the entire app.
/// </summary>
public sealed class GlobalExceptionHandler
{
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NexusFinance",
        "Logs"
    );

    private static GlobalExceptionHandler? _instance;
    private static readonly object Lock = new();

    private GlobalExceptionHandler()
    {
        EnsureLogDirectoryExists();
    }

    public static GlobalExceptionHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    _instance ??= new GlobalExceptionHandler();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Registers global exception handlers for WPF Dispatcher and Task Scheduler.
    /// MUST be called in App.xaml.cs OnStartup.
    /// </summary>
    public void RegisterHandlers()
    {
        // WPF UI Thread exceptions
        Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;

        // Background Task exceptions (async/await)
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        // AppDomain exceptions (last resort)
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogException(e.Exception, "UI Thread");
        
        MessageBox.Show(
            $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe error has been logged.",
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );

        // Prevent crash
        e.Handled = true;
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogException(e.Exception, "Background Task");
        
        // Prevent crash
        e.SetObserved();
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            LogException(exception, "AppDomain");
        }
    }

    /// <summary>
    /// Logs exception to file with timestamp and stack trace.
    /// </summary>
    private void LogException(Exception exception, string source)
    {
        try
        {
            var logFile = Path.Combine(LogDirectory, $"errors_{DateTime.Now:yyyy-MM-dd}.log");
            var logEntry = $"""
                ================================================================================
                [{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {source} Exception
                --------------------------------------------------------------------------------
                Type: {exception.GetType().FullName}
                Message: {exception.Message}
                Stack Trace:
                {exception.StackTrace}
                
                Inner Exception: {exception.InnerException?.Message ?? "None"}
                ================================================================================

                """;

            File.AppendAllText(logFile, logEntry);
        }
        catch
        {
            // If logging fails, don't crash - fail silently
        }
    }

    private static void EnsureLogDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }
        catch
        {
            // Fail silently if directory creation fails
        }
    }

    /// <summary>
    /// Manual exception logging for try/catch blocks.
    /// </summary>
    public void LogError(Exception exception, string context)
    {
        LogException(exception, context);
    }
}
