using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using NexusFinance.Models;
using NexusFinance.Services;
using NexusFinance.Views;

namespace NexusFinance.ViewModels;

/// <summary>
/// Project Analytics ViewModel - manages project and team member data.
/// Implements Clean Architecture with proper dependency injection.
/// </summary>
public partial class ProjectAnalyticsViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private ObservableCollection<string> _projects = new();

    [ObservableProperty]
    private string? _selectedProject;

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
    
    [ObservableProperty]
    private ObservableCollection<TeamMemberViewModel> _teamMembers = new();
    
    [ObservableProperty]
    private decimal _totalMonthlyPayroll;

    public ProjectAnalyticsViewModel() : this(ServiceContainer.Instance.DataService)
    {
    }

    public ProjectAnalyticsViewModel(IDataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        
        try
        {
            LoadProjectData();
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "ProjectAnalyticsViewModel.Constructor");
        }
    }

    [RelayCommand]
    private void SelectProject(string? project)
    {
        SelectedProject = project;
        UpdateSelectedProjectStats();
        LoadTeamMembers();
    }

    [RelayCommand]
    private void AddProject()
    {
        try
        {
            var dialog = new ProjectEditorDialog
            {
                Owner = Application.Current.MainWindow
            };
            
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                _dataService.AddProject(dialog.Result);
                LoadProjectData();
                MessageBox.Show(
                    $"Project '{dialog.Result.Name}' added successfully!", 
                    Constants.ErrorMessages.Success,
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "ProjectAnalyticsViewModel.AddProject");
            MessageBox.Show(
                $"Failed to add project: {ex.Message}",
                Constants.ErrorMessages.ValidationError,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void EditProject(string? projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName))
            return;

        var projects = _dataService.GetProjects();
        var project = projects.FirstOrDefault(p => p.Name == projectName);
        if (project == null)
            return;

        var dialog = new ProjectEditorDialog(project)
        {
            Owner = Application.Current.MainWindow
        };
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            _dataService.UpdateProject(projectName, dialog.Result);
            LoadProjectData();
            MessageBox.Show($"Project '{dialog.Result.Name}' updated successfully!", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void DeleteProject(string? projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName))
            return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete project '{projectName}'?\nThis action cannot be undone.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _dataService.DeleteProject(projectName);
            LoadProjectData();
            MessageBox.Show($"Project '{projectName}' deleted successfully!", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void LoadProjectData()
    {
        try
        {
            var projectsList = _dataService.GetProjects();
            
            // Update projects list (defensive: filter out null/empty names)
            Projects = new ObservableCollection<string>(
                projectsList
                    .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                    .Select(p => p.Name)
            );
            
            // Update summaries
            ProjectSummaries = new ObservableCollection<ProjectSummary>(
                projectsList.Select(p => new ProjectSummary(
                    p.Name ?? "Unknown",
                    Math.Max(0, p.Revenue),
                    Math.Max(0, p.Cost),
                    p.Profit,
                    GetProjectColor(p.Name ?? "Unknown")
                ))
            );

            // Select first project if nothing selected or current selection is invalid
            if (SelectedProject == null || !Projects.Contains(SelectedProject))
            {
                SelectedProject = Projects.FirstOrDefault();
            }

            UpdateSelectedProjectStats();
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "ProjectAnalyticsViewModel.LoadProjectData");
            // UI will show empty state
        }
    }

    private void UpdateSelectedProjectStats()
    {
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
        else
        {
            TotalRevenue = 0;
            TotalCost = 0;
            NetProfit = 0;
            ProfitMargin = 0;
        }
    }

    private static string GetProjectColor(string projectName)
    {
        // Assign colors based on project name hash
        var colors = new[] { "#C0C0C0", "#909090", "#808080", "#A0A0A0", "#B0B0B0" };
        var hash = Math.Abs(projectName.GetHashCode());
        return colors[hash % colors.Length];
    }
    
    private void LoadTeamMembers()
    {
        if (string.IsNullOrWhiteSpace(SelectedProject))
        {
            TeamMembers.Clear();
            TotalMonthlyPayroll = 0;
            return;
        }
        
        var members = _dataService.GetTeamMembersByProject(SelectedProject);
        TeamMembers = new ObservableCollection<TeamMemberViewModel>(
            members.Select(m => new TeamMemberViewModel(m))
        );
        
        TotalMonthlyPayroll = _dataService.CalculateProjectPayroll(SelectedProject);
    }
    
    [RelayCommand]
    private void AddTeamMember()
    {
        if (string.IsNullOrWhiteSpace(SelectedProject))
        {
            MessageBox.Show("Please select a project first!", "No Project Selected",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        var dialog = new TeamMemberEditorDialog(SelectedProject)
        {
            Owner = Application.Current.MainWindow
        };
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            _dataService.AddTeamMember(dialog.Result);
            LoadTeamMembers();
            MessageBox.Show($"Team member '{dialog.Result.Name}' added successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    [RelayCommand]
    private void EditTeamMember(string? id)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(SelectedProject)) return;
        
        var member = _dataService.GetTeamMembers().FirstOrDefault(tm => tm.Id == id);
        if (member == null) return;
        
        var dialog = new TeamMemberEditorDialog(SelectedProject, member)
        {
            Owner = Application.Current.MainWindow
        };
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            _dataService.UpdateTeamMember(id, dialog.Result);
            LoadTeamMembers();
            MessageBox.Show($"Team member '{dialog.Result.Name}' updated successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    [RelayCommand]
    private void DeleteTeamMember(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        
        var member = _dataService.GetTeamMembers().FirstOrDefault(tm => tm.Id == id);
        if (member == null) return;
        
        var result = MessageBox.Show(
            $"Are you sure you want to delete team member '{member.Name}'?\nThis action cannot be undone.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        
        if (result == MessageBoxResult.Yes)
        {
            _dataService.DeleteTeamMember(id);
            LoadTeamMembers();
            MessageBox.Show($"Team member '{member.Name}' deleted successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    [RelayCommand]
    private void ToggleTeamMemberStatus(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        
        _dataService.ToggleTeamMemberStatus(id);
        LoadTeamMembers();
    }
}

public record ProjectSummary(
    string Name,
    decimal Revenue,
    decimal Cost,
    decimal Profit,
    string Color
);

/// <summary>
/// View-friendly wrapper for TeamMember
/// </summary>
public class TeamMemberViewModel
{
    private readonly TeamMember _member;
    
    public TeamMemberViewModel(TeamMember member)
    {
        _member = member;
    }
    
    public string Id => _member.Id;
    public string Name => _member.Name;
    public string Role => _member.Role;
    public decimal Salary => _member.Salary;
    public string PaymentFrequency => _member.PaymentFrequency.ToString();
    public decimal MonthlyCost => _member.MonthlyCost;
    public bool IsActive => _member.IsActive;
    public string StatusDisplay => _member.StatusDisplay;
    public string StatusColor => _member.StatusColor;
}
