/*	ProgressForegroundConverter.cs
 * 10-Feb-2019
 * 13:17:36
 *
 * 
 */

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Echoslate
{
	public class ProgressBarForegroundConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Color foreground = new Color();
			if (value == null)
				return new SolidColorBrush(foreground);
			
			double progress = (double) value;

			if (progress >= 3d)
				foreground = Colors.Red;
			else if (progress >= 2d)
				foreground = Colors.Yellow;
			else if (progress >= 1d)
				foreground = Colors.Green;
			return new SolidColorBrush(foreground);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
