using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace NexusFinance.ViewModels;

public partial class ProjectAnalyticsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string> _projects = new() { "NexusAI", "FinSync", "Personal" };

    [ObservableProperty]
    private string? _selectedProject = "NexusAI";

    [ObservableProperty]
    private ObservableCollection<ProjectSummary> _projectSummaries = new();

    [ObservableProperty]
    private decimal _totalRevenue;

    [ObservableProperty]
    private decimal _totalCost;

    [ObservableProperty]
    private decimal _netProfit;

    [ObservableProperty]
    private decimal _profitMargin;

    public ProjectAnalyticsViewModel()
    {
        LoadProjectData();
    }

    [RelayCommand]
    private void SelectProject(string? project)
    {
        SelectedProject = project;
        LoadProjectData();
    }

    private void LoadProjectData()
    {
        // Load project summaries
        ProjectSummaries = new ObservableCollection<ProjectSummary>
        {
            new("NexusAI", 250000, 135000, 115000, "#8A2BE2"),
            new("FinSync", 180000, 95000, 85000, "#00BCD4"),
            new("Personal", 0, 65000, -65000, "#FF1744"),
        };

        // Calculate totals for selected project
        var selected = ProjectSummaries.FirstOrDefault(p => p.Name == SelectedProject);
        if (selected != null)
        {
            TotalRevenue = selected.Revenue;
            TotalCost = selected.Cost;
            NetProfit = selected.Profit;
            ProfitMargin = TotalRevenue > 0 
                ? Math.Round(NetProfit / TotalRevenue * 100, 1) 
                : 0;
        }
    }
}

public record ProjectSummary(
    string Name,
    decimal Revenue,
    decimal Cost,
    decimal Profit,
    string Color
);
