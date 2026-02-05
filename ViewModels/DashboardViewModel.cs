using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using NexusFinance.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace NexusFinance.ViewModels;

/// <summary>
/// Dashboard ViewModel - displays financial overview and recent activity.
/// Follows MVVM pattern with proper dependency injection.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private decimal _netWorth;

    [ObservableProperty]
    private decimal _monthlyIncome;

    [ObservableProperty]
    private decimal _monthlyExpense;

    [ObservableProperty]
    private decimal _savingsRate;

    [ObservableProperty]
    private string _lastUpdated = DateTime.Now.ToString(Constants.DateFormats.DateTime);

    [ObservableProperty]
    private ObservableCollection<TransactionItem> _recentTransactions = new();

    [ObservableProperty]
    private ObservableCollection<CategoryExpense> _topExpenses = new();

    [ObservableProperty]
    private ISeries[] _series = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _xAxes = 
    [
        new Axis
        {
            Name = "Month",
            TextSize = 12,
            LabelsPaint = new SolidColorPaint(SKColors.Gray)
        }
    ];

    [ObservableProperty]
    private Axis[] _yAxes = 
    [
        new Axis
        {
            Name = "Amount (₽)",
            TextSize = 12,
            LabelsPaint = new SolidColorPaint(SKColors.Gray)
        }
    ];

    public DashboardViewModel() : this(ServiceContainer.Instance.DataService)
    {
    }

    /// <summary>
    /// Constructor with dependency injection for testability.
    /// WHY: Allows mocking IDataService in unit tests.
    /// </summary>
    public DashboardViewModel(IDataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        
        try
        {
            LoadDashboardData();
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "DashboardViewModel.Constructor");
            // UI will show empty state, which is acceptable
        }
    }

    [RelayCommand]
    public void Refresh()
    {
        try
        {
            LoadDashboardData();
            LastUpdated = DateTime.Now.ToString(Constants.DateFormats.DateTime);
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "DashboardViewModel.Refresh");
            System.Windows.MessageBox.Show(
                $"Failed to refresh dashboard: {ex.Message}",
                Constants.ErrorMessages.ValidationError,
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning
            );
        }
    }

    private void LoadDashboardData()
    {
        var transactions = _dataService.GetTransactions();
        var accounts = _dataService.GetAccounts();
        var investments = _dataService.GetInvestments();

        // If no data exists, show empty dashboard (no mock data after ClearAllData)
        if (!transactions.Any() && !accounts.Any() && !investments.Any())
        {
            NetWorth = 0;
            MonthlyIncome = 0;
            MonthlyExpense = 0;
            SavingsRate = 0;
            RecentTransactions = new ObservableCollection<TransactionItem>();
            TopExpenses = new ObservableCollection<CategoryExpense>();
            Series = Array.Empty<ISeries>();
            return;
        }

        // Calculate Net Worth (defensive: ensure no negative values)
        var totalAccounts = accounts.Sum(a => Math.Max(0, a.Balance));
        var totalInvestments = investments.Sum(i => Math.Max(0, i.CurrentValue));
        NetWorth = totalAccounts + totalInvestments;

        // Calculate monthly income and expenses (last 30 days)
        var thirtyDaysAgo = DateTime.Now.AddDays(-Constants.Defaults.DaysForMonthlyCalculation);
        var recentTransactions = transactions.Where(t => t.Date >= thirtyDaysAgo).ToList();
        
        MonthlyIncome = recentTransactions.Where(t => t.IsIncome).Sum(t => t.Amount);
        MonthlyExpense = recentTransactions.Where(t => !t.IsIncome).Sum(t => t.Amount);

        // Calculate savings rate (defensive: prevent division by zero)
        SavingsRate = MonthlyIncome > 0 
            ? Math.Round((MonthlyIncome - MonthlyExpense) / MonthlyIncome * 100, 1) 
            : 0;

        // Load recent transactions (last 10)
        RecentTransactions = new ObservableCollection<TransactionItem>(
            transactions
                .OrderByDescending(t => t.Date)
                .Take(Constants.Defaults.RecentTransactionsCount)
                .Select(t => new TransactionItem(
                    t.Date.ToString(Constants.DateFormats.ShortDate),
                    t.Description ?? string.Empty,
                    t.Category ?? string.Empty,
                    t.Project ?? string.Empty,
                    t.IsIncome ? t.Amount : -t.Amount,
                    t.IsIncome ? Constants.Colors.IncomeGreen : Constants.Colors.ExpenseRed
                ))
        );

        // Calculate top expenses by category
        var expensesByCategory = recentTransactions
            .Where(t => !t.IsIncome)
            .GroupBy(t => t.Category)
            .Select(g => new CategoryExpense(
                g.Key ?? "Unknown",
                g.Sum(t => t.Amount),
                GetCategoryColor(g.Key ?? "Unknown")
            ))
            .OrderByDescending(c => c.Amount)
            .Take(Constants.Defaults.TopExpensesCount);

        TopExpenses = new ObservableCollection<CategoryExpense>(expensesByCategory);

        // Load chart data
        LoadChartData(transactions.ToList());
    }

    private void LoadMockData()
    {
        // WHY: Show meaningful UI instead of empty charts on first run
        NetWorth = 500000m;
        MonthlyIncome = 150000m;
        MonthlyExpense = 85000m;
        SavingsRate = 43.3m;

        // Mock recent transactions
        RecentTransactions = new ObservableCollection<TransactionItem>
        {
            new("Jan 28", "Consulting Revenue", "Income", "Project Alpha", 50000m, Constants.Colors.IncomeGreen),
            new("Jan 25", "Server Infrastructure", "IT", "Project Alpha", -12500m, Constants.Colors.ExpenseRed),
            new("Jan 20", "Marketing Campaign", "Marketing", "Project Beta", -8000m, Constants.Colors.ExpenseRed),
            new("Jan 15", "Client Payment", "Income", "Project Beta", 35000m, Constants.Colors.IncomeGreen),
            new("Jan 10", "Office Supplies", "Operations", "General", -2500m, Constants.Colors.ExpenseRed)
        };

        // Mock top expenses
        TopExpenses = new ObservableCollection<CategoryExpense>
        {
            new("IT Infrastructure", 12500m, "#C0C0C0"),
            new("Marketing", 8000m, "#909090"),
            new("Operations", 2500m, "#808080"),
            new("Travel", 5000m, "#A0A0A0"),
            new("Software", 3000m, "#B0B0B0")
        };

        // Mock chart data - smooth growth curve
        var mockValues = new double[] { 100000, 150000, 200000, 280000, 380000, 500000 };
        Series = 
        [
            new LineSeries<double>
            {
                Name = "Net Worth",
                Values = mockValues,
                Stroke = new SolidColorPaint(SKColors.Silver) { StrokeThickness = 3 },
                Fill = new SolidColorPaint(SKColors.Silver.WithAlpha(40)),
                GeometrySize = 8,
                GeometryStroke = new SolidColorPaint(SKColors.Silver) { StrokeThickness = 2 },
                GeometryFill = new SolidColorPaint(SKColors.Black)
            }
        ];
    }

    private void LoadChartData(List<Models.Transaction> transactions)
    {
        // WHY: Visualize net worth trend over last 6 months
        if (!transactions.Any())
        {
            return;
        }

        var sixMonthsAgo = DateTime.Now.AddMonths(-6);
        var monthlyData = transactions
            .Where(t => t.Date >= sixMonthsAgo)
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new
            {
                Month = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM}",
                NetFlow = g.Sum(t => t.IsIncome ? t.Amount : -t.Amount)
            })
            .ToList();

        if (!monthlyData.Any())
        {
            return;
        }

        // Calculate cumulative net worth
        decimal cumulative = 0;
        var values = monthlyData.Select(m => 
        {
            cumulative += m.NetFlow;
            return (double)cumulative;
        }).ToArray();

        Series = 
        [
            new LineSeries<double>
            {
                Name = "Net Worth Trend",
                Values = values,
                Stroke = new SolidColorPaint(SKColors.Silver) { StrokeThickness = 3 },
                Fill = new SolidColorPaint(SKColors.Silver.WithAlpha(40)),
                GeometrySize = 8,
                GeometryStroke = new SolidColorPaint(SKColors.Silver) { StrokeThickness = 2 },
                GeometryFill = new SolidColorPaint(SKColors.Black)
            }
        ];
    }

    private static string GetCategoryColor(string category)
    {
        var colors = new[] { "#C0C0C0", "#909090", "#808080", "#A0A0A0", "#B0B0B0" };
        var hash = Math.Abs(category.GetHashCode());
        return colors[hash % colors.Length];
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
