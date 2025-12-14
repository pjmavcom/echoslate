using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Echoslate.Converters {
	public class BoolToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is not bool boolValue) {
				return Visibility.Collapsed;
			}
			if (parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase)) {
				return boolValue ? Visibility.Collapsed : Visibility.Visible;
			}

			return boolValue ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}