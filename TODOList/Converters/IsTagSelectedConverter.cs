using System;
using System.Globalization;
using System.Windows.Data;
using Echoslate.Components;


namespace Echoslate.Converters {
	public class IsTagSelectedConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if (values.Length < 2 || values[0] is not FilterButton button || values[1] is not string selectedTag) {
				return false;
			}

			return (button.Filter == "All" && string.IsNullOrEmpty(selectedTag)) ||
				   string.Equals(button.Filter, selectedTag, StringComparison.OrdinalIgnoreCase);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}