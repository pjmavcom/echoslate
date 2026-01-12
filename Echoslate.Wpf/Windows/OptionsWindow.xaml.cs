using System.Windows;
using System.Windows.Controls;
using Echoslate.Core.ViewModels;


namespace Echoslate.Wpf.Windows;

public partial class OptionsWindow : UserControl {
	public OptionsWindow(OptionsViewModel vm) {
		InitializeComponent();
		DataContext = vm;
	}
	private void Cancel_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is OptionsViewModel vm && Parent is Window window) {
			vm.Result = false;
			window.DialogResult = true;
			window.Close();
		}
	}
	private void Ok_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is OptionsViewModel vm && Parent is Window window) {
			vm.Result = true;
			window.DialogResult = true;
			window.Close();
		}
	}
}