using System;
using System.Globalization;
using System.Windows.Data;

namespace Echoslate.Converters {
	public class DivideWidthConverter : IValueConverter {
		public double Divisor { get; set; } = 3.0; // default divide by 3
		public double Subtract { get; set; } = 40; // subtract for margins/padding/gaps

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is double width) {
				double divisor = Divisor;
				if (parameter is double p) divisor = p;

				return Math.Max(0, (width / divisor) - Subtract);
			}
			return 0d;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}