using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Windows;

public partial class WelcomeWindow : UserControl {
	public bool Result;
	
	public WelcomeWindow(WelcomeViewModel vm) {
		InitializeComponent();
		DataContext = vm;
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
	}
	
	private void CreateNew_OnClick(object sender, RoutedEventArgs e) {
		Log.Print("Creating new file...");
		if (DataContext is WelcomeViewModel vm && Parent is Window window) {
			AppServices.MainWindowVM.CreateNewFile();

			Log.Print("Saving WelcomeWindow preferences and closing...");
			vm.SavePreferences();
			Result = true;
			window.Close(true);
		}
	}
	private void OpenExisting_OnClick(object sender, RoutedEventArgs e) {
		Log.Print("Opening existing file...");
		if (DataContext is WelcomeViewModel vm && Parent is Window window) {
			if (AppServices.MainWindowVM.OpenFile()) {
				
				Log.Print("Saving WelcomeWindow preferences and closing...");
				vm.SavePreferences();
				Result = true;
				window.Close(true);
			}
		}
	}
}