using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using TODOList.Resources;


namespace TODOList.ViewModels {
	public class TodoListViewModel : INotifyPropertyChanged {
		public List<TodoItem> MasterList { get; }
		public List<TodoItemHolder> AllItems { get; }
		private ICollectionView _displayedItems;
		public ICollectionView DisplayedItems {
			get => _displayedItems;
			private set {
				_displayedItems = value;
				OnPropertyChanged();
			}
		}

		public ListBox lbTodos;

		public List<string> AllTags { get; }
		public List<string> MasterFilterTags { get; }
		// private List<string> _filterTags;
		public ObservableCollection<string> FilterTags { get; }
		private string _prioritySortTag;
		public string PrioritySortTag {
			get => _prioritySortTag;
			set {
				if (_prioritySortTag == value) {
					return;
				}
				_prioritySortTag = value;
				ApplyPriorityTagSorting();
				OnPropertyChanged();
			}
		}
		private string _currentTagFilter = "All";
		public string? CurrentTagFilter {
			get => _currentTagFilter;
			set {
				_currentTagFilter = value;
				_reverseSort = true;
				RefreshDisplayedItems();
				GetCurrentHashTags();
				OnPropertyChanged();
			}
		}

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
				OnPropertyChanged(nameof(SeverityButtonBackground));
			}
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
		private bool _reverseSort;
		private bool _previousReverseSort;


		public TodoListViewModel(List<TodoItem> masterList, List<string> masterFilterTags) {
			MasterList = masterList ?? throw new ArgumentNullException(nameof(masterList));
			AllItems = new List<TodoItemHolder>();

			MasterFilterTags = masterFilterTags ?? throw new ArgumentNullException(nameof(masterFilterTags));
			AllTags = new List<string>(MasterFilterTags);
			FilterTags = new ObservableCollection<string>(MasterFilterTags);
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
		public void RefreshAvailableTags() {
			FilterTags.Clear();

			FilterTags.Add("All");
			FilterTags.Add("#OTHER");
			FilterTags.Add("#BUG");
			FilterTags.Add("#FEATURE");

			foreach (string filter in MasterFilterTags) {
				if (filter == "All") {
					continue;
				}
				string newFilter = "#" + filter.ToUpper();
				if (FilterTags.Contains(newFilter)) {
					continue;
				}
				FilterTags.Add(newFilter);
			}
		}
		public void FixRanks() {
			if (DisplayedItems == null) {
				return;
			}
			DisplayedItems.SortDescriptions.Clear();
			DisplayedItems.SortDescriptions.Add(new SortDescription("Rank", ListSortDirection.Ascending));
			int index = 1;
			foreach (TodoItemHolder ih in DisplayedItems) {
				ih.Rank = index++;
			}
		}
		public void RefreshDisplayedItems(bool forceRefresh = false) {
			AllItems.Clear();
			foreach (TodoItem item in MasterList) {
				TodoItemHolder ih = new TodoItemHolder(item);
				ih.CurrentFilter = GetCurrentTagFilterWithoutHash();
				if (ih.Rank <= 0) {
					ih.Rank = int.MaxValue;
				}

				AllItems.Add(ih);
			}

			if (AllItems.Count == 0) {
				Log.Warn("AllItems is empty.");
				return;
			}

			DisplayedItems = CollectionViewSource.GetDefaultView(AllItems);
			DisplayedItems.Filter = CombinedFilter;
			FixRanks();

			ApplySort(forceRefresh);

			DisplayedItems?.Refresh();
		}
		public void RefreshAll() {
			RefreshDisplayedItems(true);
		}
		public void CycleSeverity() {
			CurrentSeverityFilter++;
		}
		private void ApplySort(bool forceRefresh = false) {
			if (!forceRefresh) {
				if (_currentSort != _previousSort) {
					_reverseSort = false;
				} else {
					_reverseSort = !_reverseSort;
				}
				_previousSort = _currentSort;
			}

			DisplayedItems.SortDescriptions.Clear();
			switch (CurrentSort) {
				case "date":
					DisplayedItems.SortDescriptions.Add(new SortDescription("StartDateTime", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					DisplayedItems.SortDescriptions.Add(new SortDescription("Rank", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "rank":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Rank", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "severity":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Severity", _reverseSort ? ListSortDirection.Ascending : ListSortDirection.Descending));
					DisplayedItems.SortDescriptions.Add(new SortDescription("Rank", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "active":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Active", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					DisplayedItems.SortDescriptions.Add(new SortDescription("Rank", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "kanban":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Kanban", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					DisplayedItems.SortDescriptions.Add(new SortDescription("Rank", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
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
				foreach (string tag in FilterTags) {
					if (ih.HasTag(tag)) {
						return false;
					}
				}
				return true;
			}
			return CurrentTagFilter == "All" || CurrentTagFilter == null || ih.HasTag(CurrentTagFilter);
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
		private void ContextMenuEdit() {
			if (lbTodos.SelectedItem is not TodoItemHolder ih) {
				return;
			}

			if (lbTodos.SelectedItems.Count > 1) {
				List<TodoItem> items = new List<TodoItem>();
				foreach (TodoItemHolder ihs in lbTodos.SelectedItems) {
					items.Add(ihs.TD);
				}
				MultiEditItems(items);
			} else if (lbTodos.SelectedItems.Count == 1) {
				EditItem(ih.TD);
			} else {
				Log.Error("No items selected.");
			}
		}
		private void EditItem(TodoItem item) {
			DlgTodoItemEditor itemEditor = new DlgTodoItemEditor(item, CurrentTagFilter);
			itemEditor.ShowDialog();

			if (itemEditor.Result) {
				// TODO: Check this for issues
				MainWindow.GetActiveWindow().RemoveItemFromMasterList(item);
				if (MasterList.Contains(item)) {
					MasterList.Remove(item);

					// TODO: needs to get all instances of the todo from _currentHistoryItem.CompletedTodos, CompletedBugs, CompletedFeatures
					// TODO: check that ranks work as intended
					// if (MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodos.Contains(td))
					// MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodos.Remove(td);

					// if (MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodosBugs.Contains(td))
					// MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodosBugs.Remove(td);

					// if (MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodosFeatures.Contains(td))
					// MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodosFeatures.Remove(td);

					MainWindow.GetActiveWindow().AddItemToMasterList(itemEditor.ResultTodoItem);
					// AutoSave();
				}

				// IncompleteItemsRefresh();
				// KanbanRefresh();
				// RefreshHistory();
				MasterList.Add(itemEditor.ResultTodoItem);
				RefreshDisplayedItems(true);
			}
		}
		public string GetCurrentTagFilterWithoutHash() {
			string result = CurrentTagFilter;
			if (CurrentTagFilter.Contains("#")) {
				result = result.Remove(0, 1);
			}
			return result.ToLower()
			   .CapitalizeFirstLetter();
		}
		public void ReRankWithSubsetMoved(List<TodoItem> subset, int newRankForSubsetFirstItem) {
			List<TodoItem> allItems = new();
			foreach (TodoItemHolder ih in DisplayedItems) {
				allItems.Add(ih.TD);
			}
			var remainingItems = allItems
			   .Where(item => !subset.Contains(item))
			   .ToList();

			int insertIndex = newRankForSubsetFirstItem - 1;
			insertIndex = Math.Clamp(insertIndex, 0, remainingItems.Count);
			remainingItems.InsertRange(insertIndex, subset);

			string currentFilterWithoutHash = GetCurrentTagFilterWithoutHash();
			for (int i = 0; i < remainingItems.Count; i++) {
				remainingItems[i].Rank[currentFilterWithoutHash] = i + 1;
			}

			AllItems.Clear();
			foreach (TodoItem item in remainingItems) {
				AllItems.Add(new TodoItemHolder(item));
			}
			RefreshAll();
		}

		private void MultiEditItems(List<TodoItem> items) {
			string tagFilter = GetCurrentTagFilterWithoutHash();
			DlgTodoMultiItemEditor dlg = new DlgTodoMultiItemEditor(items, tagFilter);
			dlg.ShowDialog();

			if (dlg.IsEnabled) {
				ReRankWithSubsetMoved(items, dlg.ResultRank);
			}
			foreach (TodoItem item in items) {
				if (dlg.IsSeverityEnabled) {
					item.Severity = dlg.ResultSeverity;
				}
				if (dlg.IsTodoEnabled) {
					item.Todo += " " + dlg.ResultTodo;
					Log.Print($"{item.Todo}");
				}
				if (dlg.IsTagEnabled) {
					foreach (string tag in dlg.CommonTags) {
						if (item.Tags.Contains(tag)) {
							item.Tags.Remove(tag);
						}
					}
					foreach (string tag in dlg.ResultTags) {
						if (!item.Tags.Contains(tag)) {
							item.Tags.Add(tag);
						}
					}
				}
				if (dlg.IsCompleteEnabled) {
					item.IsComplete = true;
				}
			}

			RefreshAll();
			/*
			IncompleteItemsRefresh();
			KanbanRefresh();
			*/
		}


		public ICommand ContextMenuEditCommand => new RelayCommand(ContextMenuEdit);
		// public ICommand ContextMenuDeleteCommand  => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuResetTimerCommand => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuKanban3Command => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuKanban2Command => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuKanban1Command => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuKanban0Command => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuMoveToTopCommand => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));
		// public ICommand ContextMenuMoveToBottomCommand => new RelayCommand<TodoItemHolder>(item => ContextMenuEdit(item));

		public ICommand SelectTagCommand
			=> new RelayCommand<string>(tag => { CurrentTagFilter = tag is null or "All" ? "All" : tag; });
		public ICommand SelectSortCommand
			=> new RelayCommand<string>(sort => { CurrentSort = sort; });
		public ICommand CycleSeverityCommand => new RelayCommand(CycleSeverity);

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}