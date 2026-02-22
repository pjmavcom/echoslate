using System;
using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.VisualTree;
using Avalonia.Controls;
using Avalonia.Reactive;

namespace Echoslate.Avalonia.Behaviors;

public class DataGridSelectedItemsBehavior : AvaloniaObject {
	public static readonly AttachedProperty<IList?> SyncSelectedItemsProperty =
		AvaloniaProperty.RegisterAttached<DataGridSelectedItemsBehavior, DataGrid, IList?>("SyncSelectedItems");
	public static void SetSyncSelectedItems(DataGrid element, IList? value)
		=> element.SetValue(SyncSelectedItemsProperty, value);
	public static IList? GetSyncSelectedItems(DataGrid element)
		=> element.GetValue(SyncSelectedItemsProperty);

	static DataGridSelectedItemsBehavior() {
		SyncSelectedItemsProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<IList?>>(OnSyncSelectedItemsChanged));
	}
	private static void OnSyncSelectedItemsChanged(AvaloniaPropertyChangedEventArgs<IList?> e) {
		if (e.Sender is not DataGrid dataGrid)
			return;

		dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
		dataGrid.DetachedFromVisualTree -= OnDetachedFromVisualTree;
		dataGrid.AttachedToVisualTree -= OnAttachedToVisualTree;

		dataGrid.SelectedItems.Clear();

		var newItems = e.NewValue.GetValueOrDefault();
		if (newItems == null)
			return;

		if (dataGrid.IsAttachedToVisualTree()) {
			SyncToGrid(dataGrid, newItems);
			dataGrid.SelectionChanged += DataGrid_SelectionChanged;
		} else {
			dataGrid.AttachedToVisualTree += OnAttachedToVisualTree;
		}

		dataGrid.DetachedFromVisualTree += OnDetachedFromVisualTree;
	}
	private static void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs args) {
		if (sender is not DataGrid dataGrid)
			return;

		var items = GetSyncSelectedItems(dataGrid);
		if (items == null)
			return;

		// If ItemsSource already ready → sync now
		var src = dataGrid.ItemsSource as IList;
		if (dataGrid.ItemsSource != null && src.Count > 0) {
			SyncToGrid(dataGrid, items);
			dataGrid.SelectionChanged += DataGrid_SelectionChanged;
			return;
		}

		IDisposable? subscription = null;

		// Otherwise wait for ItemsSource to be set
		void OnItemsSourceChanged(IEnumerable? newSource) {
			if (newSource != null && src.Count > 0) {
				SyncToGrid(dataGrid, items);
				dataGrid.SelectionChanged += DataGrid_SelectionChanged;
				subscription?.Dispose(); // stop listening once synced
			}
		}

		subscription = dataGrid.GetObservable(DataGrid.ItemsSourceProperty).Subscribe(new AnonymousObserver<IEnumerable?>(OnItemsSourceChanged));
	}

	private static void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs args) {
		if (sender is not DataGrid dataGrid)
			return;

		dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
	}

	private static void SyncToGrid(DataGrid dataGrid, IList items) {
		var source = dataGrid.ItemsSource;
		if (source == null) {
			return;
		}

		var sourceSet = source is null
			? null
			: source.Cast<object>().ToHashSet();

		foreach (var item in items) {
			if (item is null)
				continue;

			if (sourceSet != null && !sourceSet.Contains(item))
				continue;

			if (!dataGrid.SelectedItems.Contains(item)) {
				dataGrid.SelectedItems.Add(item); // won't throw now
			}
		}
	}

	private static void DataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
		if (sender is not DataGrid dataGrid)
			return;

		var collection = GetSyncSelectedItems(dataGrid);
		if (collection == null)
			return;

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