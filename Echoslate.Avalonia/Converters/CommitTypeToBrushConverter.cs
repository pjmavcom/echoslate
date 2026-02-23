using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Echoslate.Core.Services;

namespace Echoslate.Avalonia.Converters;
public class CommitTypeToBrushConverter : IValueConverter {
	private static readonly Brush DefaultBrush = (Brush)BrushService.DefaultBrush;

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is string type) {
			return BrushService.GetBrushForCommitType(type);
		}
		return BrushService.DefaultBrush;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}
