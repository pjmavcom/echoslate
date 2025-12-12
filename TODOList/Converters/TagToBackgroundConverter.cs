using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Echoslate.ViewModels;

namespace Echoslate;

public class TagToBackgroundConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		var vm = (Application.Current.MainWindow?.DataContext as TodoListViewModel);
		var tag = value?.ToString();
		bool isSelected = (tag == "All" && vm?.CurrentFilter == null) || tag == vm?.CurrentFilter;
		return isSelected
				   ? new SolidColorBrush(Color.FromRgb(0, 120, 215))
				   : new SolidColorBrush(Color.FromRgb(70, 70, 70));
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}