using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Echoslate.Core.Services;

namespace Echoslate.Avalonia.Converters;

public class PriorityToBrushConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> value is int s
			? s switch {
				0 => AppServices.BrushService.PriorityNoneBrush,
				1 => AppServices.BrushService.PriorityLowBrush,
				2 => AppServices.BrushService.PriorityMedBrush,
				3 => AppServices.BrushService.PriorityHighBrush,
				4 => AppServices.BrushService.PriorityCritBrush,
				_ => AppServices.BrushService.TransparentBrush
			}
			: AppServices.BrushService.TransparentBrush;

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
