using System;

namespace FinancialPlanner.Models
{
    public class LevelSystem
    {
        public int Level { get; set; } = 1;
        public long TotalXP { get; set; } = 0;
        public long CurrentLevelXP { get; set; } = 0;
        public long XPToNextLevel { get; set; } = 100;
        public DateTime LastLevelUp { get; set; } = DateTime.MinValue;
    }

    public static class LevelCalculator
    {
        // Формула XP для следующего уровня (экспоненциальный рост как в Solo Leveling)
        public static long GetXPForLevel(int level)
        {
            // Базовый XP * (уровень^1.5) - экспоненциальный рост
            return (long)(100 * Math.Pow(level, 1.5));
        }

        public static int CalculateLevel(long totalXP)
        {
            int level = 1;
            long xpNeeded = 0;

            while (xpNeeded <= totalXP)
            {
                level++;
                xpNeeded += GetXPForLevel(level - 1);
                
                if (level > 1000) break; // Защита от бесконечного цикла
            }

            return Math.Max(1, level - 1);
        }

        public static (long CurrentLevelXP, long XPToNextLevel) GetLevelProgress(long totalXP, int currentLevel)
        {
            long xpForCurrentLevel = 0;
            for (int i = 1; i < currentLevel; i++)
            {
                xpForCurrentLevel += GetXPForLevel(i);
            }

            long currentLevelXP = totalXP - xpForCurrentLevel;
            long xpToNextLevel = GetXPForLevel(currentLevel);

            return (currentLevelXP, xpToNextLevel);
        }

        // XP за выполнение задачи в зависимости от приоритета
        public static int GetXPForTask(int priority)
        {
            // Приоритет 1-5, больше приоритет = больше XP
            return priority * 20 + 10; // 30, 50, 70, 90, 110 XP
        }

        // XP за выполнение привычки
        public static int GetXPForHabit()
        {
            return 15; // Базовая награда за привычку
        }
    }
}
