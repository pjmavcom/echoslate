using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Echoslate.Converters;

public class CommitTypeToBrushConverter : IValueConverter {
	private static readonly Dictionary<string, Brush> TypeBrushes = new(StringComparer.OrdinalIgnoreCase) {
		{ "feat", new SolidColorBrush(Color.FromRgb(40, 167, 69)) },     // Green (bootstrap success)
		{ "fix", new SolidColorBrush(Color.FromRgb(220, 53, 69)) },      // Red (danger)
		{ "refactor", new SolidColorBrush(Color.FromRgb(0, 123, 255)) }, // Blue (primary)
		{ "chore", new SolidColorBrush(Color.FromRgb(108, 117, 125)) },  // Gray
		{ "docs", new SolidColorBrush(Color.FromRgb(253, 203, 110)) },   // Yellow
		// Add more as you like
	};

	private static readonly Brush DefaultBrush = Brushes.Gray;

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is string type && TypeBrushes.TryGetValue(type, out var brush)) {
			return brush;
		}
		return DefaultBrush;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}