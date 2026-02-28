using System.Windows;
using System.Windows.Controls;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;

namespace Echoslate.Windows;

public partial class MessageWindow : UserControl {
	public MessageWindow(MessageWindowViewModel vm) {
		InitializeComponent();
		DataContext = vm;
	}
	private void Ok_OnClick(object? sender, RoutedEventArgs e) {
		if (DataContext is MessageWindowViewModel vm && Parent is Window window) {
			vm.Result = DialogResult.Ok;
			window.Close();
		}
	}
	private void Yes_OnClick(object? sender, RoutedEventArgs e) {
		if (DataContext is MessageWindowViewModel vm && Parent is Window window) {
			vm.Result = DialogResult.Yes;
			window.Close();
		}
	}
	private void No_OnClick(object? sender, RoutedEventArgs e) {
		if (DataContext is MessageWindowViewModel vm && Parent is Window window) {
			vm.Result = DialogResult.No;
			window.Close();
		}
	}
	private void Cancel_OnClick(object? sender, RoutedEventArgs e) {
		if (DataContext is MessageWindowViewModel vm && Parent is Window window) {
			vm.Result = DialogResult.Cancel;
			window.Close();
		}
	}
}