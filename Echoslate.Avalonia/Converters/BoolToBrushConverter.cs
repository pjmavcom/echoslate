using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Echoslate.Avalonia.Converters;

public class BoolToBrushConverter : IValueConverter {
	public IBrush? TrueBrush { get; set; }
	public IBrush? FalseBrush { get; set; }

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> (bool)value ? TrueBrush : FalseBrush;
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}