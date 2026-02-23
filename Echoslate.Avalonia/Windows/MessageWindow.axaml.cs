using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Windows;

public partial class MessageWindow : UserControl {
	public MessageWindow(MessageWindowViewModel vm) {
		InitializeComponent();
		DataContext = vm;
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
	}
	private void Ok_OnClick(object? sender, RoutedEventArgs e) {
		if (DataContext is MessageWindowViewModel vm && Parent is Window window) {
			vm.Result = DialogResult.Ok;
			window.Close(true);
		}
	}
	private void Yes_OnClick(object? sender, RoutedEventArgs e) {
		if (DataContext is MessageWindowViewModel vm && Parent is Window window) {
			vm.Result = DialogResult.Yes;
			window.Close(true);
		}
	}
	private void No_OnClick(object? sender, RoutedEventArgs e) {
		if (DataContext is MessageWindowViewModel vm && Parent is Window window) {
			vm.Result = DialogResult.No;
			window.Close(false);
		}
	}
	private void Cancel_OnClick(object? sender, RoutedEventArgs e) {
		if (DataContext is MessageWindowViewModel vm && Parent is Window window) {
			vm.Result = DialogResult.Cancel;
			window.Close(false);
		}
	}
}