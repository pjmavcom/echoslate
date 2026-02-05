using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace Echoslate.Avalonia.Behaviors;

public static class ListBoxSelectedItemsBehavior {
	// Register ON ListBox so Changed handlers get a ListBox sender and this is strongly scoped.
	public static readonly AttachedProperty<IList?> SelectedItemsProperty =
		AvaloniaProperty.RegisterAttached<ListBox, IList?>(
			"SelectedItems",
			typeof(ListBoxSelectedItemsBehavior),
			defaultValue: null,
			defaultBindingMode: BindingMode.TwoWay);

	public static IList? GetSelectedItems(AvaloniaObject obj) => obj.GetValue(SelectedItemsProperty);
	public static void SetSelectedItems(AvaloniaObject obj, IList? value) => obj.SetValue(SelectedItemsProperty, value);

	private sealed class State {
		public IList? VmList;
		public INotifyCollectionChanged? VmNotify;
		public bool IsUpdating;
	}

	private static readonly ConditionalWeakTable<ListBox, State> States = new();

	static ListBoxSelectedItemsBehavior() {
		// This ensures the handler runs for each ListBox when the attached property changes.
		SelectedItemsProperty.Changed.AddClassHandler<ListBox>(OnSelectedItemsChanged);
	}

	private static void OnSelectedItemsChanged(ListBox listBox, AvaloniaPropertyChangedEventArgs e) {
		var state = States.GetOrCreateValue(listBox);

		// Detach old VM list notifications
		if (state.VmNotify is not null)
			state.VmNotify.CollectionChanged -= (_, args) => { }; // placeholder; we subscribe with a stored handler below

		// We need stable delegates to unsubscribe correctly
		// so we keep them as local functions that close over (listBox, state).
		void VmCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args) {
			if (state.IsUpdating) return;
			if (state.VmList is null) return;

			state.IsUpdating = true;
			try {
				SyncFromVmToListBox(listBox, state.VmList);
			}
			finally {
				state.IsUpdating = false;
			}
		}

		void ListBoxSelectionChanged(object? sender, SelectionChangedEventArgs args) {
			if (state.IsUpdating) return;
			if (state.VmList is null) return;

			state.IsUpdating = true;
			try {
				// ListBox -> VM
				foreach (var removed in args.RemovedItems)
					state.VmList.Remove(removed);

				foreach (var added in args.AddedItems) {
					if (!state.VmList.Contains(added))
						state.VmList.Add(added);
				}
			}
			finally {
				state.IsUpdating = false;
			}
		}

		// Remove any previous subscriptions by resetting the ListBox handler (safe + simple)
		listBox.SelectionChanged -= ListBoxSelectionChanged;
		listBox.SelectionChanged += ListBoxSelectionChanged;

		state.VmList = e.NewValue as IList;

		state.VmNotify = state.VmList as INotifyCollectionChanged;
		if (state.VmNotify is not null)
			state.VmNotify.CollectionChanged += VmCollectionChanged;

		// Initial sync VM -> ListBox
		if (state.VmList is not null) {
			state.IsUpdating = true;
			try {
				SyncFromVmToListBox(listBox, state.VmList);
			}
			finally {
				state.IsUpdating = false;
			}
		}
	}

	private static void SyncFromVmToListBox(ListBox listBox, IList vmList) {
		// Note: ListBox.SelectedItems exists for multi-select. Ensure SelectionMode supports it.
		listBox.SelectedItems.Clear();
		foreach (var item in vmList) {
			if (item is not null && !listBox.SelectedItems.Contains(item))
				listBox.SelectedItems.Add(item);
		}
	}
}