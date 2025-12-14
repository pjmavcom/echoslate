using System;
using System.Globalization;
using System.Windows.Data;

namespace Echoslate.Converters {
	public class WidthMinusConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is double width && parameter is string s && double.TryParse(s, out double subtract))
				return width - subtract;
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}