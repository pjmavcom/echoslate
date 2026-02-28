using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Echoslate.Core.Models;

namespace Echoslate.Avalonia.Windows;

public partial class AboutWindow : UserControl, INotifyPropertyChanged {
	private string _version;
	public string Version {
		get => _version;
		set {
			if (_version == value) {
				return;
			}
			_version = value;
			OnPropertyChanged();
		}
	}
	
	public AboutWindow(string version) {
		DataContext = this;
		InitializeComponent();
		Version = version;
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
		OnPropertyChanged(nameof(Version));
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
	

	public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
		if (EqualityComparer<T>.Default.Equals(field, value)) {
			return false;
		}
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	private void Cancel_OnClick(object? sender, RoutedEventArgs e) {
		if (Parent is Window window) {
			window.Close();
		}
	}
}