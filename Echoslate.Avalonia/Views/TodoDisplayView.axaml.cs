using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Views;

public partial class TodoDisplayView : UserControl {
	public TodoDisplayView() {
		InitializeComponent();
	}
	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		if (IsVisible && DataContext is TodoDisplayViewModelBase vm)
			vm.RefreshAll();
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
	}
	private void Todos_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
		foreach (TodoItem ih in e.RemovedItems.OfType<TodoItem>()) {
			ih.CleanNotes();
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
	private void ListBox_DoubleTapped(object? sender, TappedEventArgs e) {
		if (sender is ListBox lb && lb.SelectedItem is TodoItem todoItem) {
			if (DataContext is TodoDisplayViewModelBase vm) {
				Log.Print($"Editing: {todoItem.Id}");
				vm.EditItem(todoItem);
			}
		}
	}
	private void Window_KeyDown(object sender, KeyEventArgs e) {
		if (DataContext is TodoDisplayViewModelBase vm) {
			if (e.Key == Key.Enter) {
				if (e.KeyModifiers == KeyModifiers.Control) {
					vm.AddAndComplete();
				} else {
					vm.NewTodoAdd();
				}
			}
			if (e.Key == Key.K && e.KeyModifiers == KeyModifiers.Alt) {
				vm.ChangeSeverityHotkeyCommand.Execute("up");
			}
			if (e.Key == Key.J && e.KeyModifiers == KeyModifiers.Alt) {
				vm.ChangeSeverityHotkeyCommand.Execute("down");
			}
		}
	}

	private void Severity_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		if (DataContext is TodoDisplayViewModelBase vm && sender is TextBlock textBlock && textBlock.DataContext is TodoItem item) {
			vm.ChangeSeverityCommand.Execute(item);
		}
	}
	private void Severity_OnDoubleTapped(object? sender, TappedEventArgs e) {
		if (DataContext is TodoDisplayViewModelBase vm && sender is TextBlock textBlock && textBlock.DataContext is TodoItem item) {
			vm.ChangeSeverityCommand.Execute(item);
		}
	}
}