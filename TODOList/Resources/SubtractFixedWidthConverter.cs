using System;
using System.Globalization;
using System.Windows.Data;

namespace TODOList.Resources;

// SubtractFixedWidthConverter.cs
public class SubtractFixedWidthConverter : IValueConverter {
	public object Convert(object value, Type t, object parameter, CultureInfo c) {
		if (value is double availableWidth && parameter is string s && double.TryParse(s, out double fixedTotal))
			return Math.Max(50, availableWidth - fixedTotal); // 50 = minimum for ellipsis
		return 300d; // fallback
	}

	public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}