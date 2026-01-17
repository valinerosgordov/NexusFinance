using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FinancialPlanner.Models;
using Newtonsoft.Json;

namespace FinancialPlanner.Services
{
    public class DataService
    {
        private readonly string _dataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FinancialPlanner");

        private readonly string _transactionsFile;
        private readonly string _dailyEntriesFile;
        private readonly string _habitsFile;
        private readonly string _budgetsFile;
        private readonly string _categoriesFile;

        public DataService()
        {
            Directory.CreateDirectory(_dataFolder);
            _transactionsFile = Path.Combine(_dataFolder, "transactions.json");
            _dailyEntriesFile = Path.Combine(_dataFolder, "daily_entries.json");
            _habitsFile = Path.Combine(_dataFolder, "habits.json");
            _budgetsFile = Path.Combine(_dataFolder, "budgets.json");
            _categoriesFile = Path.Combine(_dataFolder, "categories.json");
        }

        public List<Transaction> LoadTransactions()
        {
            if (!File.Exists(_transactionsFile))
                return new List<Transaction>();

            var json = File.ReadAllText(_transactionsFile);
            return JsonConvert.DeserializeObject<List<Transaction>>(json) ?? new List<Transaction>();
        }

        public void SaveTransactions(List<Transaction> transactions)
        {
            var json = JsonConvert.SerializeObject(transactions, Formatting.Indented);
            File.WriteAllText(_transactionsFile, json);
        }

        public List<DailyEntry> LoadDailyEntries()
        {
            if (!File.Exists(_dailyEntriesFile))
                return new List<DailyEntry>();

            var json = File.ReadAllText(_dailyEntriesFile);
            return JsonConvert.DeserializeObject<List<DailyEntry>>(json) ?? new List<DailyEntry>();
        }

        public void SaveDailyEntries(List<DailyEntry> entries)
        {
            var json = JsonConvert.SerializeObject(entries, Formatting.Indented);
            File.WriteAllText(_dailyEntriesFile, json);
        }

        public List<string> LoadHabits()
        {
            if (!File.Exists(_habitsFile))
                return new List<string>();

            var json = File.ReadAllText(_habitsFile);
            return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }

        public void SaveHabits(List<string> habits)
        {
            var json = JsonConvert.SerializeObject(habits, Formatting.Indented);
            File.WriteAllText(_habitsFile, json);
        }

        public List<Budget> LoadBudgets()
        {
            if (!File.Exists(_budgetsFile))
                return new List<Budget>();

            var json = File.ReadAllText(_budgetsFile);
            return JsonConvert.DeserializeObject<List<Budget>>(json) ?? new List<Budget>();
        }

        public void SaveBudgets(List<Budget> budgets)
        {
            var json = JsonConvert.SerializeObject(budgets, Formatting.Indented);
            File.WriteAllText(_budgetsFile, json);
        }

        public List<string> LoadCategories()
        {
            if (!File.Exists(_categoriesFile))
                return new List<string> { "Еда", "Транспорт", "Развлечения", "Здоровье", "Одежда", "Жилье", "Образование", "Прочее" };

            var json = File.ReadAllText(_categoriesFile);
            return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }

        public void SaveCategories(List<string> categories)
        {
            var json = JsonConvert.SerializeObject(categories, Formatting.Indented);
            File.WriteAllText(_categoriesFile, json);
        }
    }
}
