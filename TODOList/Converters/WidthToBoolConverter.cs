using System.Windows;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Echoslate.Converters {
	public class WidthToBoolConverter : IValueConverter {
		public double Threshold { get; set; } = 900;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is double width) {
				bool isWide = width >= Threshold;
				// parameter True = visible when wide, False = visible when narrow
				bool wantWide = parameter is string and "False" ? false : true;
				return isWide == wantWide ? Visibility.Visible : Visibility.Collapsed;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}