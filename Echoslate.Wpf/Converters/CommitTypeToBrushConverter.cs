using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Echoslate.Wpf.Services;

namespace Echoslate.Wpf.Converters;

public class CommitTypeToBrushConverter : IValueConverter {
	private static readonly Brush DefaultBrush = Brushes.Gray;

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is string type) {
			return WpfBrushService.GetBrushForCommitType(type);
		}
		return WpfBrushService.DefaultBrush;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}