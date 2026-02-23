using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Windows;

public partial class EditTabsWindow : UserControl {
	public EditTabsWindow(EditTabsViewModel vm) {
		InitializeComponent();
		DataContext = vm;
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
	}

	private void OK_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is EditTabsViewModel vm && Parent is Window window) {
			vm.OnOk();
			window.Close(vm);
		}
	}
	private void ReSelectItems(IEnumerable<string> itemsToSelect) {
		var lb = this.FindControl<ListBox>("lbTabs");
		lb.SelectedItems.Clear();
		foreach (string s in itemsToSelect) {
			lb.SelectedItems.Add(s);
		}
	}
	private void Cancel_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is EditTabsViewModel vm && Parent is Window window) {
			vm.Result = false;
			window.Close(null);
		}
	}
	private void MoveUp_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is EditTabsViewModel vm) {
			ReSelectItems(vm.MoveSelectedItemsUp());
		}
	}
	private void MoveDown_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is EditTabsViewModel vm) {
			ReSelectItems(vm.MoveSelectedItemsDown());
		}
	}
}