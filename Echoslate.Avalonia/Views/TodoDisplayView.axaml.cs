using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Views;

public partial class TodoDisplayView : UserControl, INotifyPropertyChanged {
	private DataGridColumn? ColTags;
	private DataGridColumn? ColDate;
	private DataGridColumn? ColSev;
	private DataGridColumn? ColRank;
	private DataGridColumn? ColTimer;

	private ItemNotesPanelView? _notesPanel;
	private Button? _notesPanelToggleButton;
	private const double MinNotesPanelSize = 200;
	private const double MaxNotesPanelSize = 600;
	private const double PanelShrinkThreshold1 = 1200;
	private const double PanelShrinkThreshold2 = 800;

	private double _currentWidth;

	public TodoDisplayView() {
		InitializeComponent();
	}
	protected override void OnDataContextChanged(EventArgs e) {
		base.OnDataContextChanged(e);

		if (IsVisible && DataContext is TodoDisplayViewModelBase vm)
			vm.RefreshAll();
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
		DataGrid? todoListDataGrid = this.FindControl<DataGrid>("TodoListDataGrid");
		ColTags = todoListDataGrid.Columns[0];
		ColDate = todoListDataGrid.Columns[1];
		ColSev = todoListDataGrid.Columns[2];
		ColRank = todoListDataGrid.Columns[3];
		ColTimer = todoListDataGrid.Columns[4];

		_notesPanel = this.FindControl<ItemNotesPanelView>("NotesPanel");
		_notesPanelToggleButton = this.FindControl<Button>("NotesPanelToggleButton");

		SizeChanged += OnSizeChanged;

		var addButton = this.FindControl<Button>("AddButton");
		addButton.AddHandler(
			InputElement.PointerPressedEvent,
			Add_OnPointerPressed,
			RoutingStrategies.Tunnel);
	}
	private void OnSizeChanged(object? sender, SizeChangedEventArgs e) {
		if (e.NewSize.Width <= 0) {
			return;
		}
		Log.Print($"size: {e.NewSize.Width}");
		UpdateColumnVisibility(e.NewSize.Width);
	}
	private void UpdateColumnVisibility(double width) {
		_currentWidth = width;
		double notesPanel = _notesPanel.IsVisible ? 0 : MinNotesPanelSize;
		ColTags.IsVisible = width < (1800 - notesPanel) ? false : true;
		ColDate.IsVisible = width < 1600 - notesPanel ? false : true;
		ColTimer.IsVisible = width < 1400 - notesPanel ? false : true;
		ColSev.IsVisible = width < 1200 - notesPanel ? false : true;
		ColRank.IsVisible = width < 600 - notesPanel ? false : true;

		if (width > PanelShrinkThreshold1) {
			_notesPanel.Width = MaxNotesPanelSize;
		} else if (width > PanelShrinkThreshold2) {
			_notesPanel.Width = MaxNotesPanelSize - (PanelShrinkThreshold1 - width);
		}
		if (DataContext is TodoDisplayViewModelBase vm) {
			if (width < PanelShrinkThreshold2) {
				vm.IsNotesPanelVisibleBySize = false;
				_notesPanelToggleButton.IsVisible = false;
			} else {
				vm.IsNotesPanelVisibleBySize = true;
				_notesPanelToggleButton.IsVisible = true;
			}
		}
	}
	public void ToggleNotesPanel(object? sender, RoutedEventArgs e) {
		if (DataContext is TodoDisplayViewModelBase vm) {
			vm.ToggleNotesPanelCommand.Execute(null);
		}
		UpdateColumnVisibility(_currentWidth);
	}
	private void Add_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		if (DataContext is not TodoDisplayViewModelBase vm) {
			return;
		}
		var point = e.GetCurrentPoint((Visual?)sender!);

		if (point.Properties.IsLeftButtonPressed && e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
			vm.ShowTodoItemEditorOnAdd = !vm.ShowTodoItemEditorOnAdd;
			e.Handled = true;
		}
	}


	private void Add_OnClick(object? sender, RoutedEventArgs e) {
		if (DataContext is TodoDisplayViewModelBase vm) {
			vm.NewTodoAddCommand.Execute(null);
		}
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

		Task<TagPickerViewModel?> vmTask = AppServices.DialogService.ShowTagPickerAsync(ihs, vm.AllTags, new ObservableCollection<string>(selectedTags));
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
	private void DataGrid_DoubleTapped(object? sender, TappedEventArgs e) {
		if (sender is DataGrid lb && lb.SelectedItem is TodoItem todoItem) {
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
		if (DataContext is TodoDisplayViewModelBase vm && sender is Border border && border.DataContext is TodoItem item) {
			vm.ChangeSeverityCommand.Execute(item);
		}
	}
	private void Severity_OnDoubleTapped(object? sender, TappedEventArgs e) {
		if (DataContext is TodoDisplayViewModelBase vm && sender is Border border && border.DataContext is TodoItem item) {
			vm.ChangeSeverityCommand.Execute(item);
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}