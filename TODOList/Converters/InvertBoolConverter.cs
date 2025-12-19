using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Echoslate.Converters {
	public class InvertBoolConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is not bool boolValue) {
				return false;
			}
			return !boolValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}