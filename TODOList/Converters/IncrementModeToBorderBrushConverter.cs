using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Echoslate.ViewModels;

namespace Echoslate.Converters {
	public class IncrementModeToBorderBrushConverter : IValueConverter {
		public Brush HighlightBrush { get; set; }
		public Brush NormalBrush { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is IncrementMode mode && parameter is IncrementMode targetMode) {
				return mode == targetMode ? HighlightBrush : NormalBrush;
			}
			return NormalBrush;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}