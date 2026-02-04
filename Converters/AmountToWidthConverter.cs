using System.Globalization;
using System.Windows.Data;

namespace NexusFinance.Converters;

public class AmountToWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal amount)
        {
            // Scale the width proportionally (max 400px for 100k)
            var width = Math.Min((double)amount / 100000 * 400, 400);
            return Math.Max(width, 10);
        }
        return 10.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
