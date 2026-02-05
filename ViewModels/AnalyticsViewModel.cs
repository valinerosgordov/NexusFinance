using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusFinance.Models;
using NexusFinance.Services;

namespace NexusFinance.ViewModels;

/// <summary>
/// Analytics ViewModel - provides advanced financial visualizations and correlations.
/// Implements Clean Architecture with dependency injection.
/// </summary>
public partial class AnalyticsViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly SankeyService _sankeyService;
    private readonly CorrelationService _correlationService;
    
    [ObservableProperty]
    private SankeyData _sankeyData = new();
    
    [ObservableProperty]
    private ObservableCollection<CorrelationCell> _correlationMatrix = new();
    
    [ObservableProperty]
    private ObservableCollection<string> _assetNames = new();
    
    [ObservableProperty]
    private string _selectedPeriod = "Last 30 Days";
    
    public AnalyticsViewModel() : this(ServiceContainer.Instance.DataService)
    {
    }

    public AnalyticsViewModel(IDataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _sankeyService = new SankeyService();
        _correlationService = new CorrelationService();
        
        try
        {
            LoadAnalytics();
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "AnalyticsViewModel.Constructor");
        }
    }
    
    private void LoadAnalytics()
    {
        try
        {
            LoadSankeyData();
            LoadCorrelationMatrix();
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "AnalyticsViewModel.LoadAnalytics");
        }
    }
    
    private void LoadSankeyData()
    {
        try
        {
            var transactions = _dataService.GetTransactions().ToList();
            var accounts = _dataService.GetAccounts().ToList();
            
            // Filter by selected period
            var filteredTransactions = FilterByPeriod(transactions);
            
            SankeyData = _sankeyService.GenerateSankeyFromTransactions(
                filteredTransactions, 
                accounts);
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "AnalyticsViewModel.LoadSankeyData");
            SankeyData = new SankeyData(); // Empty state
        }
    }
    
    private void LoadCorrelationMatrix()
    {
        var investments = _dataService.GetInvestments();
        var projects = _dataService.GetProjects();
        
        // Create list of assets to analyze
        var assetList = new List<string>();
        
        // Add investments
        assetList.AddRange(investments.Select(i => i.Name).Take(5));
        
        // Add top projects
        assetList.AddRange(projects
            .OrderByDescending(p => p.Revenue)
            .Select(p => p.Name)
            .Take(3));
        
        // Add market indices (synthetic for demo)
        if (assetList.Count < 3)
        {
            assetList.AddRange(new[] { "BTC", "ETH", "S&P 500" });
        }
        
        AssetNames = new ObservableCollection<string>(assetList.Distinct());
        
        // Generate synthetic price data for correlation analysis
        var priceData = _correlationService.GenerateSyntheticPriceData(assetList);
        
        // Calculate correlation matrix
        var correlations = _correlationService.CalculateCorrelationMatrix(priceData);
        
        // Build matrix for UI
        var matrix = new ObservableCollection<CorrelationCell>();
        
        foreach (var asset1 in assetList)
        {
            foreach (var asset2 in assetList)
            {
                var correlation = correlations.GetValueOrDefault((asset1, asset2), 0);
                
                matrix.Add(new CorrelationCell
                {
                    Asset1 = asset1,
                    Asset2 = asset2,
                    Correlation = correlation,
                    DisplayValue = correlation.ToString("F2"),
                    BackgroundColor = GetCorrelationColor(correlation),
                    TooltipText = $"{asset1} vs {asset2}: {correlation:F3}"
                });
            }
        }
        
        CorrelationMatrix = matrix;
    }
    
    private List<Transaction> FilterByPeriod(List<Transaction> transactions)
    {
        var cutoffDate = SelectedPeriod switch
        {
            "Last 7 Days" => DateTime.Now.AddDays(-7),
            "Last 30 Days" => DateTime.Now.AddDays(-30),
            "Last 90 Days" => DateTime.Now.AddDays(-90),
            "Last Year" => DateTime.Now.AddYears(-1),
            _ => DateTime.Now.AddDays(-30)
        };
        
        return transactions.Where(t => t.Date >= cutoffDate).ToList();
    }
    
    private string GetCorrelationColor(double correlation)
    {
        var absCorr = Math.Abs(correlation);
        
        if (correlation > 0.7)
            return "#808080"; // Light grey for strong positive
        if (correlation > 0.4)
            return "#505050"; // Mid grey for moderate positive
        if (correlation < -0.7)
            return "#303030"; // Dark grey for strong negative (hedge)
        if (correlation < -0.4)
            return "#404040"; // Slightly lighter for moderate negative
        
        return "#1E1E1E"; // Near black for low correlation
    }
    
    [RelayCommand]
    private void Refresh()
    {
        LoadAnalytics();
    }
    
    [RelayCommand]
    private void ChangePeriod(string period)
    {
        SelectedPeriod = period;
        LoadSankeyData();
    }
}

/// <summary>
/// Represents a cell in the correlation matrix grid
/// </summary>
public class CorrelationCell
{
    public string Asset1 { get; set; } = string.Empty;
    public string Asset2 { get; set; } = string.Empty;
    public double Correlation { get; set; }
    public string DisplayValue { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = "#1E1E1E";
    public string TooltipText { get; set; } = string.Empty;
}
