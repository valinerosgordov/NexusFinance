using System;
using System.Collections.Generic;

namespace FinancialPlanner.Models
{
    public class DailyEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; } = DateTime.Today;
        public string Notes { get; set; } = string.Empty;
        public int Mood { get; set; } = 5; // 1-10 scale
        public List<HabitCheck> Habits { get; set; } = new List<HabitCheck>();
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }

    public class HabitCheck
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string HabitName { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }

    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public int Priority { get; set; } = 3; // 1-5 scale
        public int XP { get; set; } = 0; // XP за выполнение
        public bool XPClaimed { get; set; } = false; // Была ли награда получена
    }
}
