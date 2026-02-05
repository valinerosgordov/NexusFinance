using System.Collections.ObjectModel;
using NexusFinance.Models;

namespace NexusFinance.Services;

/// <summary>
/// Data access abstraction (Repository Pattern + DIP).
/// WHY: ViewModels should depend on abstractions, not concrete implementations.
/// This enables testing, mocking, and future DB migrations without touching ViewModels.
/// </summary>
public interface IDataService
{
    // Projects
    ObservableCollection<Project> GetProjects();
    void AddProject(Project project);
    void UpdateProject(string oldName, Project updatedProject);
    void DeleteProject(string name);

    // Transactions
    ObservableCollection<Transaction> GetTransactions();
    void AddTransaction(Transaction transaction);
    void DeleteTransaction(Transaction transaction);

    // Accounts
    ObservableCollection<Account> GetAccounts();
    void AddAccount(Account account);
    void UpdateAccount(string oldName, Account updatedAccount);
    void DeleteAccount(string name);

    // Investments
    ObservableCollection<Investment> GetInvestments();
    void AddInvestment(Investment investment);
    void UpdateInvestment(string oldName, Investment updatedInvestment);
    void DeleteInvestment(string name);

    // Categories
    ObservableCollection<Category> GetCategories();
    void AddCategory(Category category);
    void DeleteCategory(string name);

    // Payables
    List<Payable> GetPayables();
    void AddPayable(Payable payable);
    void UpdatePayable(string id, Payable updatedPayable);
    void DeletePayable(string id);
    void MarkPayableAsPaid(string id);

    // Receivables
    List<Receivable> GetReceivables();
    void AddReceivable(Receivable receivable);
    void UpdateReceivable(string id, Receivable updatedReceivable);
    void DeleteReceivable(string id);
    void MarkReceivableAsReceived(string id);

    // Team Members
    List<TeamMember> GetTeamMembers();
    List<TeamMember> GetTeamMembersByProject(string projectId);
    void AddTeamMember(TeamMember member);
    void UpdateTeamMember(string id, TeamMember updatedMember);
    void DeleteTeamMember(string id);
    void ToggleTeamMemberStatus(string id);
    decimal CalculateProjectPayroll(string projectId);
    void ClearAllData();
}
