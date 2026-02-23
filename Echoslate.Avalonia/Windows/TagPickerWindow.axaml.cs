using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Windows;

public partial class TagPickerWindow : UserControl, INotifyPropertyChanged {
	public TagPickerWindow(TagPickerViewModel vm) {
		InitializeComponent();
		DataContext = vm;
		Loaded += OnLoaded;
		AttachedToVisualTree += (s, e) => {
			var tb = this.FindControl<TextBox>("tbNewTag");
			tb?.Focus();
		};
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
	}
	private void OnLoaded(object? sender, RoutedEventArgs e) {
		var lbTags = this.FindControl<ListBox>("lbTags");
		if (DataContext is TagPickerViewModel vm) {
			foreach (string tag in vm.SelectedTags) {
				lbTags.SelectedItems.Add(tag);
			}
		}
		var tb = this.FindControl<TextBox>("tbNewTag");
		tb?.Focus();
	}
	private void Ok_OnClick(object sender, RoutedEventArgs e) {
		OkCommand();
	}
	public void OkCommand() {
		if (DataContext is TagPickerViewModel vm && Parent is Window window) {
			vm.Result = true;
			window.Close(vm);
		}
	}
	private void Cancel_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is TagPickerViewModel vm && Parent is Window window) {
			vm.Result = false;
			window.Close(null);
		}
	}
	private void NewTag_OnClick(object sender, RoutedEventArgs e) {
		var lbTags = this.FindControl<ListBox>("lbTags");
		if (DataContext is TagPickerViewModel vm) {
			vm.NewTag();
			var list = vm.SelectedTags.ToList();
			lbTags.SelectedItems.Clear();
			foreach (string tag in list) {
				lbTags.SelectedItems.Add(tag);
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}