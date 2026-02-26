using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Echoslate.Core.Services;
using Echoslate.Core.Theming;
using Echoslate.Core.ViewModels;

namespace Echoslate.Wpf.Converters;

public class WorkBreakBrushConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		PomoActiveState isWork = (PomoActiveState)value;
		return isWork switch {
			PomoActiveState.Work => BrushService.CreateBrush(ColorRgba.DangerRed),
			PomoActiveState.Break => BrushService.CreateBrush(ColorRgba.SuccessGreen),
			PomoActiveState.Idle => BrushService.CreateBrush(ColorRgba.ChoreGray),
			_ => BrushService.CreateBrush(ColorRgba.ChoreGray)
		};
	}
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}