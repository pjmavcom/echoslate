using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Windows;

public partial class OptionsWindow : UserControl {
	private OptionsViewModel _vm;
	
	public OptionsWindow(OptionsViewModel vm) {
		InitializeComponent();
		DataContext = vm;
		_vm = vm;
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
	}
	private void Cancel_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is OptionsViewModel vm && Parent is Window window) {
			window.Close(null);
		}
	}
	private void Ok_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is OptionsViewModel vm && Parent is Window window) {
			vm.Result = true;
			window.Close(_vm);
		}
	}
}
