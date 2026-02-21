using System.Windows;
using System.Windows.Controls;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;

namespace Echoslate.Wpf.Windows;

public partial class WelcomeWindow : UserControl {
	public WelcomeWindow(WelcomeViewModel vm) {
		InitializeComponent();
		DataContext = vm;
	}
	private void CreateNew_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is WelcomeViewModel vm && Parent is Window window) {
			AppServices.MainWindowVM.CreateNewFile();
			vm.SavePreferences();
			window.DialogResult = true;
			window.Close();
		}
	}
	private async void OpenExisting_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is WelcomeViewModel vm && Parent is Window window) {
			var result = await AppServices.MainWindowVM.OpenFile();
			if (result) {
				vm.SavePreferences();
				window.DialogResult = true;
				window.Close();
			}
		}
	}
}