using System.Text.Json;
using NexusFinance.ViewModels;

namespace NexusFinance.Services;

/// <summary>
/// Builds a JSON snapshot of the user's financial state for AI analysis.
/// </summary>
public class FinancialContextBuilder
{
    /// <summary>
    /// Creates a comprehensive financial snapshot from ViewModels.
    /// </summary>
    public static string BuildContext(
        DashboardViewModel dashboard,
        ProjectAnalyticsViewModel projects,
        WalletViewModel wallet)
    {
        var context = new
        {
            // High-level metrics
            Overview = new
            {
                NetWorth = dashboard.NetWorth,
                MonthlyIncome = dashboard.MonthlyIncome,
                MonthlyExpense = dashboard.MonthlyExpense,
                SavingsRate = dashboard.SavingsRate,
                LastUpdated = dashboard.LastUpdated
            },

            // Recent transactions
            RecentTransactions = dashboard.RecentTransactions.Select(t => new
            {
                t.Date,
                t.Description,
                t.Category,
                t.Project,
                t.Amount
            }).ToList(),

            // Expense breakdown
            TopExpenses = dashboard.TopExpenses.Select(e => new
            {
                e.Name,
                e.Amount
            }).ToList(),

            // Project analytics
            Projects = projects.ProjectSummaries.Select(p => new
            {
                p.Name,
                p.Revenue,
                p.Cost,
                p.Profit,
                ProfitMargin = p.Revenue > 0 ? Math.Round(p.Profit / p.Revenue * 100, 1) : 0
            }).ToList(),

            // Wallet & Investments
            Accounts = wallet.Accounts.Select(a => new
            {
                a.Name,
                a.Balance,
                a.Institution
            }).ToList(),

            Investments = wallet.Investments.Select(i => new
            {
                i.Name,
                i.Invested,
                i.CurrentValue,
                i.ReturnPercent
            }).ToList(),

            // Calculated metrics
            Analysis = new
            {
                TotalLiquidAssets = wallet.TotalBalance,
                TotalInvestments = wallet.TotalInvested,
                InvestmentReturn = wallet.TotalReturn,
                MonthlyBurnRate = dashboard.MonthlyExpense,
                RunwayMonths = dashboard.MonthlyExpense > 0 
                    ? Math.Round(wallet.TotalBalance / dashboard.MonthlyExpense, 1) 
                    : 0,
                IsProjectProfitable = projects.NetProfit > 0
            }
        };

        return JsonSerializer.Serialize(context, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Builds a simplified context (if full ViewModels not available).
    /// </summary>
    public static string BuildSimpleContext(
        decimal netWorth,
        decimal monthlyIncome,
        decimal monthlyExpense)
    {
        var context = new
        {
            NetWorth = netWorth,
            MonthlyIncome = monthlyIncome,
            MonthlyExpense = monthlyExpense,
            SavingsRate = monthlyIncome > 0 
                ? Math.Round((monthlyIncome - monthlyExpense) / monthlyIncome * 100, 1) 
                : 0,
            RunwayMonths = monthlyExpense > 0 
                ? Math.Round(netWorth / monthlyExpense, 1) 
                : 0
        };

        return JsonSerializer.Serialize(context, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
