using System;
using System.Globalization;
using System.Windows.Data;

namespace Echoslate.Converters {
	public class WidthToColumnWidthConverter : IValueConverter {
		public double Threshold { get; set; } = 600;
		public double DefaultWidth { get; set; } = 100;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is double actualWidth) {
				if (parameter is string paramString) {
					var parts = paramString.Split('|');
					if (parts.Length == 2 &&
						double.TryParse(parts[0], out double customThreshold) &&
						double.TryParse(parts[1], out double customWidth)) {
						return actualWidth < customThreshold ? 0.0 : customWidth;
					}
				}

				return actualWidth < Threshold ? 0.0 : DefaultWidth;
			}

			return DefaultWidth;
		}
		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}