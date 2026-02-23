using System;
using System.Globalization;
using System.Windows.Data;
using Echoslate.Core.Services;

namespace Echoslate.Wpf.Converters;

public class SeverityToBrushConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> value is int s
			? s switch {
				0 => AppServices.BrushService.SeverityNoneBrush,
				1 => AppServices.BrushService.SeverityLowBrush,
				2 => AppServices.BrushService.SeverityMedBrush,
				3 => AppServices.BrushService.SeverityHighBrush,
				_ => AppServices.BrushService.TransparentBrush
			}
			: AppServices.BrushService.TransparentBrush;

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}