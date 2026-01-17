using System;

namespace FinancialPlanner.Models
{
    public class Currency
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
    }

    public class ExchangeRate
    {
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class Budget
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);
        public string Currency { get; set; } = "RUB";
    }
}
