using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace NexusFinance.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private decimal _netWorth = 2_450_000;

    [ObservableProperty]
    private decimal _monthlyIncome = 125_000;

    [ObservableProperty]
    private decimal _monthlyExpense = 55_000;

    [ObservableProperty]
    private decimal _savingsRate;

    [ObservableProperty]
    private string _lastUpdated = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

    [ObservableProperty]
    private ObservableCollection<TransactionItem> _recentTransactions = new();

    [ObservableProperty]
    private ObservableCollection<CategoryExpense> _topExpenses = new();

    public DashboardViewModel()
    {
        LoadDashboardData();
    }

    private void LoadDashboardData()
    {
        // Calculate savings rate
        SavingsRate = MonthlyIncome > 0 
            ? Math.Round((MonthlyIncome - MonthlyExpense) / MonthlyIncome * 100, 1) 
            : 0;

        // Load recent transactions
        RecentTransactions = new ObservableCollection<TransactionItem>
        {
            new("04.02.2026", "Salary - NexusAI", "Income", "NexusAI", 125000, "#00E676"),
            new("03.02.2026", "AWS Cloud Services", "Infrastructure", "NexusAI", -12500, "#FF1744"),
            new("02.02.2026", "Groceries", "Food", "Personal", -5500, "#FF1744"),
            new("01.02.2026", "Freelance Payment", "Income", "FinSync", 45000, "#00E676"),
            new("31.01.2026", "Office Rent", "Rent", "NexusAI", -25000, "#FF1744"),
        };

        // Load top expenses
        TopExpenses = new ObservableCollection<CategoryExpense>
        {
            new("Infrastructure", 28500, "#FF1493"),
            new("Payroll", 85000, "#FFA500"),
            new("Food", 12000, "#00BCD4"),
            new("Rent", 25000, "#8A2BE2"),
            new("Transport", 8500, "#FFEB3B"),
        };
    }
}

public record TransactionItem(
    string Date,
    string Description,
    string Category,
    string Project,
    decimal Amount,
    string AmountColor
);

public record CategoryExpense(
    string Name,
    decimal Amount,
    string Color
);
