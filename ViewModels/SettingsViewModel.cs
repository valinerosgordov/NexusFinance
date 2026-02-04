using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusFinance.Services;
using System.Windows;

namespace NexusFinance.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SecureStorageService _secureStorage;

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    private bool _hasApiKey;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _statusColor = "#B0B0B0";

    public SettingsViewModel()
    {
        _secureStorage = new SecureStorageService();
        CheckApiKeyStatus();
    }

    [RelayCommand]
    private void SaveApiKey()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                StatusMessage = "⚠️ Please enter an API key";
                StatusColor = "#FF1744";
                return;
            }

            // Basic validation (Gemini keys start with "AIza")
            if (!ApiKey.StartsWith("AIza"))
            {
                StatusMessage = "⚠️ Invalid Gemini API key format. Keys should start with 'AIza'";
                StatusColor = "#FF1744";
                return;
            }

            _secureStorage.SaveApiKey(ApiKey);
            HasApiKey = true;
            StatusMessage = "✅ API Key saved securely!";
            StatusColor = "#00E676";

            // Clear the input for security
            ApiKey = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
            StatusColor = "#FF1744";
        }
    }

    [RelayCommand]
    private void DeleteApiKey()
    {
        var result = MessageBox.Show(
            "Are you sure you want to delete the stored API key?",
            "Confirm Deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _secureStorage.DeleteApiKey();
                HasApiKey = false;
                ApiKey = string.Empty;
                StatusMessage = "🗑️ API Key deleted";
                StatusColor = "#B0B0B0";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Error: {ex.Message}";
                StatusColor = "#FF1744";
            }
        }
    }

    [RelayCommand]
    private void TestConnection()
    {
        if (!HasApiKey)
        {
            StatusMessage = "⚠️ No API key configured";
            StatusColor = "#FF1744";
            return;
        }

        try
        {
            var aiService = new GeminiAnalysisService(_secureStorage);
            
            if (aiService.IsConfigured)
            {
                StatusMessage = "✅ Configuration valid. AI service ready!";
                StatusColor = "#00E676";
            }
            else
            {
                StatusMessage = "⚠️ API key not found or invalid";
                StatusColor = "#FF1744";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Test failed: {ex.Message}";
            StatusColor = "#FF1744";
        }
    }

    [RelayCommand]
    private void OpenApiKeyLink()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://ai.google.dev/",
                UseShellExecute = true
            });
        }
        catch
        {
            StatusMessage = "⚠️ Could not open browser";
            StatusColor = "#FF1744";
        }
    }

    private void CheckApiKeyStatus()
    {
        HasApiKey = _secureStorage.HasApiKey();
        
        if (HasApiKey)
        {
            StatusMessage = "✅ API Key is configured";
            StatusColor = "#00E676";
        }
        else
        {
            StatusMessage = "⚠️ No API key configured. Get one at ai.google.dev";
            StatusColor = "#FFA500";
        }
    }
}
