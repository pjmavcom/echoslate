using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;

namespace Echoslate.Wpf.Views;

public partial class TodoDisplayView : UserControl {
	public TodoDisplayView() {
		InitializeComponent();
	}
	private void Todos_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
		foreach (TodoItem ih in e.RemovedItems.OfType<TodoItem>()) {
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
	private async void NotesPanelEditTagsRequested(object sender, RoutedEventArgs e) {
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
		foreach (TodoItem ih in vm.SelectedTodoItems) {
			ihs.Add(ih);
		}

		selectedTags = new List<string>(ihs.Select(x => x.Tags ?? Enumerable.Empty<string>()).Aggregate((a, b) => a.Intersect(b).ToList()));

		Task<TagPickerViewModel?> vmTask = AppServices.DialogService.ShowTagPickerAsync(ihs, vm.AllTags, new List<string>(selectedTags));
		TagPickerViewModel tpvm = await vmTask;
		if (tpvm == null) {
			return;
		}
		if (tpvm.Result) {
			foreach (TodoItem item in ihs) {
				foreach (string tag in selectedTags) {
					item.Tags.Remove(tag);
				}
				foreach (string tag in tpvm.SelectedTags) {
					item.AddTag(tag);
				}
			}
			vm.CleanAllTodoHashRanks();
			vm.RefreshAll();
		}
	}
	public ICommand RefreshAllCommand => new RelayCommand(() => {
		TodoDisplayViewModelBase? vm = (TodoDisplayViewModelBase)DataContext;
		vm.RefreshAll();
	});

	private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
		if (sender is ListBoxItem lbItem) {
			if (lbItem.DataContext is TodoItem ih) {
				TodoItem item = ih;
				if (DataContext is TodoDisplayViewModelBase vm) {
					vm.EditItem(item);
				}
			}
		}
	}
	private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
	}
}