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
using Echoslate.ViewModels;

namespace Echoslate.UserControls {
	public partial class TodoListView : UserControl {
		
		
		public TodoListView() {
			InitializeComponent();
			Loaded += TodoListView_OnLoaded;
			lbTodos.SelectionChanged += Todos_OnSelectionChanged;
		}
		private void TodoListView_OnLoaded(object sender, RoutedEventArgs e) {
			// This helps it load the first time just a bit faster
			var dummy = new TodoItem(); 
			var tempList = new List<TodoItem> { dummy };
			lbTodos.ItemsSource = tempList;
			Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
			if (DataContext is TodoListViewModel vm) {
				lbTodos.ItemsSource = vm.DisplayedItems;
			}
		}
		private void Todos_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			foreach (TodoItemHolder ih in e.RemovedItems.OfType<TodoItemHolder>()) {
				ih.CleanNotes();
			}
		}

		private void TodoListView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
			if (e.NewValue is true && DataContext is TodoListViewModel vm) {
				vm.RefreshAll();	
				vm.lbTodos = lbTodos;
			}
		}
		private void NotesPanelCompleteRequested(object sender, RoutedEventArgs e) {
			TodoListViewModel? vm = (TodoListViewModel)DataContext;
			if (vm == null) {
				Log.Print("Can not find ViewModel.");
				return;
			}
			if (vm.lbTodos.SelectedItem == null) {
				Log.Print("No todos selected.");
				return;
			}


		}
		private void NotesPanelEditTagsRequested(object sender, RoutedEventArgs e) {
			TodoListViewModel? vm = (TodoListViewModel)DataContext;
			if (vm == null) {
				Log.Print("Can not find ViewModel.");
				return;
			}
			if (vm.lbTodos.SelectedItem == null) {
				Log.Print("No todos selected.");
			return;
			}

			List<TodoItem> ihs = [];
			List<string> selectedTags = [];
			foreach (TodoItemHolder ih in lbTodos.SelectedItems) {
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
						item.Tags.Add(tag);
					}
				}
				vm.RefreshAll();
			}
		}
	}
}