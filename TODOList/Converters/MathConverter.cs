using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Echoslate.Converters {
	public class MathConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is double doubleValue && parameter is string expression) {
				// Split by spaces: "/3 -60" → ["/3", "-60"]
				var parts = expression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				double result = doubleValue;

				foreach (var part in parts) {
					if (string.IsNullOrWhiteSpace(part)) continue;

					char op = part[0];
					string numStr = part.Substring(1);

					if (!double.TryParse(numStr, out double operand))
						return result; // skip invalid

					switch (op) {
						case '+': result += operand; break;
						case '-': result -= operand; break;
						case '*': result *= operand; break;
						case '/': result = operand != 0 ? result / operand : result; break;
					}
				}

				return Math.Max(0, result); // prevent negative widths
			}

			return value; // fallback
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}