using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusFinance.Views;

namespace NexusFinance.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _activeView = "Dashboard";

    public MainViewModel()
    {
        // Start with Dashboard
        CurrentView = new DashboardView();
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentView = new DashboardView();
        ActiveView = "Dashboard";
    }

    [RelayCommand]
    private void NavigateToProjects()
    {
        CurrentView = new ProjectAnalyticsView();
        ActiveView = "Projects";
    }

    [RelayCommand]
    private void NavigateToWallet()
    {
        CurrentView = new WalletView();
        ActiveView = "Wallet";
    }

    [RelayCommand]
    private void ShowTransactionInput()
    {
        CurrentView = new TransactionInputView();
        ActiveView = "Transaction";
    }

    [RelayCommand]
    private void NavigateToAnalytics()
    {
        CurrentView = new AnalyticsView();
        ActiveView = "Analytics";
    }

    [RelayCommand]
    private void NavigateToLiquidity()
    {
        CurrentView = new LiquidityView();
        ActiveView = "Liquidity";
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentView = new SettingsView();
        ActiveView = "Settings";
    }

    [RelayCommand]
    private void NavigateToNeuralCfo()
    {
        CurrentView = new NeuralCfoView();
        ActiveView = "AI";
    }
}
