using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Windows;

public partial class ReminderEditorWindow : UserControl {
	public ReminderEditorWindow() {
		InitializeComponent();
	}
	public ReminderEditorWindow(ReminderEditorViewModel vm) {
		InitializeComponent();
		DataContext = vm;
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
	}
	private void Cancel_Click(object? sender, RoutedEventArgs e) {
		if (DataContext is ReminderEditorViewModel vm && Parent is Window window) {
			window.Close(null);
		}
	}
	private void Save_OnClick(object? sender, RoutedEventArgs e) {
		if (DataContext is ReminderEditorViewModel vm && Parent is Window window) {
			vm.SaveChanges();
			window.Close(vm);
		}
	}
	private void Clear_Click(object? sender, RoutedEventArgs e) {
		if (DataContext is ReminderEditorViewModel vm && Parent is Window window) {
			vm.Clear();
			window.Close(vm);
		}
	}
	private void OnMinute_LostFocus(object? sender, RoutedEventArgs e) {
		if (sender is NumericUpDown nud && nud.Value.HasValue) {
			int snapped = (int)nud.Value / 15 * 15;
			nud.Value = snapped;
			if (DataContext is ReminderEditorViewModel vm) {
				vm.DueMinute = snapped;
			}
		}
	}
	private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
		
	}
	private void Cancel_OnClick(object? sender, RoutedEventArgs e) {
		if (Parent is Window window) {
			window.Close();
		}
	}
}