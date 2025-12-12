using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Echoslate.Converters {
	public class TagIsSelectedConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if (values.Length < 2 || values[0] is not string tag || values[1] is not ICollection<string> selected) {
				return false;
			}

			return selected.Contains(tag);
		}
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			return null;
		}
	}
}