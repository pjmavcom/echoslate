using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Echoslate.Behaviors
{
	public class ListBoxSelectedItemsBehavior : Behavior<ListBox> {
		public static readonly DependencyProperty SelectedItemsProperty =
			DependencyProperty.Register(
										nameof(SelectedItems),
										typeof(IList),
										typeof(ListBoxSelectedItemsBehavior),
										new PropertyMetadata(null, OnSelectedItemsChanged));

		public IList SelectedItems {
			get => (IList)GetValue(SelectedItemsProperty);
			set => SetValue(SelectedItemsProperty, value);
		}

		protected override void OnAttached() {
			base.OnAttached();
			AssociatedObject.SelectionChanged += OnListBoxSelectionChanged;
			if (SelectedItems != null) SyncSelectedItemsToListBox();
		}

		protected override void OnDetaching() {
			base.OnDetaching();
			AssociatedObject.SelectionChanged -= OnListBoxSelectionChanged;
			if (SelectedItems is INotifyCollectionChanged notifyCollection)
				notifyCollection.CollectionChanged -= OnSelectedItemsCollectionChanged;
		}

		private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is ListBoxSelectedItemsBehavior behavior) {
				if (e.OldValue is INotifyCollectionChanged oldCollection)
					oldCollection.CollectionChanged -= behavior.OnSelectedItemsCollectionChanged;

				if (e.NewValue is INotifyCollectionChanged newCollection)
					newCollection.CollectionChanged += behavior.OnSelectedItemsCollectionChanged;

				behavior.SyncSelectedItemsToListBox();
			}
		}

		private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			SyncSelectedItemsToListBox();
		}

		private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (SelectedItems == null) return;

			foreach (var item in e.AddedItems)
				if (!SelectedItems.Contains(item))
					SelectedItems.Add(item);

			foreach (var item in e.RemovedItems)
				if (SelectedItems.Contains(item))
					SelectedItems.Remove(item);
		}

		private void SyncSelectedItemsToListBox() {
			if (SelectedItems == null) return;

			AssociatedObject.SelectedItems.Clear();
			foreach (var item in SelectedItems)
				if (AssociatedObject.Items.Contains(item))
					AssociatedObject.SelectedItems.Add(item);
		}
	}
}