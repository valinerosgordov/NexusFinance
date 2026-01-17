using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using FinancialPlanner.Models;

namespace FinancialPlanner.Converters
{
    public class TransactionColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TransactionType type)
            {
                return type == TransactionType.Income 
                    ? new SolidColorBrush(Color.FromRgb(209, 250, 229)) // Light green
                    : new SolidColorBrush(Color.FromRgb(254, 226, 226)); // Light red
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TransactionBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TransactionType type)
            {
                return type == TransactionType.Income 
                    ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) // Green
                    : new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BalanceColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal balance)
            {
                return balance >= 0 
                    ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) // Green
                    : new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StrikeThroughConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCompleted && isCompleted)
            {
                return TextDecorations.Strikethrough;
            }
            return new TextDecorationCollection();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
