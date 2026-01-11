using System.Windows;
using System.Windows.Controls;
using Echoslate.Core.ViewModels;

namespace Echoslate.Windows;

public partial class TodoMultiItemEditorWindow : UserControl {
	public TodoMultiItemEditorWindow(TodoMultiItemEditorViewModel vm) {
		InitializeComponent();
		DataContext = vm;
	}
	private void Ok_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is TodoMultiItemEditorViewModel vm && Parent is Window window) {
			vm.OkCommand();
			window.DialogResult = true;
			window.Close();
		}
	}
	private void Cancel_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is TodoMultiItemEditorViewModel vm && Parent is Window window) {
			vm.CancelCommand();
			window.DialogResult = false;
			window.Close();
		}
	}
	private void Complete_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is TodoMultiItemEditorViewModel vm && Parent is Window window) {
			vm.CompleteCommand();
			window.DialogResult = true;
			window.Close();
		}
	}
}