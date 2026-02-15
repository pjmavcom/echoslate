using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Echoslate.Avalonia.Converters;

public class BoolToFontWeightConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> (bool)value ? FontWeight.Bold : FontWeight.Normal;
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}