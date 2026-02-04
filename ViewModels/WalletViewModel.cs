using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace NexusFinance.ViewModels;

public partial class WalletViewModel : ObservableObject
{
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

    public WalletViewModel()
    {
        LoadWalletData();
    }

    private void LoadWalletData()
    {
        // Load accounts
        Accounts = new ObservableCollection<WalletAccount>
        {
            new("💳 Checking Account", 450000, "Sberbank", "#00E676"),
            new("💰 Savings", 1200000, "Tinkoff", "#8A2BE2"),
            new("🏦 Business Account", 650000, "Alfa Bank", "#00BCD4"),
            new("💵 Cash", 150000, "Physical", "#FFA500"),
        };

        TotalBalance = Accounts.Sum(a => a.Balance);

        // Load investments
        Investments = new ObservableCollection<InvestmentItem>
        {
            new("₿ Bitcoin", 850000, 920000, 8.2m, "#F7931A"),
            new("📈 Stocks Portfolio", 650000, 720000, 10.8m, "#00E676"),
            new("🏠 Real Estate Fund", 1200000, 1450000, 20.8m, "#8A2BE2"),
            new("💎 Gold", 450000, 480000, 6.7m, "#FFD700"),
        };

        TotalInvested = Investments.Sum(i => i.Invested);
        TotalReturn = Investments.Sum(i => i.CurrentValue) - TotalInvested;
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
