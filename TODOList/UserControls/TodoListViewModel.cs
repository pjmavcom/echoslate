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
		public static Dictionary<string, string> HashShortcuts;

		public ListBox lbTodos;

		public ObservableCollection<string> AllTags { get; }
		public List<string> MasterFilterTags { get; set; }
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

		private static SolidColorBrush SeverityBrush(int severity) =>
			severity switch {
				3 => new SolidColorBrush(Color.FromRgb(190, 0, 0)), // High = Red
				2 => new SolidColorBrush(Color.FromRgb(200, 160, 0)), // Med = Yellow/Orange
				1 => new SolidColorBrush(Color.FromRgb(0, 140, 0)), // Low = Green
				0 => new SolidColorBrush(Color.FromRgb(50, 50, 50)), // Off = Dark gray (your normal tag color)
				_ => new SolidColorBrush(Color.FromRgb(25, 25, 25)) // Off = Dark gray (your normal tag color)
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
				RefreshDisplayedItems();
				OnPropertyChanged();
			}
		}
		private SolidColorBrush _currentSeverityBrush;
		public SolidColorBrush CurrentSeverityBrush {
			get => _currentSeverityBrush;
			set {
				_currentSeverityBrush = value;
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
		private bool _reverseSort;
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


		public TodoListViewModel(List<TodoItem> masterList, List<string> masterFilterTags, Dictionary<string, string> hashShortcuts) {
			MasterList = masterList ?? throw new ArgumentNullException(nameof(masterList));
			AllItems = new List<TodoItemHolder>();

			MasterFilterTags = masterFilterTags ?? throw new ArgumentNullException(nameof(masterFilterTags));
			AllTags = new ObservableCollection<string>(MasterFilterTags);
			FilterTags = new ObservableCollection<string>(MasterFilterTags);
			
			HashShortcuts = hashShortcuts ?? throw new ArgumentNullException(nameof(hashShortcuts));
			
			CurrentSeverityFilter = -1;
			NewTodoSeverity = 0;
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
			RefreshAvailableTags();
			RefreshDisplayedItems(true);
		}
		public void CycleSeverity() {
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
			DlgTodoItemEditor dlg = new DlgTodoItemEditor(item, GetCurrentTagFilterWithoutHash());
			dlg.ShowDialog();

			Log.Debug($"{item}");
			Log.Debug($"{dlg.ResultTodoItem}");

			if (dlg.Result) {
				RemoveItemFromMasterList(item);
				AddItemToMasterList(dlg.ResultTodoItem);
				ReRankWithSubsetMoved(dlg.ResultTodoItem, dlg.Rank);
				RefreshDisplayedItems(true);
				// if (MasterList.Contains(item)) {
				// MasterList.Remove(item);

				// TODO: needs to get all instances of the todo from _currentHistoryItem.CompletedTodos, CompletedBugs, CompletedFeatures
				// TODO: check that ranks work as intended
				// if (MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodos.Contains(td))
				// MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodos.Remove(td);

				// if (MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodosBugs.Contains(td))
				// MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodosBugs.Remove(td);

				// if (MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodosFeatures.Contains(td))
				// MainWindow.GetActiveWindow()._currentHistoryItem.CompletedTodosFeatures.Remove(td);

				// MainWindow.GetActiveWindow().AddItemToMasterList(itemEditor.ResultTodoItem);
				// AutoSave();
				// }

				// IncompleteItemsRefresh();
				// KanbanRefresh();
				// RefreshHistory();
			}
		}
		private void CleanTodoHashRanks(TodoItem td) {
			List<string> remove = (from pair in td.Rank where !MasterFilterTags.Contains(pair.Key) select pair.Key).ToList();
			foreach (string hash in remove)
				td.Rank.Remove(hash);
			foreach (string name in MasterFilterTags.Where(name => !td.Rank.ContainsKey(name)))
				td.Rank.Add(name, -1);
		}
		public void AddItemToMasterList(TodoItem td) {
			if (MasterListContains(td) >= 0)
				return;
			CleanTodoHashRanks(td);
			MasterList.Add(td);
		}
		public void RemoveItemFromMasterList(TodoItem? td) {
			if (td == null) {
				Log.Warn("item is null");
				return;
			}
			int index = MasterListContains(td);
			if (index == -1)
				return;
			MasterList.RemoveAt(index);
		}
		private int MasterListContains(TodoItem td) {
			if (MasterList.Contains(td))
				return MasterList.IndexOf(td);
			return -1;
		}
		public string GetCurrentTagFilterWithoutHash() {
			string result = CurrentTagFilter;
			if (CurrentTagFilter.Contains("#")) {
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

		private static void ExpandHashTags(TodoItem td) {
			// UNCHECKED
			string tempTodo = ExpandHashTagsInString(td.Todo);
			string tempTags = ExpandHashTagsInList(td.Tags);
			td.Tags = new ObservableCollection<string>();

			td.Todo = tempTags.Trim() + " " + tempTodo.Trim();
		}
		public static string ExpandHashTagsInString(string todo) {
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

					foreach (string hash in from pair in HashShortcuts
											where t.Equals("#" + pair.Key.ToUpper())
											select "#" + pair.Value)
						s = hash;

					s = s.ToLower();
				}

				list.Add(s);
			}

			return list.Where(s => s != "").Aggregate("", (current, s) => current + (s + " "));
		}
		private static string ExpandHashTagsInList(ObservableCollection<string> tags) {
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
					item.IsComplete = true;
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
			foreach (TodoItemHolder ih in lbTodos.SelectedItems) {
				items.Add(ih.TD);
			}
			return items;
		}
		public List<TodoItemHolder> GetSelectedListBoxHolders() {
			List<TodoItemHolder> ihs = [];
			foreach (TodoItemHolder ih in lbTodos.SelectedItems) {
				ihs.Add(ih);
			}
			return ihs;
		}
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
				item.IsComplete = true;
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
			int visibleIndex = list.IndexOf(ih);
			if (visibleIndex >= list.Count - 1) {
				return;
			}

			TodoItemHolder nextItem = list.ElementAt(visibleIndex + 1);

			ih.Rank++;
			nextItem.Rank--;
			RefreshAll();
		});
		public ICommand RankUpCommand => new RelayCommand<TodoItemHolder>(ih => {
			var list = DisplayedItems.Cast<TodoItemHolder>()
			   .OrderBy(h => h.Rank)
			   .ToList();
			int visibleIndex = list.IndexOf(ih);
			if (visibleIndex <= 0) {
				return;
			}

			TodoItemHolder prevItem = list.ElementAt(visibleIndex - 1);

			ih.Rank--;
			prevItem.Rank++;
			RefreshAll();
		});
		public ICommand ChangeSeverityCommand => new RelayCommand<TodoItemHolder>(ih => {
			ih.Severity = (ih.Severity + 1) % 4;
			RefreshAll();
		});
		public ICommand ToggleTimerCommand => new RelayCommand<TodoItemHolder>(ih => {
			ih.TD.IsTimerOn = !ih.IsTimerOn;
			RefreshAll();
		});
		public void NewTodoAdd() {
		}

		public ICommand SelectTagCommand => new RelayCommand<string>(tag => { CurrentTagFilter = tag is null or "All" ? "All" : tag; });
		public ICommand SelectSortCommand => new RelayCommand<string>(sort => { CurrentSort = sort; });
		public ICommand CycleSeverityFilterCommand => new RelayCommand(() => { CurrentSeverityFilter++; });
		public ICommand CycleNewTodoSeverityCommand => new RelayCommand(() => { NewTodoSeverity++; });
		public ICommand EditFilterBarCommand => new RelayCommand(EditFilterBarRelayCommand);
		public ICommand NewTodoAddCommand => new RelayCommand(() => {
			TodoItem item = new TodoItem() { Todo = NewTodoText, Severity = NewTodoSeverity };
			ExpandHashTags(item);
			item.Tags.Add(CurrentTagFilter);
			AddItemToMasterList(item);
			RefreshAll();
			NewTodoText = "";
		});

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}