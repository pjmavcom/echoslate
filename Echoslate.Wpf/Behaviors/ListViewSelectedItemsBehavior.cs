using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Echoslate.Wpf.Behaviors;

public static class ListViewSelectedItemsBehavior {
	public static readonly DependencyProperty SyncSelectedItemsProperty =
		DependencyProperty.RegisterAttached(
			"SyncSelectedItems",
			typeof(IList),
			typeof(ListViewSelectedItemsBehavior),
			new PropertyMetadata(null, OnSyncSelectedItemsChanged));

	public static void SetSyncSelectedItems(DependencyObject element, IList value) {
		element.SetValue(SyncSelectedItemsProperty, value);
	}

	public static IList GetSyncSelectedItems(DependencyObject element) {
		return (IList)element.GetValue(SyncSelectedItemsProperty);
	}

	private static void OnSyncSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is ListView listView) {
			listView.SelectedItems.Clear();

			if (e.NewValue is IList newItems) {
				foreach (var item in newItems) {
					listView.SelectedItems.Add(item);
				}
			}

			listView.SelectionChanged += (s, args) => {
				var collection = GetSyncSelectedItems(listView);
				if (collection == null) {
					return;
				}

				foreach (var removed in args.RemovedItems) {
					collection.Remove(removed);
				}

				foreach (var added in args.AddedItems) {
					collection.Add(added);
				}
			};
		}
	}
}