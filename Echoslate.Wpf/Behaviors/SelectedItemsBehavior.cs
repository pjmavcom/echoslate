using System.Collections;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Echoslate.Wpf.Behaviors;

public class SelectedItemsBehavior : Behavior<ListBox> {
	public static readonly DependencyProperty SelectedItemsProperty =
		DependencyProperty.Register(nameof(SelectedItems),
			typeof(IList),
			typeof(SelectedItemsBehavior),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	public IList SelectedItems {
		get => (IList)GetValue(SelectedItemsProperty);
		set => SetValue(SelectedItemsProperty, value);
	}
	protected override void OnAttached() {
		base.OnAttached();
		AssociatedObject.SelectionChanged += OnSelectionChanged;
	}
	protected override void OnDetaching() {
		AssociatedObject.SelectionChanged -= OnSelectionChanged;
		base.OnDetaching();
	}
	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
		if (SelectedItems == null) {
			return;
		}

		foreach (var item in e.AddedItems) {
			if (!SelectedItems.Contains(item)) {
				SelectedItems.Add(item);
			}
		}

		foreach (var item in e.RemovedItems) {
			SelectedItems.Remove(item);
		}
	}
}