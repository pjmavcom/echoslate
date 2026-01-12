using System.Windows;
using System.Windows.Controls;
using Echoslate.Core.ViewModels;

namespace Echoslate.Wpf.Windows;

public partial class ChooseDraftWindow : UserControl {
	public ChooseDraftWindow(ChooseDraftViewModel vm) {
		InitializeComponent();
		DataContext = vm;
	}
	private void Ok_Click(object sender, RoutedEventArgs e) {
		if (DataContext is ChooseDraftViewModel vm && Parent is Window window) {
			vm.SetResult();
			window.DialogResult = true;
			window.Close();
		}
	}
	private void Cancel_Click(object sender, RoutedEventArgs e) {
		if (DataContext is ChooseDraftViewModel vm && Parent is Window window) {
			vm.Result = false;
			window.DialogResult = false;
			window.Close();
		}
	}
}