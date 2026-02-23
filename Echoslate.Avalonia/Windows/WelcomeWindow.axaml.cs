using System;
using System.Threading.Tasks;
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
			vm.Close(true);
			window.Close(true);
		}
	}
	private async void OpenExisting_OnClick(object sender, RoutedEventArgs e) {
		Log.Print("Opening existing file...");
		await OpenExisting_OnClickAsync(sender, e);
	}
	private async Task OpenExisting_OnClickAsync(object? sender, RoutedEventArgs e) {
		if (DataContext is WelcomeViewModel vm && Parent is Window window) {
			try {
				bool opened = await AppServices.MainWindowVM.OpenFileAsync();

				if (opened) {
					Log.Print("Saving WelcomeWindow preferences and closing...");
					vm.SavePreferences();
					Result = true;
					vm.Close(true);
					window.Close(true);
				}
			} catch (Exception ex) {
				Log.Error("OpenFile() crashed!");
				Log.Error(ex.ToString());
			}
		} else {
			Log.Error("Cannot find VM or Window");
		}
	}
}