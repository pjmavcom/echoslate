using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Echoslate.Core.ViewModels;

namespace Echoslate.Wpf.Windows;

public partial class TagPickerWindow : UserControl, INotifyPropertyChanged {
	public TagPickerWindow(TagPickerViewModel vm) {
		InitializeComponent();
		DataContext = vm;
		Loaded += OnLoaded;
	}
	private void OnLoaded(object sender, RoutedEventArgs e) {
		if (DataContext is TagPickerViewModel vm) {
			foreach (string tag in vm.SelectedTags) {
				lbTags.SelectedItems.Add(tag);
			}
		}
		tbNewTag.Focus();
	}
	private void Ok_OnClick(object sender, RoutedEventArgs e) {
		OkCommand();
	}
	private void OkCommand() {
		if (DataContext is TagPickerViewModel vm && Parent is Window window) {
			vm.Result = true;
			window.DialogResult = true;
			window.Close();
		}
	}
	private void Cancel_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is TagPickerViewModel vm && Parent is Window window) {
			vm.Result = false;
			window.DialogResult = false;
			window.Close();
		}
	}
	private void NewTag_OnClick(object sender, RoutedEventArgs e) {
		if (DataContext is TagPickerViewModel vm) {
			vm.NewTag();
			var list = vm.SelectedTags.ToList();
			lbTags.SelectedItems.Clear();
			foreach (string tag in list) {
				lbTags.SelectedItems.Add(tag);
			}
		}
	}
	private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
		if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control) {
			OkCommand();
			e.Handled = true;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}