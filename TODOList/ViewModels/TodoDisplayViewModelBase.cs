using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Components;
using Echoslate.Resources;


namespace Echoslate.ViewModels {
	public abstract class TodoDisplayViewModelBase : INotifyPropertyChanged {
		public AppData Data { get; set; }
		public ObservableCollection<TodoItem> MasterList { get; set; }
		public ObservableCollection<TodoItemHolder> AllItems { get; set; }
		private ICollectionView _displayedItems;
		public ICollectionView DisplayedItems {
			get => _displayedItems;
			set {
				_displayedItems = value;
				OnPropertyChanged();
			}
		}
		public ObservableCollection<HistoryItem> HistoryItems { get; set; }
		public HistoryItem CurrentHistoryItem { get; set; }

		public ObservableCollection<FilterButton> FilterButtons { get; set; } = [];

		public ObservableCollection<string> AllTags { get; set; }
		public ObservableCollection<string> CurrentVisibleTags { get; set; }
		public ObservableCollection<string> MasterFilterTags { get; set; }
		public ObservableCollection<string> FilterList { get; set; }
		private string _prioritySortTag;
		public string PrioritySortTag {
			get => _prioritySortTag;
			set {
				if (_prioritySortTag == value) {
					return;
				}
				_prioritySortTag = value;
				RefreshAll();
				ApplyPriorityTagSorting();
				OnPropertyChanged();
			}
		}
		private string _currentFilter = "All";
		public string? CurrentFilter {
			get => _currentFilter;
			set {
				_currentFilter = value;
				// _reverseSort = false;
				RefreshAll();
				OnPropertyChanged();
			}
		}

		private static SolidColorBrush SeverityBrush(int severity) =>
			severity switch {
				3 => new SolidColorBrush(Color.FromRgb(190, 0, 0)),   // High = Red
				2 => new SolidColorBrush(Color.FromRgb(200, 160, 0)), // Med = Yellow/Orange
				1 => new SolidColorBrush(Color.FromRgb(0, 140, 0)),   // Low = Green
				0 => new SolidColorBrush(Color.FromRgb(50, 50, 50)),  // Off = Dark gray (your normal tag color)
				_ => new SolidColorBrush(Color.FromRgb(25, 25, 25))   // Off = Dark gray (your normal tag color)
			};
		private int _currentSeverityFilter;
		public int CurrentSeverityFilter {
			get => _currentSeverityFilter;
			set {
				if (value > 3) {
					value = -1;
				}
				_currentSeverityFilter = value;
				CurrentSeverityBrush = SeverityBrush(CurrentSeverityFilter);
				RefreshAll();
				OnPropertyChanged();
			}
		}
		private SolidColorBrush _currentSeverityBrush;
		public SolidColorBrush CurrentSeverityBrush {
			get => _currentSeverityBrush;
			set {
				_currentSeverityBrush = value;
				RefreshAll();
				OnPropertyChanged();
			}
		}


		// TODO: Change this to RANK after testing
		// or make it an option
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
		protected bool _reverseSort;
		private bool _previousReverseSort;

		private string _newTodoText;
		public string NewTodoText {
			get => _newTodoText;
			set {
				_newTodoText = value;
				OnPropertyChanged();
			}
		}
		private int _newTodoSeverity;
		public int NewTodoSeverity {
			get => _newTodoSeverity;
			set {
				_newTodoSeverity = value % 4;
				NewTodoSeverityBrush = SeverityBrush(NewTodoSeverity);
				OnPropertyChanged();
			}
		}
		private SolidColorBrush _newTodoSeverityBrush;
		public SolidColorBrush NewTodoSeverityBrush {
			get => _newTodoSeverityBrush;
			set {
				_newTodoSeverityBrush = value;
				OnPropertyChanged();
			}
		}
		public IList SelectedTodoItems { get; } = new ObservableCollection<object>();

		private object _selectedTodoItem;
		public object SelectedTodoItem {
			get => _selectedTodoItem;
			set {
				if (_selectedTodoItem != value) {
					if (_selectedTodoItem is TodoItemHolder item) {
						item.TD.PropertyChanged -= TodoItem_PropertyChanged;
					}
					_selectedTodoItem = value;
					if (_selectedTodoItem is TodoItemHolder newItem) {
						newItem.TD.PropertyChanged += TodoItem_PropertyChanged;
					}
					
					OnPropertyChanged();
				}
			}
		}

		public TodoDisplayViewModelBase() {
		}
		public virtual void Initialize(MainWindowViewModel mainWindowVM) {
			Data = mainWindowVM.Data;
			MasterList = mainWindowVM.MasterTodoItemsList;
			HistoryItems = mainWindowVM.MasterHistoryItemsList;
			MasterFilterTags = mainWindowVM.MasterFilterTags ?? throw new ArgumentNullException(nameof(mainWindowVM.MasterFilterTags));

			AllItems = [];
			FilterButtons = [];

			CurrentVisibleTags = new ObservableCollection<string>(MasterFilterTags);
			AllTags = [];
			FilterList = new ObservableCollection<string>(MasterFilterTags);

			CurrentSeverityFilter = -1;
			NewTodoSeverity = 0;
			RefreshAll();
		}
		protected abstract void RefreshFilter();
		protected abstract bool MatchFilter(ObservableCollection<string> filterList, TodoItemHolder ih);
		protected abstract bool RefreshAllItems();

		public void TodoItem_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (sender is TodoItem item) {
				item.UpdateTags(Data.AllTags);
			}
		}
		public void RebuildView() {
			DisplayedItems = CollectionViewSource.GetDefaultView(AllItems);
		}
		public void GetCurrentHashTags() {
			CurrentVisibleTags.Clear();
			if (DisplayedItems == null || MasterList == null) {
				return;
			}
			foreach (TodoItemHolder ih in DisplayedItems) {
				foreach (string tag in ih.Tags) {
					if (CurrentVisibleTags.Contains(tag)) {
						continue;
					}
					CurrentVisibleTags.Add(tag);
				}
			}
			AllTags.Clear();
			foreach (TodoItem item in MasterList) {
				foreach (string tag in item.Tags) {
					if (!AllTags.Contains(tag)) {
						AllTags.Add(tag);
					}
				}
			}
			OnPropertyChanged(nameof(CurrentVisibleTags));
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
			if (RefreshAllItems()) {
				return;
			}

			DisplayedItems = CollectionViewSource.GetDefaultView(AllItems);
			DisplayedItems.Filter = CombinedFilter;
			FixRanks();

			ApplySort(forceRefresh);

			DisplayedItems?.Refresh();
		}
		public void RefreshAll() {
			RefreshFilter();
			RefreshDisplayedItems(true);
			GetCurrentHashTags();
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

			// Filter by type
			return MatchFilter(FilterList, ih);
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
			if (SelectedTodoItems[0] is not TodoItemHolder ih) {
				return;
			}

			if (SelectedTodoItems.Count > 1) {
				List<TodoItem> items = new List<TodoItem>();
				foreach (TodoItemHolder ihs in SelectedTodoItems) {
					items.Add(ihs.TD);
				}
				MultiEditItems(items);
			} else if (SelectedTodoItems.Count == 1) {
				EditItem(ih.TD);
			} else {
				Log.Error("No items selected.");
			}
		}
		public void MarkSelectedItemAsComplete() {
			if (SelectedTodoItems == null || SelectedTodoItems.Count == 0 || SelectedTodoItems[0] == null) {
				Log.Warn("No todos selected.");
				return;
			}
			TodoItemHolder? ih = SelectedTodoItems[0] as TodoItemHolder;
			TodoItem? item = ih?.TD;
			
			if (item != null) {
				MarkTodoAsComplete(item);
			}
			Log.Warn("Selected item is null.");
		}
		public void MarkTodoAsComplete(TodoItem item) {
			item.IsComplete = true;
			RefreshAllItems();
			SortCompleteTodosToHistory();
		}
		public void SortCompleteTodosToHistory() {
			foreach (TodoItemHolder ih in AllItems) {
				if (ih.TD.IsComplete) {
					Log.Debug($"{ih.TD}");

					RemoveItemFromMasterList(ih.TD);
					AddItemToHistory(ih.TD);
				}
			}
			RefreshAll();
		}
		public void AddItemToHistory(TodoItem item) {
			CurrentHistoryItem = HistoryItems[0];
			CurrentHistoryItem.AddCompletedTodo(item);
		}
		private void EditItem(TodoItem item) {
			DlgTodoItemEditor dlg = new DlgTodoItemEditor(item, GetCurrentTagFilterWithoutHash());
			dlg.ShowDialog();

			Log.Debug($"{item}");
			Log.Debug($"{dlg.ResultTodoItem}");

			if (dlg.Result) {
				TodoItem newItem = dlg.ResultTodoItem;
				RemoveItemFromMasterList(item);
				AddItemToMasterList(newItem);

				if (newItem.IsComplete) {
					MarkTodoAsComplete(newItem);
				} else {
					ReRankWithSubsetMoved(newItem, dlg.Rank);
				}

				RefreshAll();
			}
		}
		private void CleanTodoHashRanks(TodoItem td) {
			List<string> remove = (from pair in td.Rank where !MasterFilterTags.Contains(pair.Key) select pair.Key).ToList();
			foreach (string hash in remove)
				td.Rank.Remove(hash);
			foreach (string name in MasterFilterTags.Where(name => !td.Rank.ContainsKey(name)))
				td.Rank.Add(name, -1);
		}
		public void AddItemToMasterList(TodoItem item) {
			if (MasterListContains(item) >= 0) {
				Log.Warn("MasterList already contains Item.");
				return;
			}
			CleanTodoHashRanks(item);
			LookForNewTags(item);
		}
		private void LookForNewTags(TodoItem item) {
			bool newTagFound = false;
			MasterList.Add(item);
			foreach (string t in item.Tags) {
				if (!Data.AllTags.Contains(t)) {
					newTagFound = true;
					Data.AllTags.Add(t);
				}
			}
			if (newTagFound) {
				UpdateTags();
			}
		}
		private void UpdateTags() {
			foreach (TodoItem item in MasterList) {
				item.UpdateTags(Data.AllTags);
			}
		}
		public void RemoveItemFromMasterList(TodoItem? item) {
			if (item == null) {
				Log.Warn("item is null.");
				return;
			}
			int index = MasterListContains(item);
			if (index == -1) {
				Log.Warn("MasterList does not contain TodoItem.");
				return;
			}
			MasterList.RemoveAt(index);
			Log.Print("TodoItem removed from MasterList.");
		}
		private int MasterListContains(TodoItem td) {
			if (MasterList.Contains(td))
				return MasterList.IndexOf(td);
			return -1;
		}
		public string GetCurrentTagFilterWithoutHash() {
			string result = CurrentFilter;
			if (CurrentFilter.Contains("#")) {
				result = result.Remove(0, 1);
			}
			return result.ToLower()
			   .CapitalizeFirstLetter();
		}
		public void ReRankWithSubsetMoved(TodoItem subset, int newRankForSubsetFirstItem) {
			List<TodoItem> allItems = new();
			foreach (TodoItemHolder ih in DisplayedItems) {
				allItems.Add(ih.TD);
			}
			var remainingItems = allItems
			   .Where(item => item != subset)
			   .ToList();

			int insertIndex = newRankForSubsetFirstItem - 1;
			insertIndex = Math.Clamp(insertIndex, 0, remainingItems.Count);
			remainingItems.Insert(insertIndex, subset);

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

		private void ExpandHashTags(TodoItem td) {
			// UNCHECKED
			string tempTodo = ExpandHashTagsInString(td.Todo);
			string tempTags = ExpandHashTagsInList(td.Tags);
			td.Todo = tempTags.Trim() + " " + tempTodo.Trim();
		}
		public string ExpandHashTagsInString(string todo) {
			// UNCHECKED
			string[] pieces = todo.Split(' ');

			List<string> list = new List<string>();
			foreach (string piece in pieces) {
				string s = piece;
				if (s.Contains('#')) {
					string t = s.ToUpper();
					if (t.Equals("#FEATURES"))
						t = "#FEATURE";

					if (t.Equals("#BUGS"))
						t = "#BUG";

					// foreach (string hash in from pair in HashShortcuts
					// where t.Equals("#" + pair.Key.ToUpper())
					// select "#" + pair.Value) {
					// s = hash;
					// }

					s = s.ToLower();
				}

				list.Add(s);
			}

			return list.Where(s => s != "").Aggregate("", (current, s) => current + (s + " "));
		}
		private string ExpandHashTagsInList(ObservableCollection<string> tags) {
			// UNCHECKED
			string result = tags.Aggregate("", (current, s) => current + (s + " "));

			result = ExpandHashTagsInString(result);
			return result;
		}
		private void MultiEditItems(List<TodoItem> items) {
			string tagFilter = GetCurrentTagFilterWithoutHash();
			DlgTodoMultiItemEditor dlg = new DlgTodoMultiItemEditor(items, tagFilter);
			dlg.ShowDialog();

			if (dlg.IsRankEnabled) {
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
					MarkTodoAsComplete(item);
				}
			}

			RefreshAll();
			/*
			IncompleteItemsRefresh();
			KanbanRefresh();
			*/
		}
		public void EditFilterBarRelayCommand() {
			DlgEditTabs dlg = new(MasterFilterTags);
			dlg.ShowDialog();
			if (dlg.Result) {
				Log.Print("Filters successfully edited");
				MasterFilterTags.Clear();
				MasterFilterTags = dlg.ResultList;
				foreach (TodoItem td in MasterList) {
					CleanTodoHashRanks(td);
				}
				RefreshAll();
			}
		}
		public List<TodoItem> GetSelectedListBoxItems() {
			List<TodoItem> items = [];
			// foreach (TodoItemHolder ih in lbTodos.SelectedItems) {
			// items.Add(ih.TD);
			// }

			foreach (TodoItemHolder ih in SelectedTodoItems) {
				items.Add(ih.TD);
			}
			return items;
		}
		public List<TodoItemHolder> GetSelectedListBoxHolders() {
			List<TodoItemHolder> ihs = [];
			foreach (TodoItemHolder ih in SelectedTodoItems) {
				ihs.Add(ih);
			}
			// foreach (TodoItemHolder ih in lbTodos.SelectedItems) {
			// 	ihs.Add(ih);
			// }
			return ihs;
		}
		// private void ItemNotesPanel_EditTagsRequested(object sender, EventArgs e) {
		// Log.Test();
		// }
		public ICommand ContextMenuEditCommand => new RelayCommand(ContextMenuEdit);
		public ICommand ContextMenuDeleteCommand => new RelayCommand(() => {
			foreach (TodoItem item in GetSelectedListBoxItems()) {
				RemoveItemFromMasterList(item);
			}
			RefreshAll();
		});
		public ICommand ContextMenuResetTimerCommand => new RelayCommand(() => {
			foreach (TodoItem item in GetSelectedListBoxItems()) {
				item.ResetTimer();
			}
			RefreshAll();
		});
		public ICommand ContextMenuKanban3Command => new RelayCommand(() => {
			foreach (TodoItemHolder ih in GetSelectedListBoxHolders()) {
				ih.Kanban = 3;
			}
			RefreshAll();
		});
		public ICommand ContextMenuKanban2Command => new RelayCommand(() => {
			foreach (TodoItemHolder ih in GetSelectedListBoxHolders()) {
				ih.Kanban = 2;
			}
			RefreshAll();
		});
		public ICommand ContextMenuKanban1Command => new RelayCommand(() => {
			foreach (TodoItemHolder ih in GetSelectedListBoxHolders()) {
				ih.Kanban = 1;
			}
			RefreshAll();
		});
		public ICommand ContextMenuKanban0Command => new RelayCommand(() => {
			foreach (TodoItemHolder ih in GetSelectedListBoxHolders()) {
				ih.Kanban = 0;
			}
			RefreshAll();
		});
		public ICommand ContextMenuMoveToTopCommand => new RelayCommand(() => {
			ReRankWithSubsetMoved(GetSelectedListBoxItems(), 0);
			RefreshAll();
		});
		public ICommand ContextMenuMoveToBottomCommand => new RelayCommand(() => {
			ReRankWithSubsetMoved(GetSelectedListBoxItems(), int.MaxValue);
			RefreshAll();
		});
		public ICommand ContextMenuCompleteCommand => new RelayCommand(() => {
			foreach (TodoItem item in GetSelectedListBoxItems()) {
				MarkTodoAsComplete(item);
			}
			RefreshAll();
		});
		public ICommand DebugPrintTodoCommand => new RelayCommand(() => {
			foreach (TodoItem item in GetSelectedListBoxItems()) {
				Log.Debug($"{item}");
			}
		});

		public ICommand RankDownCommand => new RelayCommand<TodoItemHolder>(ih => {
			var list = DisplayedItems.Cast<TodoItemHolder>()
			   .OrderBy(h => h.Rank)
			   .ToList();
			if (ih != null) {
				int visibleIndex = list.IndexOf(ih);
				if (visibleIndex >= list.Count - 1) {
					return;
				}

				TodoItemHolder nextItem = list.ElementAt(visibleIndex + 1);

				ih.Rank++;
				nextItem.Rank--;
			}
			RefreshAll();
		});
		public ICommand RankUpCommand => new RelayCommand<TodoItemHolder>(ih => {
			var list = DisplayedItems.Cast<TodoItemHolder>()
			   .OrderBy(h => h.Rank)
			   .ToList();
			if (ih != null) {
				int visibleIndex = list.IndexOf(ih);
				if (visibleIndex <= 0) {
					return;
				}

				TodoItemHolder prevItem = list.ElementAt(visibleIndex - 1);

				ih.Rank--;
				prevItem.Rank++;
			}
			RefreshAll();
		});
		public ICommand ChangeSeverityCommand => new RelayCommand<TodoItemHolder>(ih => {
			if (ih != null) ih.Severity = (ih.Severity + 1) % 4;
			RefreshAll();
		});
		public ICommand ToggleTimerCommand => new RelayCommand<TodoItemHolder>(ih => {
			if (ih != null) ih.TD.IsTimerOn = !ih.IsTimerOn;
			RefreshAll();
		});

		public ICommand SelectTagCommand => new RelayCommand<FilterButton>(button => { CurrentFilter = button.Filter is null or "All" ? "All" : button.Filter; });
		public ICommand SelectSortCommand => new RelayCommand<string>(sort => { CurrentSort = sort; });
		public ICommand CycleSeverityFilterCommand => new RelayCommand(() => { CurrentSeverityFilter++; });
		public ICommand CycleNewTodoSeverityCommand => new RelayCommand(() => { NewTodoSeverity++; });
		public ICommand EditFilterBarCommand => new RelayCommand(EditFilterBarRelayCommand);
		public ICommand NewTodoAddCommand => new RelayCommand(() => {
			TodoItem item = new TodoItem() { Todo = NewTodoText, Severity = NewTodoSeverity };
			item.DateTimeStarted = DateTime.Now;
			ExpandHashTags(item);
			if (CurrentFilter != "All") {
				item.Tags.Add(CurrentFilter);
			}
			AddItemToMasterList(item);
			RefreshAll();
			NewTodoText = "";
		});
		public ICommand AddAndCompleteCommand => new RelayCommand(AddAndComplete);
		public void AddAndComplete() {
			TodoItem item = new TodoItem() { Todo = NewTodoText, Severity = NewTodoSeverity };
			item.DateTimeStarted = DateTime.Now;
			ExpandHashTags(item);
			if (CurrentFilter != "All") {
				item.Tags.Add(CurrentFilter);
			}
			AddItemToMasterList(item);
			MarkTodoAsComplete(item);
			RefreshAll();
			NewTodoText = "";
		}
		public ICommand RefreshAllCommand => new RelayCommand(() => { RefreshAll(); });

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}