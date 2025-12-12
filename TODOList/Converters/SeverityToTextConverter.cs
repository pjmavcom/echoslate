using System;
using System.Globalization;
using System.Windows.Data;

namespace Echoslate.Resources {
	public class SeverityToTextConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> value is int s ? s switch { 0 => "None", 1 => "Low", 2 => "Med", 3 => "High", _ => "" } : "";

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}