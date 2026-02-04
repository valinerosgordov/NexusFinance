namespace NexusFinance.Services;

/// <summary>
/// Defines the contract for AI-powered financial analysis services.
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Analyzes financial context using AI and answers user questions.
    /// </summary>
    /// <param name="jsonContext">Financial data in JSON format (Net Worth, transactions, projects, etc.).</param>
    /// <param name="userQuestion">The user's question or request for analysis.</param>
    /// <returns>AI-generated analysis and recommendations.</returns>
    Task<string> AnalyzeFinancialContextAsync(string jsonContext, string userQuestion);

    /// <summary>
    /// Checks if the AI service is properly configured (API key exists).
    /// </summary>
    bool IsConfigured { get; }
}
