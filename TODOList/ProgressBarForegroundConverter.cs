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


namespace TODOList
{
	public class ProgressBarForegroundConverter : IValueConverter
	{
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //


		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //


		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double progress = (double) value;
			Color foreground = new Color();
			

			if (progress >= 3d)
			{
				foreground = Colors.Red;
			}
			else if (progress >= 2d)
			{
				foreground = Colors.Yellow;
			}
			else if (progress >= 1d)
			{
				foreground = Colors.Green;
			}
			return foreground;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		// MONOGAME METHODS //////////////////////////////////////////////////////////////////////////////////////////////////////////////// MONOGAME METHODS //


		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS //


	}
}