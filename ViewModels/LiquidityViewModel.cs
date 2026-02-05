using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusFinance.Models;
using NexusFinance.Services;

namespace NexusFinance.ViewModels;

/// <summary>
/// Liquidity ViewModel - manages payables, receivables, and cash flow.
/// Implements Clean Architecture with dependency injection.
/// </summary>
public partial class LiquidityViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    
    [ObservableProperty]
    private ObservableCollection<PayableViewModel> _payables = new();
    
    [ObservableProperty]
    private ObservableCollection<ReceivableViewModel> _receivables = new();
    
    [ObservableProperty]
    private decimal _currentCash;
    
    [ObservableProperty]
    private decimal _totalPayables;
    
    [ObservableProperty]
    private decimal _totalReceivables;
    
    [ObservableProperty]
    private decimal _projectedBalance;
    
    [ObservableProperty]
    private string _projectedBalanceColor = Constants.Colors.NeutralGray;
    
    [ObservableProperty]
    private int _overduePayablesCount;
    
    [ObservableProperty]
    private int _criticalPayablesCount;
    
    public LiquidityViewModel() : this(ServiceContainer.Instance.DataService)
    {
    }

    public LiquidityViewModel(IDataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        
        try
        {
            LoadLiquidityData();
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "LiquidityViewModel.Constructor");
        }
    }
    
    private void LoadLiquidityData()
    {
        try
        {
            // Load current cash from accounts (defensive: ensure no negative balances)
            var accounts = _dataService.GetAccounts();
            CurrentCash = accounts.Sum(a => Math.Max(0, a.Balance));
            
            // Load payables
            var payables = _dataService.GetPayables();
            Payables = new ObservableCollection<PayableViewModel>(
                payables
                    .Where(p => !p.IsPaid)
                    .OrderBy(p => p.DueDate)
                    .Select(p => new PayableViewModel(p))
            );
            
            TotalPayables = Payables.Sum(p => Math.Max(0, p.Amount));
            OverduePayablesCount = payables.Count(p => !p.IsPaid && p.UrgencyLevel == "Overdue");
            CriticalPayablesCount = payables.Count(p => !p.IsPaid && 
                (p.UrgencyLevel == "Critical" || p.UrgencyLevel == "High"));
            
            // Load receivables
            var receivables = _dataService.GetReceivables();
            Receivables = new ObservableCollection<ReceivableViewModel>(
                receivables
                    .Where(r => !r.IsReceived)
                    .OrderBy(r => r.ExpectedDate)
                    .Select(r => new ReceivableViewModel(r))
            );
            
            TotalReceivables = Receivables.Sum(r => Math.Max(0, r.WeightedAmount));
            
            // Calculate projected balance
            ProjectedBalance = CurrentCash + TotalReceivables - TotalPayables;
            ProjectedBalanceColor = ProjectedBalance < 0 
                ? Constants.Colors.CriticalRed 
                : Constants.Colors.IncomeGreen;
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "LiquidityViewModel.LoadLiquidityData");
        }
    }
    
    [RelayCommand]
    private void AddPayable()
    {
        var dialog = new Views.PayableEditorDialog
        {
            Owner = Application.Current.MainWindow
        };
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            _dataService.AddPayable(dialog.Result);
            LoadLiquidityData();
            MessageBox.Show($"Payable '{dialog.Result.Title}' added successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    [RelayCommand]
    private void EditPayable(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        
        var payable = _dataService.GetPayables().FirstOrDefault(p => p.Id == id);
        if (payable == null) return;
        
        var dialog = new Views.PayableEditorDialog(payable)
        {
            Owner = Application.Current.MainWindow
        };
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            _dataService.UpdatePayable(id, dialog.Result);
            LoadLiquidityData();
            MessageBox.Show($"Payable '{dialog.Result.Title}' updated successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    [RelayCommand]
    private void DeletePayable(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        
        var payable = _dataService.GetPayables().FirstOrDefault(p => p.Id == id);
        if (payable == null) return;
        
        var result = MessageBox.Show(
            $"Are you sure you want to delete payable '{payable.Title}'?\nThis action cannot be undone.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        
        if (result == MessageBoxResult.Yes)
        {
            _dataService.DeletePayable(id);
            LoadLiquidityData();
            MessageBox.Show($"Payable '{payable.Title}' deleted successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    [RelayCommand]
    private void MarkPayableAsPaid(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        
        _dataService.MarkPayableAsPaid(id);
        LoadLiquidityData();
        MessageBox.Show("Payable marked as paid!", "Success",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    [RelayCommand]
    private void AddReceivable()
    {
        var projects = _dataService.GetProjects().ToList();
        var dialog = new Views.ReceivableEditorDialog(projects)
        {
            Owner = Application.Current.MainWindow
        };
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            _dataService.AddReceivable(dialog.Result);
            LoadLiquidityData();
            MessageBox.Show($"Receivable '{dialog.Result.Title}' added successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    [RelayCommand]
    private void EditReceivable(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        
        var receivable = _dataService.GetReceivables().FirstOrDefault(r => r.Id == id);
        if (receivable == null) return;
        
        var projects = _dataService.GetProjects().ToList();
        var dialog = new Views.ReceivableEditorDialog(projects, receivable)
        {
            Owner = Application.Current.MainWindow
        };
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            _dataService.UpdateReceivable(id, dialog.Result);
            LoadLiquidityData();
            MessageBox.Show($"Receivable '{dialog.Result.Title}' updated successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    [RelayCommand]
    private void DeleteReceivable(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        
        var receivable = _dataService.GetReceivables().FirstOrDefault(r => r.Id == id);
        if (receivable == null) return;
        
        var result = MessageBox.Show(
            $"Are you sure you want to delete receivable '{receivable.Title}'?\nThis action cannot be undone.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        
        if (result == MessageBoxResult.Yes)
        {
            _dataService.DeleteReceivable(id);
            LoadLiquidityData();
            MessageBox.Show($"Receivable '{receivable.Title}' deleted successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    [RelayCommand]
    private void MarkReceivableAsReceived(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        
        _dataService.MarkReceivableAsReceived(id);
        LoadLiquidityData();
        MessageBox.Show("Receivable marked as received!", "Success",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    [RelayCommand]
    private void Refresh()
    {
        try
        {
            LoadLiquidityData();
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Instance.LogError(ex, "LiquidityViewModel.Refresh");
            MessageBox.Show(
                $"Failed to refresh liquidity data: {ex.Message}",
                Constants.ErrorMessages.ValidationError,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}

/// <summary>
/// View-friendly wrapper for Payable
/// </summary>
public class PayableViewModel
{
    private readonly Payable _payable;
    
    public PayableViewModel(Payable payable)
    {
        _payable = payable;
    }
    
    public string Id => _payable.Id;
    public string Title => _payable.Title;
    public decimal Amount => _payable.Amount;
    public string CreditorName => _payable.CreditorName;
    public DateTime DueDate => _payable.DueDate;
    public string UrgencyLevel => _payable.UrgencyLevel;
    public string UrgencyColor => _payable.UrgencyColor;
    public string DueDateDisplay => _payable.DueDate.ToString("MMM dd, yyyy");
    public int DaysUntilDue => (_payable.DueDate - DateTime.Now).Days;
    public string DaysUntilDueText => DaysUntilDue < 0 ? $"{Math.Abs(DaysUntilDue)}d overdue" : $"{DaysUntilDue}d left";
}

/// <summary>
/// View-friendly wrapper for Receivable
/// </summary>
public class ReceivableViewModel
{
    private readonly Receivable _receivable;
    
    public ReceivableViewModel(Receivable receivable)
    {
        _receivable = receivable;
    }
    
    public string Id => _receivable.Id;
    public string Title => _receivable.Title;
    public decimal Amount => _receivable.Amount;
    public decimal WeightedAmount => _receivable.WeightedAmount;
    public string DebtorName => _receivable.DebtorName;
    public DateTime ExpectedDate => _receivable.ExpectedDate;
    public string ProbabilityText => _receivable.ProbabilityText;
    public string ProbabilityColor => _receivable.ProbabilityColor;
    public string ExpectedDateDisplay => _receivable.ExpectedDate.ToString("MMM dd, yyyy");
    public string ConfidenceDisplay => $"{GetConfidencePercent()}% confidence";
    
    private int GetConfidencePercent()
    {
        return _receivable.Probability switch
        {
            ProbabilityLevel.Confirmed => 100,
            ProbabilityLevel.Likely => 75,
            ProbabilityLevel.Uncertain => 40,
            _ => 50
        };
    }
}
