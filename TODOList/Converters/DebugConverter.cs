using System;
using System.Globalization;
using System.Windows.Data;
using System.Diagnostics;  // for Debug.WriteLine

namespace Echoslate.Converters
{
    /// <summary>
    /// Converts any value to itself while logging what it receives.
    /// Perfect for debugging bindings — see output in Visual Studio Output window (Debug tab)
    /// </summary>
    [ValueConversion(typeof(object), typeof(object))]
    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine($"[DebugConverter] Value: '{value}' (Type: {value?.GetType().Name ?? "null"})");
            Debug.WriteLine($"[DebugConverter] Parameter: '{parameter}'");
            Debug.WriteLine($"[DebugConverter] TargetType: {targetType.Name}");
            Debug.WriteLine("---");

            return value; // pass through unchanged
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine($"[DebugConverter BACK] Value: '{value}'");
            return value;
        }
    }
}
