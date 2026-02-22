using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Echoslate.Core.Models;

namespace Echoslate.Avalonia.Windows;

public partial class HelpWindow : UserControl {
	public List<HotkeyItem> HotkeyItems { get; }

	public HelpWindow() {
		InitializeComponent();

		HotkeyItems = [
			new HotkeyItem("Enter", "Add new Todo item"),
			new HotkeyItem("Ctrl + Enter", "Quick complete selected Todo item"),
			new HotkeyItem("Alt + H", "Previous tab"),
			new HotkeyItem("Alt + L", "Next tab"),
			new HotkeyItem("Alt + J", "Decrease severity (quick-add)"),
			new HotkeyItem("Alt + K", "Increase severity (quick-add)"),
			new HotkeyItem("Ctrl + S", "Quick save"),
			new HotkeyItem("Ctrl + L", "Quick load previous file"),
			new HotkeyItem("Ctrl + P", "Toggle Pomodoro/work timer")
		];
		DataContext = this;
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
	}
	private void Cancel_OnClick(object? sender, RoutedEventArgs e) {
		if (Parent is Window window) {
			window.Close();
		}
	}

}