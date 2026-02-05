using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Echoslate.Core.Components;

namespace Echoslate.Avalonia.Converters;

public class IsTagSelectedConverter : IMultiValueConverter {
	public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
		if (values.Count < 2 || values[0] is not FilterButton button || values[1] is not string selectedTag) {
			return false;
		}

		return (button.Filter == "All" && string.IsNullOrEmpty(selectedTag)) ||
			   string.Equals(button.Filter, selectedTag, StringComparison.OrdinalIgnoreCase);
	}
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}