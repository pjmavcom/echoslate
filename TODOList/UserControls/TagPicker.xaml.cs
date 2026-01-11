using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Echoslate.Core.ViewModels;

namespace Echoslate {
	public partial class TagPicker : UserControl, INotifyPropertyChanged {
		public TagPicker(TagPickerViewModel vm) {
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
		}
		private void Ok_OnClick(object sender, RoutedEventArgs e) {
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

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}