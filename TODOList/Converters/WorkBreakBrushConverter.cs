using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Echoslate.ViewModels;

namespace Echoslate.Converters;

public class WorkBreakBrushConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		PomoActiveState isWork = (PomoActiveState)value;
		return isWork switch {
			PomoActiveState.Work => new SolidColorBrush(Color.FromRgb(220, 50, 50)),
			PomoActiveState.Break => new SolidColorBrush(Color.FromRgb(50, 180, 80)),
			PomoActiveState.Idle => new SolidColorBrush(Color.FromRgb(49, 49, 49)),
			_ => new SolidColorBrush(Color.FromRgb(30, 30, 30))
		};
	}
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}