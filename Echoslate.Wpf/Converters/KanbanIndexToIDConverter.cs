using System;
using System.Globalization;
using System.Windows.Data;

namespace Echoslate.Wpf.Converters;

public class KanbanIndexToIDConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is int index) {
			return index switch {
				1 => "BackLog",
				2 => "Next",
				3 => "Current",
				_ => "None"
			};
		}
		return "None";
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}