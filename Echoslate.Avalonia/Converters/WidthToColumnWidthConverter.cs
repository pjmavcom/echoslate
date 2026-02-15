using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Echoslate.Avalonia.Converters;

public class WidthToColumnWidthConverter : IValueConverter {
	public double Threshold { get; set; } = 600;
	public double DefaultWidth { get; set; } = 100;

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		double actualWidth;
		switch (value) {
			case double d:
				actualWidth = d;
				break;
			case Rect rect:
				actualWidth = rect.Width;
				break;
			case Size size:
				actualWidth = size.Width;
				break;
			default:
				return DefaultWidth;
		}
		if (parameter is string paramString) {
			var parts = paramString.Split('|');
			if (parts.Length == 2 &&
				double.TryParse(parts[0], out double customThreshold) &&
				double.TryParse(parts[1], out double customWidth)) {
				return actualWidth < customThreshold ? 0.0 : customWidth;
			}
		}


		return DefaultWidth;
	}
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}