namespace NexusFinance;

/// <summary>
/// Application-wide constants. Eliminates magic strings (Clean Code principle).
/// WHY: Hardcoded strings lead to typos, inconsistencies, and maintenance nightmares.
/// </summary>
public static class Constants
{
    public static class TransactionTypes
    {
        public const string Income = "Income";
        public const string Expense = "Expense";
    }

    public static class AccountTypes
    {
        public const string Checking = "Checking";
        public const string Savings = "Savings";
        public const string Cash = "Cash";
    }

    public static class InvestmentTypes
    {
        public const string Stock = "Stock";
        public const string Crypto = "Crypto";
        public const string Bond = "Bond";
        public const string RealEstate = "RealEstate";
    }

    public static class ProjectNames
    {
        public const string Personal = "Personal";
    }

    public static class Colors
    {
        public const string IncomeGreen = "#00C853";
        public const string ExpenseRed = "#D50000";
        public const string NeutralGray = "#C0C0C0";
        public const string WarningOrange = "#FF9800";
        public const string CriticalRed = "#FF5252";
    }

    public static class ErrorMessages
    {
        public const string ValidationError = "Validation Error";
        public const string SaveError = "Failed to save data";
        public const string LoadError = "Failed to load data";
        public const string DeleteConfirmation = "Confirm Delete";
        public const string Success = "Success";
        public const string ApiKeyMissing = "API Key not configured";
        public const string ApiKeyInvalid = "Invalid API Key";
        public const string NetworkError = "Network connection error";
    }

    public static class DateFormats
    {
        public const string ShortDate = "dd.MM.yyyy";
        public const string DateTime = "dd.MM.yyyy HH:mm";
        public const string FullDateTime = "yyyy-MM-dd HH:mm:ss.fff";
        public const string MonthYear = "MMM yyyy";
        public const string MonthDay = "MMM dd, yyyy";
    }

    public static class Defaults
    {
        public const int RecentTransactionsCount = 10;
        public const int TopExpensesCount = 5;
        public const int DaysForMonthlyCalculation = 30;
        public const decimal MinimumBalance = 0m;
    }
}
