using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Windows;

public partial class AlarmPopupWindow : UserControl {
	public AlarmPopupWindow() {
		InitializeComponent();
	}
	public AlarmPopupWindow(AlarmPopupViewModel vm) {
		InitializeComponent();
		DataContext = vm;
		
		vm.RequestClose += (s, e) => Close();
	}
	public void Close() {
		if (DataContext is AlarmPopupViewModel vm && Parent is Window window) {
			window.Close(vm);
		}
	}

	private void Close_OnClick(object? sender, RoutedEventArgs e) {
		if (DataContext is AlarmPopupViewModel vm && Parent is Window window) {
			vm.Result = true;
			window.Close(vm);
		}
	}
	private void DismissAll_OnClick(object? sender, RoutedEventArgs e) {
		if (DataContext is AlarmPopupViewModel vm && Parent is Window window) {
			vm.DismissAll();
			window.Close(vm);
		}
	}
	private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
		if (DataContext is AlarmPopupViewModel vm) {
			vm.SelectionChanged();
		}
	}
	private void SnoozeAll_OnClick(object? sender, RoutedEventArgs e) {
		
	}
}