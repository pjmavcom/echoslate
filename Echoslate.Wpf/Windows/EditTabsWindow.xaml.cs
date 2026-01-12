using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Echoslate.Core.ViewModels;


namespace Echoslate.Wpf.Windows;

public partial class EditTabsWindow : UserControl {
	private ListBox lb;

	public EditTabsWindow(EditTabsViewModel vm) {
		InitializeComponent();
		DataContext = vm;
		lb = lbTabs;
	}
	private void OK_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is EditTabsViewModel vm && Parent is Window window) {
			vm.OnOk();
			window.DialogResult = true;
			window.Close();
		}
	}
	private void ReSelectItems(IEnumerable<string> itemsToSelect) {
		lb.SelectedItems.Clear();
		foreach (string s in itemsToSelect) {
			lb.SelectedItems.Add(s);
		}
	}
	private void Cancel_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is OptionsViewModel vm && Parent is Window window) {
			vm.Result = false;
			window.DialogResult = false;
			window.Close();
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