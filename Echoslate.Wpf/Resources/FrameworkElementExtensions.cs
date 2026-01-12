using System.Windows;
using System.Windows.Media;

namespace Echoslate.Wpf.Resources;

public static class FrameworkElementExtensions {
	public static T? TryFindParent<T>(this DependencyObject child) where T : DependencyObject {
		var parent = VisualTreeHelper.GetParent(child);
		while (parent != null && parent is not T) {
			parent = VisualTreeHelper.GetParent(parent);
		}
		return parent as T;
	}
}