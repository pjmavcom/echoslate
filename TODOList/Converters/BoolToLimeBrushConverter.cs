using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Echoslate.Converters;

public class BoolToLimeBrushConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is bool and true) {
			return new SolidColorBrush(Colors.Lime);
		} else {
			return new SolidColorBrush(Colors.Transparent);
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}