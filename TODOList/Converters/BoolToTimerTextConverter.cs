using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Echoslate.Converters;

public class BoolToTimerTextConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is bool isOn && isOn)
			return Brushes.Black;
		else
			return Brushes.LightGray;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}