using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Windows;

public partial class TodoItemEditorWindow : UserControl {
	public TodoItemEditorWindow(TodoItemEditorViewModel vm) {
		InitializeComponent();
		DataContext = vm;
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
	}

	public void Ok_OnClick(Object sender, RoutedEventArgs e) {
		if (DataContext is TodoItemEditorViewModel vm && Parent is Window window) {
			vm.OkCommand();
			window.Close(vm);
		}
	}
	public void Complete_OnClick(Object sender, RoutedEventArgs e) {
		if (DataContext is TodoItemEditorViewModel vm && Parent is Window window) {
			vm.CompleteCommand();
			window.Close(vm);
		}
	}
	public void Cancel_OnClick(Object sender, RoutedEventArgs e) {
		if (DataContext is TodoItemEditorViewModel vm && Parent is Window window) {
			vm.Result = false;
			window.Close(null);
		}
	}
}