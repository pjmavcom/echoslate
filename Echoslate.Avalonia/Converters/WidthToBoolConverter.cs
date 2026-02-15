using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Echoslate.Avalonia.Converters;

public class WidthToBoolConverter : IValueConverter {
	public double Threshold { get; set; } = 900;

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		double width;
		switch (value) {
			case double w:
				width = w;
				break;
			case Rect rect:
				width = rect.Width;
				break;
			case Size size:
				width = size.Width;
				break;
			default:
				return false;
		}
		bool isWide = width >= Threshold;
		bool wantWide = true;
		if (parameter is string paramStr && paramStr.Equals("False", StringComparison.OrdinalIgnoreCase)) {
			wantWide = false;
		}
		return isWide == wantWide;
	}
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}