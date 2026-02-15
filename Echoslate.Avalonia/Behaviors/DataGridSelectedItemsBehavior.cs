using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;

namespace Echoslate.Avalonia.Behaviors;

public class DataGridSelectedItemsBehavior : AvaloniaObject {
	public static readonly AttachedProperty<IList?> SyncSelectedItemsProperty =
		AvaloniaProperty.RegisterAttached<DataGridSelectedItemsBehavior, DataGrid, IList?>(
			"SyncSelectedItems");
	public static void SetSyncSelectedItems(DataGrid element, IList? value) {
		element.SetValue(SyncSelectedItemsProperty, value);
	}
	public static IList? GetSyncSelectedItems(DataGrid element) {
		return element.GetValue(SyncSelectedItemsProperty);
	}


	static DataGridSelectedItemsBehavior() {
		SyncSelectedItemsProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<IList?>>(OnSyncSelectedItemsChanged));
	}
	private static void OnSyncSelectedItemsChanged(AvaloniaPropertyChangedEventArgs<IList?> e) {
		if (e.Sender is not DataGrid dataGrid) {
			return;
		}

		dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
		dataGrid.SelectedItems.Clear();

		var newItems = e.NewValue.GetValueOrDefault();
		if (newItems == null) {
			return;
		}

		if (dataGrid.ItemsSource != null) {
			SyncToGrid(dataGrid, newItems);
			dataGrid.SelectionChanged += DataGrid_SelectionChanged;
			return;
		}

		void OnAttached(object? sender, VisualTreeAttachmentEventArgs args) {
			if (dataGrid.ItemsSource == null) {
				return;
			}
			dataGrid.AttachedToVisualTree -= OnAttached;
			SyncToGrid(dataGrid, newItems);
			dataGrid.SelectionChanged += DataGrid_SelectionChanged;
		}

		dataGrid.AttachedToVisualTree += OnAttached;
	}
	private static void SyncToGrid(DataGrid dataGrid, IList items) {
		foreach (var item in items) {
			if (!dataGrid.SelectedItems.Contains(item)) {
				dataGrid.SelectedItems.Add(item);
			}
		}
	}
	private static void SyncSelections(DataGrid dataGrid, IList newItems) {
		foreach (var item in newItems) {
			if (!dataGrid.SelectedItems.Contains(item)) {
				dataGrid.SelectedItems.Add(item);
			}
		}
		dataGrid.SelectionChanged += DataGrid_SelectionChanged;
	}
	private static void DataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
		if (sender is not DataGrid dataGrid) {
			return;
		}

		var collection = GetSyncSelectedItems(dataGrid);
		if (collection == null) {
			return;
		}

		foreach (var removed in e.RemovedItems) {
			if (collection.Contains(removed)) {
				collection.Remove(removed);
			}
		}

		foreach (var added in e.AddedItems) {
			if (!collection.Contains(added)) {
				collection.Add(added);
			}
		}
	}
}