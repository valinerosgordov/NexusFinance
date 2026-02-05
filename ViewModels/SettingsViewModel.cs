using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusFinance.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace NexusFinance.ViewModels;

/// <summary>
/// Settings ViewModel - manages application configuration.
/// Implements Clean Architecture with dependency injection.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly SecureStorageService _secureStorage;
    private readonly LocalizationService _localization;
    private readonly IDataService _dataService;
    private readonly GlobalExceptionHandler _exceptionHandler;

    [ObservableProperty]
    private string? _apiKey;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<LanguageOption> _availableLanguages = new();

    [ObservableProperty]
    private LanguageOption? _selectedLanguage;

    public SettingsViewModel() 
        : this(ServiceContainer.Instance.SecureStorageService, 
               LocalizationService.Instance, 
               ServiceContainer.Instance.DataService)
    {
    }

    public SettingsViewModel(SecureStorageService secureStorage, LocalizationService localization, IDataService dataService)
    {
        _secureStorage = secureStorage ?? throw new ArgumentNullException(nameof(secureStorage));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _exceptionHandler = GlobalExceptionHandler.Instance;

        try
        {
            // Load saved API key (if exists)
            _apiKey = _secureStorage.LoadApiKey();

            // Initialize language options
            _availableLanguages = new ObservableCollection<LanguageOption>
            {
                new LanguageOption { Code = "en-US", DisplayName = "🇺🇸 English" },
                new LanguageOption { Code = "ru-RU", DisplayName = "🇷🇺 Русский" }
            };

            // Set current language selection
            var currentCode = _localization.CurrentCulture.Name;
            _selectedLanguage = _availableLanguages.FirstOrDefault(l => l.Code == currentCode) 
                                ?? _availableLanguages[0];
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "SettingsViewModel.Constructor");
        }
    }

    [RelayCommand]
    private void SaveApiKey()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                StatusMessage = $"⚠️ {Constants.ErrorMessages.ApiKeyMissing}";
                return;
            }

            _secureStorage.SaveApiKey(ApiKey);
            StatusMessage = "✅ API Key saved securely";
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "SettingsViewModel.SaveApiKey");
            StatusMessage = $"❌ Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void TestConnection()
    {
        StatusMessage = "🔄 Testing connection... (Feature in development)";
    }
    
    [RelayCommand]
    private void DeleteApiKey()
    {
        try
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete your saved API key?",
                "Delete API Key",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No
            );
            
            if (result == MessageBoxResult.Yes)
            {
                _secureStorage.DeleteApiKey();
                ApiKey = string.Empty;
                StatusMessage = "✅ API Key deleted";
            }
        }
        catch (Exception ex)
        {
            _exceptionHandler.LogError(ex, "SettingsViewModel.DeleteApiKey");
            StatusMessage = $"❌ Error: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private void OpenApiKeyLink()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://ai.google.dev/",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _exceptionHandler.LogError(ex, "SettingsViewModel.OpenApiKeyLink");
            StatusMessage = $"❌ Error opening browser: {ex.Message}";
        }
    }

    partial void OnSelectedLanguageChanged(LanguageOption? value)
    {
        if (value == null || value.Code == _localization.CurrentCulture.Name)
            return;

        try
        {
            _localization.SetLanguage(value.Code);
            
            // Show confirmation message
            var message = value.Code == "ru-RU"
                ? "✅ Язык изменён на Русский. Интерфейс обновлён!"
                : "✅ Language changed to English. UI refreshed!";
            
            MessageBox.Show(message, "Language Changed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error changing language: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private void ClearAllData()
    {
        try
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear ALL data? This will delete:\n\n" +
                "• All transactions\n" +
                "• All projects\n" +
                "• All accounts\n" +
                "• All investments\n" +
                "• All payables and receivables\n" +
                "• All team members\n\n" +
                "This action cannot be undone!",
                "Clear All Data",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No
            );
            
            if (result == MessageBoxResult.Yes)
            {
                _dataService.ClearAllData();
                
                StatusMessage = "✅ All data cleared. Restart the app to see changes.";
                
                MessageBox.Show(
                    "All data has been cleared successfully.\n\nPlease restart the application to refresh all views.",
                    "Data Cleared",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
        catch (Exception ex)
        {
            _exceptionHandler.LogError(ex, "SettingsViewModel.ClearAllData");
            StatusMessage = $"❌ Error clearing data: {ex.Message}";
        }
    }
}

public class LanguageOption
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
