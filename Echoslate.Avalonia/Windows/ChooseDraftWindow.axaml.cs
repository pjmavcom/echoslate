using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Windows;

public partial class ChooseDraftWindow : UserControl {
	public ChooseDraftWindow(ChooseDraftViewModel vm) {
		InitializeComponent();
		DataContext = vm;
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
	}
	private void Ok_Click(object sender, RoutedEventArgs e) {
		if (DataContext is ChooseDraftViewModel vm && Parent is Window window) {
			vm.SetResult();
			window.Close(vm);
		}
	}
	private void Cancel_Click(object sender, RoutedEventArgs e) {
		if (DataContext is ChooseDraftViewModel vm && Parent is Window window) {
			vm.Result = false;
			window.Close(null);
		}
	}
}