using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Echoslate.Converters;

public class BoolToFontWeightConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> (bool)value ? FontWeights.Bold : FontWeights.Normal;
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}