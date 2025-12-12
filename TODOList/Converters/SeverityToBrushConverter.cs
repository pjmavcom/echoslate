using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Echoslate.Converters {

	public class SeverityToBrushConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> value is int s
				   ? s switch {
						 0 => new SolidColorBrush(Color.FromRgb(40, 40, 40)), // dark gray
						 1 => new SolidColorBrush(Color.FromRgb(0, 135, 0)), // green
						 2 => new SolidColorBrush(Color.FromRgb(200, 150, 0)), // yellow/orange
						 3 => new SolidColorBrush(Color.FromRgb(180, 0, 0)), // red
						 _ => Brushes.Transparent
					 }
				   : Brushes.Transparent;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}