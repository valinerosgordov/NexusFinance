using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;

namespace NexusFinance.Services;

/// <summary>
/// AI-powered financial analysis service using Google Gemini via Microsoft Semantic Kernel.
/// </summary>
public class GeminiAnalysisService : IAiService
{
    private readonly SecureStorageService _secureStorage;
    private Kernel? _kernel;
    private bool _isInitialized;

    private const string SystemPrompt = """
        You are the Neural CFO of NexusFinance - a ruthless, highly analytical financial advisor.
        
        YOUR MISSION:
        - Maximize the user's Net Worth
        - Minimize unnecessary Burn Rate
        - Identify financial risks and inefficiencies
        
        ANALYSIS RULES:
        1. Be concise and direct (max 3-4 sentences per point)
        2. Highlight critical risks (e.g., runway < 3 months, excessive spending)
        3. Provide specific, actionable optimizations with numbers
        4. Use bullet points for clarity
        5. If spending is wasteful, call it out directly
        6. Focus on ROI - is money being invested or wasted?
        
        TONE: Professional but ruthless. No pleasantries. Be effective, not polite.
        
        OUTPUT FORMAT:
        - 🚨 CRITICAL ISSUES (if any)
        - 📊 ANALYSIS
        - ⚡ RECOMMENDATIONS
        - 💎 OPTIMIZATION OPPORTUNITIES
        
        The user will provide financial data in JSON format. Analyze it and answer their question.
        """;

    public GeminiAnalysisService(SecureStorageService secureStorage)
    {
        _secureStorage = secureStorage;
    }

    public bool IsConfigured => _secureStorage.HasApiKey();

    /// <summary>
    /// Analyzes financial context using Google Gemini.
    /// </summary>
    public async Task<string> AnalyzeFinancialContextAsync(string jsonContext, string userQuestion)
    {
        EnsureInitialized();

        try
        {
            var prompt = $"""
                FINANCIAL DATA:
                {jsonContext}
                
                USER QUESTION:
                {userQuestion}
                
                Analyze the data above and answer the user's question. Follow the system rules.
                """;

            var result = await _kernel!.InvokePromptAsync(prompt);
            return result.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "AI analysis failed. Check your API key and internet connection.",
                ex
            );
        }
    }

    private void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        var apiKey = _secureStorage.LoadApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "⚠️ Gemini API Key not configured.\n\n" +
                "Please go to Settings and enter your Google AI API Key.\n" +
                "Get your free key at: https://ai.google.dev/"
            );
        }

        try
        {
            // Initialize Semantic Kernel with Google Gemini
            var builder = Kernel.CreateBuilder();

#pragma warning disable SKEXP0070 // Google AI connector is experimental
            builder.AddGoogleAIGeminiChatCompletion(
                modelId: "gemini-1.5-flash", // Fast and cost-effective
                apiKey: apiKey
            );
#pragma warning restore SKEXP0070

            _kernel = builder.Build();

            // Inject system prompt (using Semantic Kernel's prompt execution settings)
            _kernel.Data["SystemPrompt"] = SystemPrompt;

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to initialize AI service. Check your API key.",
                ex
            );
        }
    }

    /// <summary>
    /// Resets the service (useful after API key change).
    /// </summary>
    public void Reset()
    {
        _isInitialized = false;
        _kernel = null;
    }
}
