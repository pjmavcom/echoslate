using System;
using System.Windows;
using System.Windows.Controls;
using Echoslate.Core.ViewModels;

namespace Echoslate.Wpf.Windows;

public partial class TodoItemEditorWindow : UserControl {
	public TodoItemEditorWindow(TodoItemEditorViewModel vm) {
		InitializeComponent();
		DataContext = vm;
	}
	public void Ok_OnClick(Object sender, RoutedEventArgs e) {
		if (DataContext is TodoItemEditorViewModel vm && Parent is Window window) {
			vm.OkCommand();
			window.DialogResult = true;
			window.Close();
		}
	}
	public void Complete_OnClick(Object sender, RoutedEventArgs e) {
		if (DataContext is TodoItemEditorViewModel vm && Parent is Window window) {
			vm.CompleteCommand();
			window.DialogResult = true;
			window.Close();
		}
	}
	public void Cancel_OnClick(Object sender, RoutedEventArgs e) {
		if (DataContext is TodoItemEditorViewModel vm && Parent is Window window) {
			vm.Result = false;
			window.DialogResult = false;
			window.Close();
		}
	}
}