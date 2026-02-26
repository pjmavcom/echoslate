using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Echoslate.Avalonia.Converters;

public class BoolToVisibilityConverter : IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value is not bool boolValue) {
			return false;
		}

		bool result = boolValue;

		if (parameter is string s &&
			s.Equals("invert", StringComparison.OrdinalIgnoreCase)) {
			result = !boolValue;
		}

		return result;
	}
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}