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
			AppServices.ApplicationService.GetMainWindowViewModel().CreateNewFile();
			AppServices.ApplicationService.Show();
			vm.SavePreferences();
			window.Close();
		}
	}
	private void OpenExisting_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is WelcomeViewModel vm && Parent is Window window) {
			if (AppServices.ApplicationService.GetMainWindowViewModel().OpenFile()) {
				AppServices.ApplicationService.Show();
				vm.SavePreferences();
				window.Close();
			}
		}
	}
	
}