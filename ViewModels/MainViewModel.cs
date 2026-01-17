using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialPlanner.Models;
using FinancialPlanner.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPlanner.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private readonly CurrencyService _currencyService;

        [ObservableProperty]
        private ObservableCollection<Transaction> transactions = new();

        [ObservableProperty]
        private ObservableCollection<DailyEntry> dailyEntries = new();

        [ObservableProperty]
        private ObservableCollection<string> availableHabits = new();

        [ObservableProperty]
        private ObservableCollection<Budget> budgets = new();

        [ObservableProperty]
        private ObservableCollection<string> categories = new();

        [ObservableProperty]
        private ObservableCollection<Currency> availableCurrencies = new();

        [ObservableProperty]
        private DailyEntry? selectedDailyEntry;

        [ObservableProperty]
        private Transaction? selectedTransaction;

        [ObservableProperty]
        private Budget? selectedBudget;

        [ObservableProperty]
        private decimal totalIncome;

        [ObservableProperty]
        private decimal totalExpenses;

        [ObservableProperty]
        private decimal balance;

        [ObservableProperty]
        private string newTransactionDescription = string.Empty;

        [ObservableProperty]
        private decimal newTransactionAmount;

        [ObservableProperty]
        private TransactionType newTransactionType = TransactionType.Expense;

        [ObservableProperty]
        private string newTransactionCategory = string.Empty;

        [ObservableProperty]
        private string newTransactionCurrency = "RUB";

        [ObservableProperty]
        private string newHabitName = string.Empty;

        [ObservableProperty]
        private string newTaskDescription = string.Empty;

        [ObservableProperty]
        private string dailyNotes = string.Empty;

        [ObservableProperty]
        private int dailyMood = 5;

        [ObservableProperty]
        private string newBudgetCategory = string.Empty;

        [ObservableProperty]
        private decimal newBudgetAmount;

        [ObservableProperty]
        private string newBudgetCurrency = "RUB";

        [ObservableProperty]
        private string newCategoryName = string.Empty;

        [ObservableProperty]
        private string currencyFrom = "RUB";

        [ObservableProperty]
        private string currencyTo = "USD";

        [ObservableProperty]
        private decimal currencyAmount = 0;

        [ObservableProperty]
        private decimal convertedAmount = 0;

        [ObservableProperty]
        private bool isLoadingRates = false;

        [ObservableProperty]
        private string baseCurrency = "RUB";

        public MainViewModel()
        {
            _dataService = new DataService();
            _currencyService = new CurrencyService();
            LoadData();
            InitializeTodayEntry();
            LoadCurrencies();
            _ = LoadExchangeRatesAsync();
        }

        private void LoadData()
        {
            var transactions = _dataService.LoadTransactions();
            Transactions = new ObservableCollection<Transaction>(transactions.OrderByDescending(t => t.Date));

            var entries = _dataService.LoadDailyEntries();
            DailyEntries = new ObservableCollection<DailyEntry>(entries.OrderByDescending(e => e.Date));

            var habits = _dataService.LoadHabits();
            AvailableHabits = new ObservableCollection<string>(habits);

            var budgets = _dataService.LoadBudgets();
            Budgets = new ObservableCollection<Budget>(budgets);

            var categories = _dataService.LoadCategories();
            Categories = new ObservableCollection<string>(categories);

            CalculateTotals();
        }

        private void LoadCurrencies()
        {
            var currencies = _currencyService.GetAvailableCurrencies();
            AvailableCurrencies = new ObservableCollection<Currency>(currencies);
        }

        private async Task LoadExchangeRatesAsync()
        {
            await Task.CompletedTask;
        }

        private void InitializeTodayEntry()
        {
            var today = DateTime.Today;
            SelectedDailyEntry = DailyEntries.FirstOrDefault(e => e.Date.Date == today);

            if (SelectedDailyEntry == null)
            {
                SelectedDailyEntry = new DailyEntry
                {
                    Date = today,
                    Habits = AvailableHabits.Select(h => new HabitCheck { HabitName = h }).ToList()
                };
                DailyEntries.Insert(0, SelectedDailyEntry);
            }

            DailyNotes = SelectedDailyEntry.Notes;
            DailyMood = SelectedDailyEntry.Mood;
        }

        [RelayCommand]
        private void AddTransaction()
        {
            if (string.IsNullOrWhiteSpace(NewTransactionDescription) || NewTransactionAmount <= 0)
                return;

            var transaction = new Transaction
            {
                Description = NewTransactionDescription,
                Amount = NewTransactionAmount,
                Type = NewTransactionType,
                Category = NewTransactionCategory,
                Currency = NewTransactionCurrency,
                Date = DateTime.Now
            };

            Transactions.Insert(0, transaction);
            _dataService.SaveTransactions(Transactions.ToList());

            NewTransactionDescription = string.Empty;
            NewTransactionAmount = 0;
            NewTransactionCategory = string.Empty;

            CalculateTotals();
        }

        [RelayCommand]
        private void DeleteTransaction(Transaction? transaction)
        {
            if (transaction == null) return;

            Transactions.Remove(transaction);
            _dataService.SaveTransactions(Transactions.ToList());
            CalculateTotals();
        }

        [RelayCommand]
        private void AddHabit()
        {
            if (string.IsNullOrWhiteSpace(NewHabitName)) return;
            if (AvailableHabits.Contains(NewHabitName)) return;

            AvailableHabits.Add(NewHabitName);
            _dataService.SaveHabits(AvailableHabits.ToList());

            if (SelectedDailyEntry != null)
            {
                SelectedDailyEntry.Habits.Add(new HabitCheck { HabitName = NewHabitName });
            }

            NewHabitName = string.Empty;
        }

        [RelayCommand]
        private void DeleteHabit(string? habit)
        {
            if (habit == null) return;

            AvailableHabits.Remove(habit);
            _dataService.SaveHabits(AvailableHabits.ToList());
        }

        [RelayCommand]
        private void AddTask()
        {
            if (string.IsNullOrWhiteSpace(NewTaskDescription) || SelectedDailyEntry == null) return;

            SelectedDailyEntry.Tasks.Add(new TaskItem
            {
                Description = NewTaskDescription,
                Priority = 3
            });

            NewTaskDescription = string.Empty;
            SaveDailyEntries();
        }

        [RelayCommand]
        private void DeleteTask(TaskItem? task)
        {
            if (task == null || SelectedDailyEntry == null) return;

            SelectedDailyEntry.Tasks.Remove(task);
            SaveDailyEntries();
        }

        [RelayCommand]
        private void SaveDailyEntry()
        {
            if (SelectedDailyEntry == null) return;

            SelectedDailyEntry.Notes = DailyNotes;
            SelectedDailyEntry.Mood = DailyMood;
            SaveDailyEntries();
        }

        [RelayCommand]
        private void ToggleHabit(HabitCheck? habit)
        {
            if (habit == null) return;
            habit.IsCompleted = !habit.IsCompleted;
            SaveDailyEntries();
        }

        [RelayCommand]
        private void ToggleTask(TaskItem? task)
        {
            if (task == null) return;
            task.IsCompleted = !task.IsCompleted;
            SaveDailyEntries();
        }

        [RelayCommand]
        private void AddBudget()
        {
            if (string.IsNullOrWhiteSpace(NewBudgetCategory) || NewBudgetAmount <= 0) return;

            var budget = new Budget
            {
                Category = NewBudgetCategory,
                Amount = NewBudgetAmount,
                Currency = NewBudgetCurrency,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            };

            Budgets.Add(budget);
            _dataService.SaveBudgets(Budgets.ToList());

            NewBudgetCategory = string.Empty;
            NewBudgetAmount = 0;
        }

        [RelayCommand]
        private void DeleteBudget(Budget? budget)
        {
            if (budget == null) return;

            Budgets.Remove(budget);
            _dataService.SaveBudgets(Budgets.ToList());
        }

        [RelayCommand]
        private void AddCategory()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName)) return;
            if (Categories.Contains(NewCategoryName)) return;

            Categories.Add(NewCategoryName);
            _dataService.SaveCategories(Categories.ToList());
            NewCategoryName = string.Empty;
        }

        [RelayCommand]
        private void DeleteCategory(string? category)
        {
            if (category == null) return;

            Categories.Remove(category);
            _dataService.SaveCategories(Categories.ToList());
        }

        [RelayCommand]
        private async Task ConvertCurrencyAsync()
        {
            if (CurrencyAmount <= 0) return;

            IsLoadingRates = true;
            try
            {
                var rate = await _currencyService.GetExchangeRateAsync(CurrencyFrom, CurrencyTo);
                ConvertedAmount = CurrencyAmount * rate;
            }
            catch
            {
                ConvertedAmount = _currencyService.ConvertCurrency(CurrencyAmount, CurrencyFrom, CurrencyTo);
            }
            finally
            {
                IsLoadingRates = false;
            }
        }

        partial void OnCurrencyAmountChanged(decimal value)
        {
            _ = ConvertCurrencyAsync();
        }

        partial void OnCurrencyFromChanged(string value)
        {
            _ = ConvertCurrencyAsync();
        }

        partial void OnCurrencyToChanged(string value)
        {
            _ = ConvertCurrencyAsync();
        }

        private void SaveDailyEntries()
        {
            _dataService.SaveDailyEntries(DailyEntries.ToList());
        }

        private void CalculateTotals()
        {
            // Конвертируем все транзакции в базовую валюту
            TotalIncome = Transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Currency == BaseCurrency 
                    ? t.Amount 
                    : _currencyService.ConvertCurrency(t.Amount, t.Currency, BaseCurrency));

            TotalExpenses = Transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Currency == BaseCurrency 
                    ? t.Amount 
                    : _currencyService.ConvertCurrency(t.Amount, t.Currency, BaseCurrency));

            Balance = TotalIncome - TotalExpenses;
        }

        public decimal GetCategoryTotal(string category)
        {
            return Transactions
                .Where(t => t.Category == category && t.Type == TransactionType.Expense)
                .Sum(t => t.Currency == BaseCurrency 
                    ? t.Amount 
                    : _currencyService.ConvertCurrency(t.Amount, t.Currency, BaseCurrency));
        }

        public decimal GetBudgetProgress(string category)
        {
            var budget = Budgets.FirstOrDefault(b => b.Category == category);
            if (budget == null) return 0;

            var spent = GetCategoryTotal(category);
            return budget.Amount > 0 ? (spent / budget.Amount) * 100 : 0;
        }
    }
}
