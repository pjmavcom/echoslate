using System;
using System.Globalization;
using System.Windows.Data;

namespace Echoslate.Wpf.Converters;

public class ProgressToWidthConverter : IMultiValueConverter {
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
		if (values.Length == 4 &&
			values[0] is double actualWidth &&
			values[1] is double value &&
			values[2] is double maximum &&
			values[3] is double minimum) {
			double percentage = (value - minimum) / (maximum - minimum);
			return actualWidth * percentage;
		}
		return 0;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}