using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusFinance.Services;
using System.Collections.ObjectModel;

namespace NexusFinance.ViewModels;

/// <summary>
/// Neural CFO ViewModel - AI-powered financial analysis using Google Gemini.
/// Implements Clean Architecture with dependency injection.
/// </summary>
public partial class NeuralCfoViewModel : ObservableObject
{
    private readonly IAiService _aiService;
    private readonly SecureStorageService _secureStorage;

    [ObservableProperty]
    private string _userQuestion = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ChatMessage> _chatHistory = new();

    [ObservableProperty]
    private bool _isAnalyzing;

    [ObservableProperty]
    private bool _isConfigured;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public NeuralCfoViewModel() 
        : this(ServiceContainer.Instance.AiService, ServiceContainer.Instance.SecureStorageService)
    {
    }

    public NeuralCfoViewModel(IAiService aiService, SecureStorageService secureStorage)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _secureStorage = secureStorage ?? throw new ArgumentNullException(nameof(secureStorage));
        
        try
        {
            CheckConfiguration();
            
            // Add welcome message
            if (IsConfigured)
            {
                ChatHistory.Add(new ChatMessage(
                    "🤖 Neural CFO",
                    "I'm your ruthless financial advisor. Ask me to analyze your finances, identify risks, or suggest optimizations. Be specific.",
                    false
                ));
            }
            else
            {
                ChatHistory.Add(new ChatMessage(
                    "⚠️ System",
                    "Neural CFO is not configured. Please go to Settings and add your Google Gemini API key.",
                    false
                ));
            }
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "NeuralCfoViewModel.Constructor");
        }
    }

    [RelayCommand]
    private async Task SendQuestion()
    {
        if (string.IsNullOrWhiteSpace(UserQuestion))
        {
            return;
        }

        if (!IsConfigured)
        {
            ChatHistory.Add(new ChatMessage(
                "⚠️ Error",
                "Please configure your API key in Settings first.",
                false
            ));
            return;
        }

        // Add user message
        var question = UserQuestion;
        ChatHistory.Add(new ChatMessage("You", question, true));
        UserQuestion = string.Empty;

        IsAnalyzing = true;
        StatusMessage = "🤔 Analyzing your finances...";

        try
        {
            // Build financial context (demo data)
            var context = FinancialContextBuilder.BuildSimpleContext(
                netWorth: 2_450_000,
                monthlyIncome: 125_000,
                monthlyExpense: 55_000
            );

            // Get AI analysis
            var response = await _aiService.AnalyzeFinancialContextAsync(context, question);

            // Add AI response
            ChatHistory.Add(new ChatMessage("🤖 Neural CFO", response, false));
            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            ChatHistory.Add(new ChatMessage(
                "❌ Error",
                $"Analysis failed: {ex.Message}",
                false
            ));
            StatusMessage = string.Empty;
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private void ClearHistory()
    {
        ChatHistory.Clear();
        ChatHistory.Add(new ChatMessage(
            "🤖 Neural CFO",
            "Chat cleared. What would you like to know?",
            false
        ));
    }

    [RelayCommand]
    private void AskSample(string sampleQuestion)
    {
        UserQuestion = sampleQuestion;
    }

    private void CheckConfiguration()
    {
        IsConfigured = _aiService.IsConfigured;
        
        if (!IsConfigured)
        {
            StatusMessage = "⚠️ Not configured - Go to Settings";
        }
    }
}

public record ChatMessage(string Sender, string Message, bool IsUser);
