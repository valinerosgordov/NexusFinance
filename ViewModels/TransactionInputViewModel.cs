using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace NexusFinance.ViewModels;

public partial class TransactionInputViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isIncome = true;

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
    private ObservableCollection<string> _projects = new() { "Personal", "NexusAI", "FinSync" };

    [ObservableProperty]
    private ObservableCollection<string> _categories = new() 
    { 
        "Salary", "Freelance", "Investment Income",
        "Food", "Transport", "Rent", "Infrastructure", "Software"
    };

    [RelayCommand]
    private void AddTransaction()
    {
        // TODO: Save to database
        var type = IsIncome ? "Income" : "Expense";
        System.Windows.MessageBox.Show(
            $"Transaction Added!\n\n" +
            $"Type: {type}\n" +
            $"Amount: ₽{Amount:N2}\n" +
            $"Description: {Description}\n" +
            $"Project: {SelectedProject ?? "None"}\n" +
            $"Category: {SelectedCategory ?? "None"}",
            "Success",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information
        );
        
        // Reset form
        Description = string.Empty;
        Amount = 0;
        Notes = string.Empty;
    }

    [RelayCommand]
    private void SetIncome()
    {
        IsIncome = true;
    }

    [RelayCommand]
    private void SetExpense()
    {
        IsIncome = false;
    }
}
