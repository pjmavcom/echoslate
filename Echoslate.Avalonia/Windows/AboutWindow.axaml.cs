using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Echoslate.Avalonia.Windows;

public partial class AboutWindow : UserControl {
	public AboutWindow() {
		InitializeComponent();
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
	}
	private async void KoFiLink_PointerPressed(object? sender, PointerPressedEventArgs e) {
		try {
			var uri = new Uri("https://ko-fi.com/pjmavcom");

			var topLevel = TopLevel.GetTopLevel(this);
			if (topLevel != null) {
				await topLevel.Launcher.LaunchUriAsync(uri);
			}
		} catch (Exception ex) {
			Console.WriteLine($"Failed to open Ko-fi: {ex.Message}");
		}
	}
	private async void GitHubLink_PointerPressed(object? sender, PointerPressedEventArgs e) {
		try {
			var uri = new Uri("https://github.com/pjmavcom/echoslate");

			var topLevel = TopLevel.GetTopLevel(this);
			if (topLevel != null) {
				await topLevel.Launcher.LaunchUriAsync(uri);
			}
		} catch (Exception ex) {
			Console.WriteLine($"Failed to open GitHub: {ex.Message}");
		}
	}
	
	
}