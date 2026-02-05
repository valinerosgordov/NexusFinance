using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using NexusFinance.Models;
using NexusFinance.Services;

namespace NexusFinance.ViewModels;

/// <summary>
/// Transaction Input ViewModel - handles income/expense entry.
/// Implements Clean Architecture with dependency injection.
/// </summary>
public partial class TransactionInputViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private bool _isIncome = false;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private DateTime _transactionDate = DateTime.Now;

    [ObservableProperty]
    private string? _selectedProject;

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _projects = new();

    [ObservableProperty]
    private ObservableCollection<string> _categories = new();

    public TransactionInputViewModel() : this(ServiceContainer.Instance.DataService)
    {
    }

    public TransactionInputViewModel(IDataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        
        try
        {
            LoadData();
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "TransactionInputViewModel.Constructor");
        }
    }

    [RelayCommand]
    private void SetIncome()
    {
        IsIncome = true;
        LoadCategories();
    }

    [RelayCommand]
    private void SetExpense()
    {
        IsIncome = false;
        LoadCategories();
    }

    [RelayCommand]
    private void Cancel()
    {
        ResetForm();
        SelectedProject = Projects.FirstOrDefault();
    }

    [RelayCommand]
    private void AddTransaction()
    {
        try
        {
            // Validation with clear error messages
            if (string.IsNullOrWhiteSpace(Description))
            {
                MessageBox.Show(
                    "Description is required!", 
                    Constants.ErrorMessages.ValidationError,
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                return;
            }

            if (Amount <= 0)
            {
                MessageBox.Show(
                    "Amount must be greater than zero!", 
                    Constants.ErrorMessages.ValidationError,
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedCategory))
            {
                MessageBox.Show(
                    "Category is required!", 
                    Constants.ErrorMessages.ValidationError,
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                return;
            }

            var transaction = new Transaction
            {
                Description = Description.Trim(),
                Amount = Amount,
                Date = TransactionDate,
                Category = SelectedCategory,
                Project = SelectedProject ?? Constants.ProjectNames.Personal,
                IsIncome = IsIncome
            };

            _dataService.AddTransaction(transaction);

            var transactionType = IsIncome ? Constants.TransactionTypes.Income : Constants.TransactionTypes.Expense;
            MessageBox.Show(
                $"✅ Transaction Saved!\n\n" +
                $"Type: {transactionType}\n" +
                $"Description: {Description}\n" +
                $"Amount: ₽{Amount:N0}\n" +
                $"Date: {TransactionDate.ToString(Constants.DateFormats.ShortDate)}\n" +
                $"Project: {transaction.Project}\n" +
                $"Category: {SelectedCategory}",
                Constants.ErrorMessages.Success,
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // Reset form
            ResetForm();
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "TransactionInputViewModel.AddTransaction");
            MessageBox.Show(
                $"Failed to save transaction: {ex.Message}",
                Constants.ErrorMessages.ValidationError,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ResetForm()
    {
        Description = string.Empty;
        Amount = 0;
        TransactionDate = DateTime.Now;
        Notes = string.Empty;
        LoadCategories();
    }

    private void LoadData()
    {
        var projectsList = _dataService.GetProjects();
        Projects = new ObservableCollection<string>(projectsList.Select(p => p.Name));
        
        if (Projects.Any())
        {
            SelectedProject = Projects.First();
        }

        LoadCategories();
    }

    private void LoadCategories()
    {
        var categoriesList = _dataService.GetCategories();
        var filtered = categoriesList.Where(c => 
            (IsIncome && c.Type == "Income") || (!IsIncome && c.Type == "Expense")
        );
        
        Categories = new ObservableCollection<string>(filtered.Select(c => c.Name));
        
        if (Categories.Any())
        {
            SelectedCategory = Categories.First();
        }
    }
}
