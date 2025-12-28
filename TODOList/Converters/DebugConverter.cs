using System;
using System.Globalization;
using System.Windows.Data;
using System.Diagnostics;

namespace Echoslate.Converters;

[ValueConversion(typeof(object), typeof(object))]
public class DebugConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		Debug.WriteLine($"[DebugConverter] Value: '{value}' (Type: {value?.GetType().Name ?? "null"})");
		Debug.WriteLine($"[DebugConverter] Parameter: '{parameter}'");
		Debug.WriteLine($"[DebugConverter] TargetType: {targetType.Name}");
		Debug.WriteLine("---");

		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		Debug.WriteLine($"[DebugConverter BACK] Value: '{value}'");
		return value;
	}
}