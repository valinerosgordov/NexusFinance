using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using NexusFinance.Services;

namespace NexusFinance.ViewModels;

/// <summary>
/// Wallet ViewModel - manages accounts and investments.
/// Implements Clean Architecture with dependency injection.
/// </summary>
public partial class WalletViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private ObservableCollection<WalletAccount> _accounts = new();

    [ObservableProperty]
    private decimal _totalBalance;

    [ObservableProperty]
    private ObservableCollection<InvestmentItem> _investments = new();

    [ObservableProperty]
    private decimal _totalInvested;

    [ObservableProperty]
    private decimal _totalReturn;

    public WalletViewModel() : this(ServiceContainer.Instance.DataService)
    {
    }

    public WalletViewModel(IDataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        
        try
        {
            LoadWalletData();
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "WalletViewModel.Constructor");
        }
    }

    [RelayCommand]
    public void Refresh()
    {
        try
        {
            LoadWalletData();
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "WalletViewModel.Refresh");
            MessageBox.Show(
                $"Failed to refresh wallet data: {ex.Message}",
                Constants.ErrorMessages.ValidationError,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void AddAccount()
    {
        var dialog = new Views.AccountEditorDialog
        {
            Owner = Application.Current.MainWindow
        };
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            _dataService.AddAccount(dialog.Result);
            LoadWalletData();
            MessageBox.Show($"Account '{dialog.Result.Name}' added successfully!", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void EditAccount(string? accountName)
    {
        if (string.IsNullOrWhiteSpace(accountName))
            return;

        var accounts = _dataService.GetAccounts();
        var account = accounts.FirstOrDefault(a => a.Name == accountName);
        if (account == null)
            return;

        var dialog = new Views.AccountEditorDialog(account)
        {
            Owner = Application.Current.MainWindow
        };
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            _dataService.UpdateAccount(accountName, dialog.Result);
            LoadWalletData();
            MessageBox.Show($"Account '{dialog.Result.Name}' updated successfully!", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void DeleteAccount(string? accountName)
    {
        if (string.IsNullOrWhiteSpace(accountName))
            return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete account '{accountName}'?\nThis action cannot be undone.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _dataService.DeleteAccount(accountName);
            LoadWalletData();
            MessageBox.Show($"Account '{accountName}' deleted successfully!", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void AddInvestment()
    {
        var dialog = new Views.InvestmentEditorDialog
        {
            Owner = Application.Current.MainWindow
        };
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            _dataService.AddInvestment(dialog.Result);
            LoadWalletData();
            MessageBox.Show($"Investment '{dialog.Result.Name}' added successfully!", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void EditInvestment(string? investmentName)
    {
        if (string.IsNullOrWhiteSpace(investmentName))
            return;

        var investments = _dataService.GetInvestments();
        var investment = investments.FirstOrDefault(i => i.Name == investmentName);
        if (investment == null)
            return;

        var dialog = new Views.InvestmentEditorDialog(investment)
        {
            Owner = Application.Current.MainWindow
        };
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            _dataService.UpdateInvestment(investmentName, dialog.Result);
            LoadWalletData();
            MessageBox.Show($"Investment '{dialog.Result.Name}' updated successfully!", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void DeleteInvestment(string? investmentName)
    {
        if (string.IsNullOrWhiteSpace(investmentName))
            return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete investment '{investmentName}'?\nThis action cannot be undone.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _dataService.DeleteInvestment(investmentName);
            LoadWalletData();
            MessageBox.Show($"Investment '{investmentName}' deleted successfully!", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void LoadWalletData()
    {
        var accountsList = _dataService.GetAccounts();
        var investmentsList = _dataService.GetInvestments();

        // Load accounts
        Accounts = new ObservableCollection<WalletAccount>(
            accountsList.Select(a => new WalletAccount(
                a.Name,
                a.Balance,
                a.Institution,
                GetAccountColor(a.Type)
            ))
        );

        TotalBalance = accountsList.Sum(a => a.Balance);

        // Load investments
        Investments = new ObservableCollection<InvestmentItem>(
            investmentsList.Select(i => new InvestmentItem(
                i.Name,
                i.Amount,
                i.CurrentValue,
                i.ReturnPercent,
                i.ReturnPercent >= 0 ? "#00C853" : "#D50000"
            ))
        );

        TotalInvested = investmentsList.Sum(i => i.Amount);
        TotalReturn = investmentsList.Sum(i => i.CurrentValue) - TotalInvested;
    }

    private static string GetAccountColor(string type)
    {
        return type switch
        {
            "Checking" => "#C0C0C0",
            "Savings" => "#909090",
            "Cash" => "#A0A0A0",
            _ => "#808080"
        };
    }
}

public record WalletAccount(
    string Name,
    decimal Balance,
    string Institution,
    string Color
);

public record InvestmentItem(
    string Name,
    decimal Invested,
    decimal CurrentValue,
    decimal ReturnPercent,
    string Color
);
