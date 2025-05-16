using System.Globalization;
using System.Windows.Data;

namespace Bruh.VMTools
{
    public class StringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() != "" && decimal.TryParse(value?.ToString(), out decimal result))
                return result;
            else
                return null;
        }

        public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "";
        }
    }
}
