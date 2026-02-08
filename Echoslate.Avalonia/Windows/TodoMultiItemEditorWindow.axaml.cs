using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Windows;

public partial class TodoMultiItemEditorWindow : UserControl {
	public TodoMultiItemEditorWindow(TodoMultiItemEditorViewModel vm) {
		InitializeComponent();
		DataContext = vm;
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
	}
	private void Ok_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is TodoMultiItemEditorViewModel vm && Parent is Window window) {
			vm.OkCommand();
			window.Close(vm);
		}
	}
	private void Cancel_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is TodoMultiItemEditorViewModel vm && Parent is Window window) {
			vm.CancelCommand();
			window.Close(vm);
		}
	}
	private void Complete_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is TodoMultiItemEditorViewModel vm && Parent is Window window) {
			vm.CompleteCommand();
			window.Close(vm);
		}
	}
}