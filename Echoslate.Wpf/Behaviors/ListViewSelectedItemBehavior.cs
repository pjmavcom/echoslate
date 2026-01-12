using System.Windows;
using System.Windows.Controls;

namespace Echoslate.Wpf.Behaviors;

public static class ListViewSelectedItemBehavior {
	public static readonly DependencyProperty SyncSelectedItemProperty =
		DependencyProperty.RegisterAttached(
			"SyncSelectedItem",
			typeof(object),
			typeof(ListViewSelectedItemBehavior),
			new PropertyMetadata(null, OnSyncSelectedItemChanged));

	public static void SetSyncSelectedItem(DependencyObject element, object value) {
		element.SetValue(SyncSelectedItemProperty, value);
	}

	public static object GetSyncSelectedItem(DependencyObject element) {
		return element.GetValue(SyncSelectedItemProperty);
	}

	private static void OnSyncSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is ListView listView) {
			if (e.NewValue != listView.SelectedItem) {
				listView.SelectedItem = e.NewValue;
			}

			listView.SelectionChanged += (s, args) => {
				var selected = listView.SelectedItem;
				var bound = GetSyncSelectedItem(listView);

				if (!object.Equals(selected, bound)) {
					SetSyncSelectedItem(listView, selected);
				}
			};
		}
	}
}