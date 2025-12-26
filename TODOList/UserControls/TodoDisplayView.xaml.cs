using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using Echoslate.ViewModels;

namespace Echoslate.UserControls {
	public partial class TodoDisplayView : UserControl {
		public TodoDisplayView() {
			InitializeComponent();
		}
		private void Todos_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			foreach (TodoItemHolder ih in e.RemovedItems.OfType<TodoItemHolder>()) {
				ih.CleanNotes();
			}
		}

		private void TodoListView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
			if (e.NewValue is true && DataContext is TodoDisplayViewModelBase vm) {
				vm.RefreshAll();
			}
		}
		private void NotesPanelCompleteRequested(object sender, RoutedEventArgs e) {
			TodoDisplayViewModelBase? vm = (TodoDisplayViewModelBase)DataContext;
			if (vm == null) {
				Log.Print("Can not find ViewModel.");
				return;
			}

			vm.MarkSelectedItemAsComplete();
		}
		private void NotesPanelEditTagsRequested(object sender, RoutedEventArgs e) {
			TodoDisplayViewModelBase? vm = (TodoDisplayViewModelBase)DataContext;
			if (vm == null) {
				Log.Print("Can not find ViewModel.");
				return;
			}
			if (vm.SelectedTodoItems.Count == 0 || vm.SelectedTodoItems[0] == null) {
				Log.Print("No todos selected.");
				return;
			}

			List<TodoItem> ihs = [];
			List<string> selectedTags = [];
			foreach (TodoItemHolder ih in vm.SelectedTodoItems) {
				ihs.Add(ih.TD);
			}

			selectedTags = new List<string>(ihs.Select(x => x.Tags ?? Enumerable.Empty<string>()).Aggregate((a, b) => a.Intersect(b).ToList()));
			TagPicker dlg = new TagPicker {
				SelectedTodoItems = ihs,
				AllAvailableTags = vm.AllTags,
				SelectedTags = new List<string>(selectedTags),
				Owner = Window.GetWindow(this)
			};
			dlg.ShowDialog();
			if (dlg.Result) {
				foreach (TodoItem item in ihs) {
					foreach (string tag in selectedTags) {
						item.Tags.Remove(tag);
					}
					foreach (string tag in dlg.SelectedTags) {
						item.AddTag(tag);
					}
				}
				vm.RefreshAll();
			}
		}
		public ICommand RefreshAllCommand => new RelayCommand(() => {
			TodoDisplayViewModelBase? vm = (TodoDisplayViewModelBase)DataContext;
			vm.RefreshAll();
		});

		private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			if (sender is ListBoxItem lbItem) {
				if (lbItem.DataContext is TodoItemHolder ih) {
					TodoItem item = ih.TD;
					if (DataContext is TodoDisplayViewModelBase vm) {
						vm.EditItem(item);
					}
				}
			}
		}
		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			// Let your InputBindings handle it first
			// If they don't handle it, you can add custom logic here if needed
			// But usually, just marking as handled=false isn't needed with InputBindings
		}
	}
}