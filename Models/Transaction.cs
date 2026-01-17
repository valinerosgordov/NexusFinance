using System;

namespace FinancialPlanner.Models
{
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
        public string Notes { get; set; } = string.Empty;
        public string Currency { get; set; } = "RUB";
    }

    public enum TransactionType
    {
        Income,
        Expense
    }
}
