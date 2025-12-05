using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using TODOList.Resources;

// using System.Windows.Input;

namespace TODOList.ViewModels {
	public class TodoListViewModel : INotifyPropertyChanged {
		public ObservableCollection<TodoItem> MasterList { get; }
		public ObservableCollection<TodoItemHolder> AllItems { get; }
		private ICollectionView _displayedItems;
		public ICollectionView DisplayedItems {
			get => _displayedItems;
			private set {
				_displayedItems = value;
				OnPropertyChanged();
			}
		}

		public ListBox lbTodos;

		public ObservableCollection<string> AllTags { get; }
		public ObservableCollection<string> FilteredTags { get; }
		public ObservableCollection<string> HashTags { get; set; }


		private string _currentTagFilter = null;
		public string CurrentTagFilter {
			get => _currentTagFilter;
			set {
				_currentTagFilter = value;
				_reverseSort = true;
				RefreshDisplayedItems();
				GetCurrentHashTags();
				OnPropertyChanged();
			}
		}

		// TODO: Change this to RANK after testing
		private string _currentSort = "severity";
		public string CurrentSort {
			get => _currentSort;
			set {
				_currentSort = value;
				RefreshDisplayedItems();
				OnPropertyChanged();
			}
		}
		private string _previousSort = "";
		private bool _reverseSort = true;
		private bool _previousReverseSort;

		private int _currentSeverityFilter = -1;
		public int CurrentSeverityFilter {
			get => _currentSeverityFilter;
			set {
				if (value > 3) {
					value = -1;
				}
				_currentSeverityFilter = value;
				RefreshDisplayedItems();
				OnPropertyChanged();
				OnPropertyChanged(nameof(SeverityButtonText));
				OnPropertyChanged(nameof(SeverityButtonBackground));
			}
		}

		private string? _prioritySortTag;
		public string? PrioritySortTag {
			get => _prioritySortTag;
			set {
				if (_prioritySortTag != value) {
					_prioritySortTag = value;
					ApplyPriorityTagSorting();
					OnPropertyChanged();
				}
			}
		}

		public TodoListViewModel(ObservableCollection<TodoItem> allItems, ObservableCollection<string> allTags) {
			MasterList = allItems ?? throw new ArgumentNullException(nameof(allItems));
			FilteredTags = allTags ?? throw new ArgumentNullException(nameof(allTags));
			AllTags = new ObservableCollection<string>(FilteredTags);
			AllItems = new ObservableCollection<TodoItemHolder>();
			RefreshAvailableTags();

			CycleSeverityCommand = new RelayCommand(CycleSeverity);
		}
		public void RefreshAll() {
			RefreshDisplayedItems(true);
		}
		public string SeverityButtonText => CurrentSeverityFilter switch {
												3 => "High",
												2 => "Med",
												1 => "Low",
												0 => "None",
												_ => ""
											};
		public Brush SeverityButtonBackground => CurrentSeverityFilter switch {
													 3 => new SolidColorBrush(Color.FromRgb(190, 0, 0)), // High = Red
													 2 => new SolidColorBrush(Color.FromRgb(200, 160, 0)), // Med = Yellow/Orange
													 1 => new SolidColorBrush(Color.FromRgb(0, 140, 0)), // Low = Green
													 0 => new SolidColorBrush(Color.FromRgb(50, 50, 50)), // Off = Dark gray (your normal tag color)
													 _ => new SolidColorBrush(Color.FromRgb(25, 25, 25)) // Off = Dark gray (your normal tag color)
												 };
		public void CycleSeverity() {
			CurrentSeverityFilter++;
		}
		public void RefreshAvailableTags() {
			if (HashTags == null) {
				HashTags = new ObservableCollection<string>();
			}
			HashTags.Clear();

			HashTags.Add("All");
			HashTags.Add("#OTHER");
			HashTags.Add("#BUG");
			HashTags.Add("#FEATURE");

			foreach (string tag in FilteredTags) {
				string hash = "#" + tag.ToUpper();
				if (HashTags.Contains(hash)) {
					continue;
				}
				HashTags.Add(hash);
			}
		}
		public void GetCurrentHashTags() {
			AllTags.Clear();
			foreach (TodoItemHolder ih in DisplayedItems) {
				foreach (string tag in ih.Tags) {
					if (AllTags.Contains(tag)) {
						continue;
					}
					AllTags.Add(tag);
				}
			}
		}
		public void RefreshDisplayedItems(bool refresh = false) {
			AllItems.Clear();
			foreach (TodoItem item in MasterList) {
				AllItems.Add(new TodoItemHolder(item));
			}

			if (AllItems.Count == 0) {
				Log.Warn("AllItems is empty.");
				return;
			}

			DisplayedItems = CollectionViewSource.GetDefaultView(AllItems);
			DisplayedItems.Filter = CombinedFilter;
			ApplySort(refresh);

			DisplayedItems?.Refresh();
		}
		private void ApplySort(bool refresh = false) {
			if (!refresh) {
				if (_currentSort != _previousSort) {
					_reverseSort = false;
					_previousReverseSort = true;
				} else {
					_previousReverseSort = _reverseSort;
					_reverseSort = !_reverseSort;
				}
				_previousSort = _currentSort;
			}

			DisplayedItems.SortDescriptions.Clear();
			switch (CurrentSort) {
				case "date":
					DisplayedItems.SortDescriptions.Add(new SortDescription("StartDateTime", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "rank":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Rank", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "severity":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Severity", _reverseSort ? ListSortDirection.Ascending : ListSortDirection.Descending));
					break;
				case "active":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Active", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "kanban":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Kanban", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
			}
		}
		private bool CombinedFilter(object item) {
			if (item is not TodoItemHolder ih) {
				return false;
			}

			// Filter by Severity
			if (CurrentSeverityFilter != -1) {
				if (ih.Severity != CurrentSeverityFilter) {
					return false;
				}
			}

			// Filter by Tag
			if (CurrentTagFilter == "#OTHER") {
				foreach (string tag in HashTags) {
					if (ih.HasTag(tag)) {
						return false;
					}
				}
				return true;
			}
			return CurrentTagFilter == "#ALL" || CurrentTagFilter == null || ih.HasTag(CurrentTagFilter);
		}
		private void ApplyPriorityTagSorting() {
			DisplayedItems.SortDescriptions.Clear();

			foreach (TodoItemHolder ih in DisplayedItems) {
				if (ih.HasTag(PrioritySortTag)) {
					ih.IsPrioritySorted = true;
				}
			}

			DisplayedItems.SortDescriptions.Add(new SortDescription(nameof(TodoItemHolder.IsPrioritySorted), ListSortDirection.Descending));
			DisplayedItems.SortDescriptions.Add(new SortDescription(nameof(TodoItemHolder.HasTags), ListSortDirection.Descending));
			DisplayedItems.SortDescriptions.Add(new SortDescription(nameof(TodoItemHolder.FirstTag), ListSortDirection.Ascending));
			ResetPrioritySortTags();
		}
		private void ResetPrioritySortTags() {
			foreach (TodoItemHolder ih in DisplayedItems) {
				ih.IsPrioritySorted = false;
			}
		}
		//TODO: Remove this later
		public void SetRankTemp() {
			int index = 1;
			foreach (TodoItemHolder ih in DisplayedItems) {
				ih.Rank = index;
				index++;
			}
		}
		private void ContextMenuEdit(TodoListViewModel vm) {
			if (lbTodos.SelectedItem is not TodoItemHolder ih) {
				return;
			}
			Log.Test();
			TodoItem item = ih.TD;
			if (lbTodos.SelectedItems.Count > 1) {
				MultiEditItems(lbTodos);
			} else if (lbTodos.SelectedItems.Count == 1) {
				EditItem(item);
			} else {
				Log.Error("No selected items!");
			}
		}
		private void EditItem(TodoItem item) {
			Log.Debug($"{item}");
			DlgTodoItemEditor itemEditor = new DlgTodoItemEditor(item, MainWindow.GetActiveWindow().TabNames);
			itemEditor.ShowDialog();

			// TODO: needs to get all instances of the todo from _currentHistoryItem.CompletedTodos, CompletedBugs, CompletedFeatures
			// TODO: check that ranks work as intended
			if (itemEditor.Result) {
				MainWindow.GetActiveWindow().RemoveItemFromMasterList(item);
				if (MasterList.Contains(item)) {
					MasterList.Remove(item);
				}
				MasterList.Add(itemEditor.ResultTodoItem);
			}
			var list = MainWindow.GetActiveWindow()._masterList;
			RefreshDisplayedItems(true);
		}
		private void EditItem(Selector lb, IReadOnlyList<TodoItem> list) {
			// UNCHECKED
			int index = lb.SelectedIndex;
			if (index < 0)
				return;

			TodoItem td = list[index];
			DlgTodoItemEditor itemEditor = new DlgTodoItemEditor(td, MainWindow.GetActiveWindow().TabNames);

			itemEditor.ShowDialog();
			if (itemEditor.Result) {
				MainWindow.GetActiveWindow().RemoveItemFromMasterList(td);
				if (MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodos.Contains(td))
					MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodos.Remove(td);

				if (MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodosBugs.Contains(td))
					MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodosBugs.Remove(td);

				if (MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodosFeatures.Contains(td))
					MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodosFeatures.Remove(td);

				MainWindow.GetActiveWindow().AddItemToMasterList(itemEditor.ResultTodoItem);
				// AutoSave();
			}

			// IncompleteItemsRefresh();
			// KanbanRefresh();
			// RefreshHistory();
		}
		private void MultiEditItems(ListBox lb) {
			// UNCHECKED
			/*
			TodoItemHolder firstTd = lb.SelectedItems[0] as TodoItemHolder;

			List<string> tags = new List<string>();
			List<string> commonTagsTemp = new List<string>();
			foreach (TodoItemHolder itemHolder in lb.SelectedItems) {
				foreach (string tag in itemHolder.TD.Tags) {
					if (!tags.Contains(tag))
						tags.Add(tag);
					else if (!commonTagsTemp.Contains(tag))
						commonTagsTemp.Add(tag);
				}
			}

			List<string> commonTags = commonTagsTemp.ToList();
			foreach (TodoItemHolder itemHolder in lb.SelectedItems)
			foreach (string tag in commonTagsTemp.Where(tag => !itemHolder.TD.Tags.Contains(tag)))
				commonTags.Remove(tag);

			if (firstTd == null)
				return;

			DlgTodoMultiItemEditor dlgTodoMultiItemEditor =
				new DlgTodoMultiItemEditor(firstTd.TD, TabNames, commonTags);
			dlgTodoMultiItemEditor.ShowDialog();
			if (!dlgTodoMultiItemEditor.Result)
				return;

			List<string> tagsToRemove =
				commonTags.Where(tag => !dlgTodoMultiItemEditor.ResultTags.Contains(tag)).ToList();

			foreach (TodoItemHolder itemHolder in lb.SelectedItems) {
				if (dlgTodoMultiItemEditor.ChangeTag) {
					foreach (string tag in tagsToRemove)
						itemHolder.TD.Tags.Remove(tag);
					foreach (string tag in
							 dlgTodoMultiItemEditor.ResultTags
								.Where(tag => !itemHolder.TD.Tags.Contains(tag.ToUpper())))
						itemHolder.TD.Tags.Add(tag.ToUpper());
				}

				if (dlgTodoMultiItemEditor.ChangeRank)
					itemHolder.TD.Rank = dlgTodoMultiItemEditor.ResultTD.Rank;
				if (dlgTodoMultiItemEditor.ChangeSev)
					itemHolder.TD.Severity = dlgTodoMultiItemEditor.ResultTD.Severity;
				if (dlgTodoMultiItemEditor.ResultIsComplete && dlgTodoMultiItemEditor.ChangeComplete)
					itemHolder.TD.IsComplete = true;
				if (!dlgTodoMultiItemEditor.ChangeTodo)
					continue;

				itemHolder.TD.Todo += Environment.NewLine + dlgTodoMultiItemEditor.ResultTD.Todo;
				foreach (string tag in
						 dlgTodoMultiItemEditor.ResultTD.Tags.Where(tag => !itemHolder.TD.Tags.Contains(tag)))
					itemHolder.TD.Tags.Add(tag);
			}

			IncompleteItemsRefresh();
			KanbanRefresh();
			*/
		}


		public ICommand ContextMenuEditCommand => new RelayCommand<TodoListViewModel>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuDeleteCommand  => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuResetTimerCommand => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuKanban3Command => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuKanban2Command => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuKanban1Command => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuKanban0Command => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuMoveToTopCommand => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuMoveToBottomCommand => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));

		public ICommand SelectTagCommand
			=> new RelayCommand<string>(tag => { CurrentTagFilter = tag == "All" ? null : tag; });
		public ICommand SelectSortCommand
			=> new RelayCommand<string>(sort => { CurrentSort = sort; });
		public ICommand CycleSeverityCommand { get; }

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}