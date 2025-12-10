using System;
using System.Globalization;
using System.Windows.Data;

namespace Echoslate.Converters;

public class IsTagSelectedConverter : IMultiValueConverter {
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
		if (values.Length < 2 || values[0] is not string tag || values[1] is not string selectedTag)
			return false;

		// Handles "All" when nothing selected, OR exact tag match
		return (tag == "All" && string.IsNullOrEmpty(selectedTag)) ||
			   string.Equals(tag, selectedTag, StringComparison.OrdinalIgnoreCase);
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}