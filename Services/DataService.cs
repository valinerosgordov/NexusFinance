using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using NexusFinance.Models;

namespace NexusFinance.Services;

/// <summary>
/// JSON-based data persistence service implementing Repository Pattern.
/// Thread-safe singleton ensures data consistency across the application.
/// </summary>
public sealed class DataService : IDataService
{
    private static readonly string DataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NexusFinance",
        "data.json"
    );

    private readonly GlobalExceptionHandler _exceptionHandler;
    private AppData _data;
    private readonly object _dataLock = new(); // Thread safety for file I/O

    public DataService()
    {
        _exceptionHandler = GlobalExceptionHandler.Instance;
        _data = LoadData();
    }

    // Projects
    public ObservableCollection<Project> GetProjects()
    {
        lock (_dataLock)
        {
            return new ObservableCollection<Project>(_data.Projects);
        }
    }

    public void AddProject(Project project)
    {
        ArgumentNullException.ThrowIfNull(project, nameof(project));
        
        if (string.IsNullOrWhiteSpace(project.Name))
            throw new ArgumentException("Project name cannot be empty", nameof(project));

        lock (_dataLock)
        {
            if (_data.Projects.Any(p => p.Name == project.Name))
                throw new InvalidOperationException($"Project '{project.Name}' already exists");

            _data.Projects.Add(project);
            SaveData();
        }
    }

    public void UpdateProject(string oldName, Project updatedProject)
    {
        ArgumentNullException.ThrowIfNull(updatedProject, nameof(updatedProject));
        
        if (string.IsNullOrWhiteSpace(oldName))
            throw new ArgumentException("Old project name cannot be empty", nameof(oldName));

        lock (_dataLock)
        {
            var index = _data.Projects.FindIndex(p => p.Name == oldName);
            if (index < 0)
                throw new InvalidOperationException($"Project '{oldName}' not found");

            _data.Projects[index] = updatedProject;
            SaveData();
        }
    }

    public void DeleteProject(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty", nameof(name));

        lock (_dataLock)
        {
            var removed = _data.Projects.RemoveAll(p => p.Name == name);
            if (removed > 0)
            {
                SaveData();
            }
        }
    }

    // Transactions
    public ObservableCollection<Transaction> GetTransactions()
    {
        lock (_dataLock)
        {
            return new ObservableCollection<Transaction>(_data.Transactions);
        }
    }

    public void AddTransaction(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));
        
        if (transaction.Amount < 0)
            throw new ArgumentException("Transaction amount cannot be negative", nameof(transaction));

        lock (_dataLock)
        {
            _data.Transactions.Add(transaction);
            SaveData();
        }
    }

    public void DeleteTransaction(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));

        lock (_dataLock)
        {
            _data.Transactions.Remove(transaction);
            SaveData();
        }
    }

    // Accounts
    public ObservableCollection<Account> GetAccounts()
    {
        lock (_dataLock)
        {
            return new ObservableCollection<Account>(_data.Accounts);
        }
    }

    public void AddAccount(Account account)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));
        
        if (string.IsNullOrWhiteSpace(account.Name))
            throw new ArgumentException("Account name cannot be empty", nameof(account));

        lock (_dataLock)
        {
            if (_data.Accounts.Any(a => a.Name == account.Name))
                throw new InvalidOperationException($"Account '{account.Name}' already exists");

            _data.Accounts.Add(account);
            SaveData();
        }
    }

    public void UpdateAccount(string oldName, Account updatedAccount)
    {
        ArgumentNullException.ThrowIfNull(updatedAccount, nameof(updatedAccount));
        
        if (string.IsNullOrWhiteSpace(oldName))
            throw new ArgumentException("Old account name cannot be empty", nameof(oldName));

        lock (_dataLock)
        {
            var index = _data.Accounts.FindIndex(a => a.Name == oldName);
            if (index < 0)
                throw new InvalidOperationException($"Account '{oldName}' not found");

            _data.Accounts[index] = updatedAccount;
            SaveData();
        }
    }

    public void DeleteAccount(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name cannot be empty", nameof(name));

        lock (_dataLock)
        {
            var removed = _data.Accounts.RemoveAll(a => a.Name == name);
            if (removed > 0)
            {
                SaveData();
            }
        }
    }

    // Investments
    public ObservableCollection<Investment> GetInvestments()
    {
        lock (_dataLock)
        {
            return new ObservableCollection<Investment>(_data.Investments);
        }
    }

    public void AddInvestment(Investment investment)
    {
        ArgumentNullException.ThrowIfNull(investment, nameof(investment));
        
        if (string.IsNullOrWhiteSpace(investment.Name))
            throw new ArgumentException("Investment name cannot be empty", nameof(investment));
        
        if (investment.Amount < 0)
            throw new ArgumentException("Investment amount cannot be negative", nameof(investment));

        lock (_dataLock)
        {
            if (_data.Investments.Any(i => i.Name == investment.Name))
                throw new InvalidOperationException($"Investment '{investment.Name}' already exists");

            _data.Investments.Add(investment);
            SaveData();
        }
    }

    public void UpdateInvestment(string oldName, Investment updatedInvestment)
    {
        ArgumentNullException.ThrowIfNull(updatedInvestment, nameof(updatedInvestment));
        
        if (string.IsNullOrWhiteSpace(oldName))
            throw new ArgumentException("Old investment name cannot be empty", nameof(oldName));

        lock (_dataLock)
        {
            var index = _data.Investments.FindIndex(i => i.Name == oldName);
            if (index < 0)
                throw new InvalidOperationException($"Investment '{oldName}' not found");

            _data.Investments[index] = updatedInvestment;
            SaveData();
        }
    }

    public void DeleteInvestment(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Investment name cannot be empty", nameof(name));

        lock (_dataLock)
        {
            var removed = _data.Investments.RemoveAll(i => i.Name == name);
            if (removed > 0)
            {
                SaveData();
            }
        }
    }

    // Categories
    public ObservableCollection<Category> GetCategories()
    {
        lock (_dataLock)
        {
            return new ObservableCollection<Category>(_data.Categories);
        }
    }

    public void AddCategory(Category category)
    {
        ArgumentNullException.ThrowIfNull(category, nameof(category));
        
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new ArgumentException("Category name cannot be empty", nameof(category));

        lock (_dataLock)
        {
            if (_data.Categories.Any(c => c.Name == category.Name))
                throw new InvalidOperationException($"Category '{category.Name}' already exists");

            _data.Categories.Add(category);
            SaveData();
        }
    }

    public void DeleteCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));

        lock (_dataLock)
        {
            var removed = _data.Categories.RemoveAll(c => c.Name == name);
            if (removed > 0)
            {
                SaveData();
            }
        }
    }
    
    // Payables
    public List<Payable> GetPayables()
    {
        lock (_dataLock)
        {
            return new List<Payable>(_data.Payables);
        }
    }
    
    public void AddPayable(Payable payable)
    {
        ArgumentNullException.ThrowIfNull(payable, nameof(payable));
        
        if (payable.Amount < 0)
            throw new ArgumentException("Payable amount cannot be negative", nameof(payable));

        lock (_dataLock)
        {
            _data.Payables.Add(payable);
            SaveData();
        }
    }
    
    public void UpdatePayable(string id, Payable updatedPayable)
    {
        ArgumentNullException.ThrowIfNull(updatedPayable, nameof(updatedPayable));
        
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Payable ID cannot be empty", nameof(id));

        lock (_dataLock)
        {
            var index = _data.Payables.FindIndex(p => p.Id == id);
            if (index < 0)
                throw new InvalidOperationException($"Payable with ID '{id}' not found");

            updatedPayable.Id = id;
            _data.Payables[index] = updatedPayable;
            SaveData();
        }
    }
    
    public void DeletePayable(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Payable ID cannot be empty", nameof(id));

        lock (_dataLock)
        {
            var removed = _data.Payables.RemoveAll(p => p.Id == id);
            if (removed > 0)
            {
                SaveData();
            }
        }
    }
    
    public void MarkPayableAsPaid(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Payable ID cannot be empty", nameof(id));

        lock (_dataLock)
        {
            var payable = _data.Payables.FirstOrDefault(p => p.Id == id);
            if (payable == null)
                throw new InvalidOperationException($"Payable with ID '{id}' not found");

            payable.IsPaid = true;
            SaveData();
        }
    }
    
    // Receivables
    public List<Receivable> GetReceivables()
    {
        lock (_dataLock)
        {
            return new List<Receivable>(_data.Receivables);
        }
    }
    
    public void AddReceivable(Receivable receivable)
    {
        ArgumentNullException.ThrowIfNull(receivable, nameof(receivable));
        
        if (receivable.Amount < 0)
            throw new ArgumentException("Receivable amount cannot be negative", nameof(receivable));

        lock (_dataLock)
        {
            _data.Receivables.Add(receivable);
            SaveData();
        }
    }
    
    public void UpdateReceivable(string id, Receivable updatedReceivable)
    {
        ArgumentNullException.ThrowIfNull(updatedReceivable, nameof(updatedReceivable));
        
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Receivable ID cannot be empty", nameof(id));

        lock (_dataLock)
        {
            var index = _data.Receivables.FindIndex(r => r.Id == id);
            if (index < 0)
                throw new InvalidOperationException($"Receivable with ID '{id}' not found");

            updatedReceivable.Id = id;
            _data.Receivables[index] = updatedReceivable;
            SaveData();
        }
    }
    
    public void DeleteReceivable(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Receivable ID cannot be empty", nameof(id));

        lock (_dataLock)
        {
            var removed = _data.Receivables.RemoveAll(r => r.Id == id);
            if (removed > 0)
            {
                SaveData();
            }
        }
    }
    
    public void MarkReceivableAsReceived(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Receivable ID cannot be empty", nameof(id));

        lock (_dataLock)
        {
            var receivable = _data.Receivables.FirstOrDefault(r => r.Id == id);
            if (receivable == null)
                throw new InvalidOperationException($"Receivable with ID '{id}' not found");

            receivable.IsReceived = true;
            SaveData();
        }
    }
    
    // Team Members
    public List<TeamMember> GetTeamMembers()
    {
        lock (_dataLock)
        {
            return new List<TeamMember>(_data.TeamMembers);
        }
    }
    
    public List<TeamMember> GetTeamMembersByProject(string projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            throw new ArgumentException("Project ID cannot be empty", nameof(projectId));

        lock (_dataLock)
        {
            return _data.TeamMembers
                .Where(tm => tm.ProjectId == projectId)
                .OrderByDescending(tm => tm.IsActive)
                .ThenBy(tm => tm.Name)
                .ToList();
        }
    }
    
    public void AddTeamMember(TeamMember member)
    {
        ArgumentNullException.ThrowIfNull(member, nameof(member));
        
        if (string.IsNullOrWhiteSpace(member.Name))
            throw new ArgumentException("Team member name cannot be empty", nameof(member));
        
        if (member.Salary < 0)
            throw new ArgumentException("Salary cannot be negative", nameof(member));

        lock (_dataLock)
        {
            _data.TeamMembers.Add(member);
            SaveData();
        }
    }
    
    public void UpdateTeamMember(string id, TeamMember updatedMember)
    {
        ArgumentNullException.ThrowIfNull(updatedMember, nameof(updatedMember));
        
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Team member ID cannot be empty", nameof(id));

        lock (_dataLock)
        {
            var index = _data.TeamMembers.FindIndex(tm => tm.Id == id);
            if (index < 0)
                throw new InvalidOperationException($"Team member with ID '{id}' not found");

            updatedMember.Id = id;
            _data.TeamMembers[index] = updatedMember;
            SaveData();
        }
    }
    
    public void DeleteTeamMember(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Team member ID cannot be empty", nameof(id));

        lock (_dataLock)
        {
            var removed = _data.TeamMembers.RemoveAll(tm => tm.Id == id);
            if (removed > 0)
            {
                SaveData();
            }
        }
    }
    
    public void ToggleTeamMemberStatus(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Team member ID cannot be empty", nameof(id));

        lock (_dataLock)
        {
            var member = _data.TeamMembers.FirstOrDefault(tm => tm.Id == id);
            if (member == null)
                throw new InvalidOperationException($"Team member with ID '{id}' not found");

            member.IsActive = !member.IsActive;
            SaveData();
        }
    }
    
    public decimal CalculateProjectPayroll(string projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            throw new ArgumentException("Project ID cannot be empty", nameof(projectId));

        lock (_dataLock)
        {
            return _data.TeamMembers
                .Where(tm => tm.ProjectId == projectId && tm.IsActive)
                .Sum(tm => tm.MonthlyCost);
        }
    }
    
    public void ClearAllData()
    {
        lock (_dataLock)
        {
            _data = CreateEmptyData();
            SaveData();
        }
    }

    private AppData LoadData()
    {
        try
        {
            if (File.Exists(DataPath))
            {
                var json = File.ReadAllText(DataPath);
                var data = JsonSerializer.Deserialize<AppData>(json);
                
                if (data != null)
                {
                    return data;
                }
            }
        }
        catch (Exception ex)
        {
            _exceptionHandler.LogError(ex, "DataService.LoadData");
            // Fall through to return default data
        }

        return CreateDefaultData();
    }

    private void SaveData()
    {
        try
        {
            var directory = Path.GetDirectoryName(DataPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(_data, options);
            File.WriteAllText(DataPath, json);
        }
        catch (Exception ex)
        {
            _exceptionHandler.LogError(ex, "DataService.SaveData");
            
            System.Windows.MessageBox.Show(
                $"{Constants.ErrorMessages.SaveError}: {ex.Message}", 
                Constants.ErrorMessages.ValidationError,
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Error);
        }
    }

    private static AppData CreateDefaultData()
    {
        return new AppData
        {
            Projects = new List<Project>
            {
                new() { Name = "NexusAI", Revenue = 250000, Cost = 135000, Description = "AI Platform Project" },
                new() { Name = "FinSync", Revenue = 180000, Cost = 95000, Description = "Financial Sync Service" },
                new() { Name = "Personal", Revenue = 0, Cost = 65000, Description = "Personal Expenses" }
            },
            Transactions = new List<Transaction>
            {
                new() { Description = "Salary - NexusAI", Amount = 125000, Date = DateTime.Now, Category = "Income", Project = "NexusAI", IsIncome = true },
                new() { Description = "AWS Cloud Services", Amount = 12500, Date = DateTime.Now.AddDays(-1), Category = "Infrastructure", Project = "NexusAI", IsIncome = false }
            },
            Accounts = new List<Account>
            {
                new() { Name = "Checking Account", Institution = "Sberbank", Balance = 450000, Type = "Checking" },
                new() { Name = "Savings", Institution = "Tinkoff", Balance = 1200000, Type = "Savings" }
            },
            Investments = new List<Investment>
            {
                new() { Name = "Bitcoin", Type = "Crypto", Amount = 500000, CurrentValue = 800000 },
                new() { Name = "TSLA Stock", Type = "Stock", Amount = 300000, CurrentValue = 350000 }
            },
            Categories = new List<Category>
            {
                new() { Name = "Income", Type = "Income", Icon = "💰" },
                new() { Name = "Salary", Type = "Income", Icon = "💵" },
                new() { Name = "Infrastructure", Type = "Expense", Icon = "☁️" },
                new() { Name = "Food", Type = "Expense", Icon = "🍕" },
                new() { Name = "Rent", Type = "Expense", Icon = "🏠" },
                new() { Name = "Transport", Type = "Expense", Icon = "🚗" }
            }
        };
    }
    
    private static AppData CreateEmptyData()
    {
        return new AppData
        {
            Projects = new List<Project>(),
            Transactions = new List<Transaction>(),
            Accounts = new List<Account>(),
            Investments = new List<Investment>(),
            Categories = new List<Category>
            {
                new() { Name = "Income", Type = "Income", Icon = "💰" },
                new() { Name = "Salary", Type = "Income", Icon = "💵" },
                new() { Name = "Infrastructure", Type = "Expense", Icon = "☁️" },
                new() { Name = "Food", Type = "Expense", Icon = "🍕" },
                new() { Name = "Rent", Type = "Expense", Icon = "🏠" },
                new() { Name = "Transport", Type = "Expense", Icon = "🚗" }
            },
            Payables = new List<Payable>(),
            Receivables = new List<Receivable>(),
            TeamMembers = new List<TeamMember>()
        };
    }
}

public class AppData
{
    public List<Project> Projects { get; set; } = new();
    public List<Transaction> Transactions { get; set; } = new();
    public List<Account> Accounts { get; set; } = new();
    public List<Investment> Investments { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Payable> Payables { get; set; } = new();
    public List<Receivable> Receivables { get; set; } = new();
    public List<TeamMember> TeamMembers { get; set; } = new();
}
