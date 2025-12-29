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
using Echoslate.Windows;


namespace Echoslate.ViewModels {
	public abstract class TodoDisplayViewModelBase : INotifyPropertyChanged {
		public AppData Data { get; set; }
		public ObservableCollection<TodoItem> MasterList { get; set; }
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
		private ICollectionView _currentVisibleTagsView;
		public ICollectionView CurrentVisibleTagsView {
			get => _currentVisibleTagsView;
			set {
				_currentVisibleTagsView = value;
				OnPropertyChanged();
			}
		}

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

		protected string _currentFilter = "All";
		public string? CurrentFilter {
			get => _currentFilter;
			set {
				_currentFilter = value;
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
		protected bool ReverseSort;

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


		protected Guid SelectedTodoItemId;
		private object _selectedTodoItem;
		public object SelectedTodoItem {
			get => _selectedTodoItem;
			set {
				if (_selectedTodoItem != value) {
					if (_selectedTodoItem is TodoItem item) {
						item.PropertyChanged -= TodoItem_PropertyChanged;
					}
					_selectedTodoItem = value;
					if (_selectedTodoItem is TodoItem newItem) {
						SelectedTodoItemId = newItem.Id;
						Log.Debug($"{SelectedTodoItemId}");
						newItem.PropertyChanged += TodoItem_PropertyChanged;
					}

					OnPropertyChanged();
				}
			}
		}


		public virtual void Initialize(MainWindowViewModel mainWindowVM) {
			Data = mainWindowVM.Data;
			MasterList = mainWindowVM.MasterTodoItemsList;
			HistoryItems = mainWindowVM.MasterHistoryItemsList;
			MasterFilterTags = mainWindowVM.MasterFilterTags ?? throw new ArgumentNullException(nameof(mainWindowVM.MasterFilterTags));

			// AllItems = [];
			FilterButtons = [];
			AllTags = [];

			CurrentVisibleTags = new ObservableCollection<string>(MasterFilterTags);
			FilterList = new ObservableCollection<string>(MasterFilterTags);

			CurrentSeverityFilter = -1;
			NewTodoSeverity = 0;
			RefreshAll();
		}
		protected abstract void RefreshFilter();
		protected abstract bool MatchFilter(ObservableCollection<string> filterList, TodoItem ih);
		protected abstract void RefreshAllItems();

		public void TodoItem_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (sender is TodoItem item) {
				if (item.UpdateTags(Data.AllTags)) {
					UpdateTags();
					RefreshAll();
				}
			}
		}
		public void RebuildView() {
			DisplayedItems = CollectionViewSource.GetDefaultView(MasterList);
		}
		public void GetCurrentHashTags() {
			string currentSortTag = PrioritySortTag;
			CurrentVisibleTags.Clear();
			if (DisplayedItems == null || MasterList == null) {
				return;
			}
			foreach (TodoItem ih in DisplayedItems) {
				foreach (string tag in ih.Tags) {
					if (CurrentVisibleTags.Contains(tag)) {
						continue;
					}
					CurrentVisibleTags.Add(tag);
				}
			}
			CurrentVisibleTagsView = CollectionViewSource.GetDefaultView(CurrentVisibleTags);
			CurrentVisibleTagsView.SortDescriptions.Clear();
			CurrentVisibleTagsView.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
			AllTags.Clear();
			foreach (TodoItem item in MasterList) {
				foreach (string tag in item.Tags) {
					if (!AllTags.Contains(tag)) {
						AllTags.Add(tag);
					}
				}
				LookForNewTags(item);
			}
			OnPropertyChanged(nameof(CurrentVisibleTags));
			if (!string.IsNullOrWhiteSpace(currentSortTag)) {
				Log.Test();
				PrioritySortTag = currentSortTag;
			}
		}
		public abstract void FixRanks();
		public void RefreshDisplayedItems(bool forceRefresh = false) {
			RefreshAllItems();

			DisplayedItems = CollectionViewSource.GetDefaultView(MasterList);
			DisplayedItems.Filter = CombinedFilter;
			FixRanks();

			ApplySort(forceRefresh);

			DisplayedItems?.Refresh();
		}
		public void RefreshAll() {
			RefreshFilter();
			RefreshDisplayedItems(true);
			GetCurrentHashTags();
			RestoreSelection();
		}
		private void RestoreSelection() {
			TodoItem foundItem = null;
			if (SelectedTodoItem == null) {
				foreach (TodoItem ih in MasterList) {
					if (ih.HasId(SelectedTodoItemId)) {
						foundItem = ih;
					}
				}
			}
			SelectedTodoItem = foundItem;
		}
		private void ApplySort(bool forceRefresh = false) {
			if (!forceRefresh) {
				if (_currentSort != _previousSort) {
					ReverseSort = false;
				} else {
					ReverseSort = !ReverseSort;
				}
				_previousSort = _currentSort;
			}

			DisplayedItems.SortDescriptions.Clear();
			switch (CurrentSort) {
				case "date":
					DisplayedItems.SortDescriptions.Add(new SortDescription("DateTimeStarted", ReverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					DisplayedItems.SortDescriptions.Add(new SortDescription("CurrentFilterRank", ReverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "rank":
					DisplayedItems.SortDescriptions.Add(new SortDescription("CurrentFilterRank", ReverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "severity":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Severity", ReverseSort ? ListSortDirection.Ascending : ListSortDirection.Descending));
					DisplayedItems.SortDescriptions.Add(new SortDescription("CurrentFilterRank", ReverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "active":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Active", ReverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					DisplayedItems.SortDescriptions.Add(new SortDescription("CurrentFilterRank", ReverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "kanban":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Kanban", ReverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					DisplayedItems.SortDescriptions.Add(new SortDescription("CurrentFilterRank", ReverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "title":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Todo", ReverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					DisplayedItems.SortDescriptions.Add(new SortDescription("CurrentFilterRank", ReverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
			}
		}
		private bool CombinedFilter(object item) {
			if (item is not TodoItem ih) {
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

			foreach (TodoItem ih in DisplayedItems) {
				if (ih.HasTag(PrioritySortTag)) {
					ih.IsPrioritySorted = true;
				}
			}

			DisplayedItems.SortDescriptions.Add(new SortDescription(nameof(TodoItem.IsPrioritySorted), ListSortDirection.Descending));
			DisplayedItems.SortDescriptions.Add(new SortDescription(nameof(TodoItem.HasTags), ListSortDirection.Descending));
			DisplayedItems.SortDescriptions.Add(new SortDescription(nameof(TodoItem.FirstTag), ListSortDirection.Ascending));
			ResetPrioritySortTags();
		}
		private void ResetPrioritySortTags() {
			foreach (TodoItem ih in DisplayedItems) {
				ih.IsPrioritySorted = false;
			}
		}
		public void MarkSelectedItemAsComplete() {
			if (SelectedTodoItems == null || SelectedTodoItems.Count == 0 || SelectedTodoItems[0] == null) {
				Log.Warn("No todos selected.");
				return;
			}
			TodoItem? ih = SelectedTodoItems[0] as TodoItem;
			TodoItem? item = ih;

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
			var completedItems = MasterList.Where(item => item.IsComplete).ToList();
			foreach (TodoItem item in completedItems) {
				Log.Print($"{item} is complete.");
				RemoveItemFromMasterList(item);
				AddItemToHistory(item);
			}
			RefreshAll();
		}
		public void AddItemToHistory(TodoItem item) {
			item.CurrentView = View.History;
			CurrentHistoryItem = HistoryItems[0];

			var uncommittedHistoryItems = HistoryItems.Where(h => !h.IsCommitted).ToList();

			HistoryItem targetHistoryItem;

			if (uncommittedHistoryItems.Count > 1) {
				var dialog = new ChooseDraftWindow(uncommittedHistoryItems, CurrentHistoryItem);
				if (dialog.ShowDialog() != true) {
					return;
				}

				targetHistoryItem = dialog.Result;
			} else {
				targetHistoryItem = CurrentHistoryItem;	
			}
			targetHistoryItem.AddCompletedTodo(item);
		}
		public void EditItem(TodoItem item) {
			DlgTodoItemEditor dlg = new DlgTodoItemEditor(item, GetCurrentTagFilterWithoutHash(), AllTags);
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
			item.UpdateTags(Data.AllTags);
			MasterList.Add(item);
			LookForNewTags(item);
		}
		public void LookForNewTags(TodoItem item) {
			bool newTagFound = false;
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
			foreach (TodoItem ih in DisplayedItems) {
				allItems.Add(ih);
			}
			var remainingItems = allItems
			   .Where(item => item != subset)
			   .ToList();

			int insertIndex = newRankForSubsetFirstItem - 1;
			insertIndex = Math.Clamp(insertIndex, 0, remainingItems.Count);
			remainingItems.Insert(insertIndex, subset);

			for (int i = 0; i < remainingItems.Count; i++) {
				remainingItems[i].CurrentFilterRank = i + 1;
			}
			RefreshAll();
		}
		public void ReRankWithSubsetMoved(List<TodoItem> subset, int newRankForSubsetFirstItem) {
			List<TodoItem> allItems = new();
			foreach (TodoItem ih in DisplayedItems) {
				allItems.Add(ih);
			}
			var remainingItems = allItems
			   .Where(item => !subset.Contains(item))
			   .ToList();

			int insertIndex = newRankForSubsetFirstItem - 1;
			insertIndex = Math.Clamp(insertIndex, 0, remainingItems.Count);
			remainingItems.InsertRange(insertIndex, subset);

			switch (remainingItems[0].CurrentView) {
				case View.TodoList:
					for (int i = 0; i < remainingItems.Count; i++) {
						remainingItems[i].CurrentFilterRank = i + 1;
					}
					break;
				case View.Kanban:
					for (int i = 0; i < remainingItems.Count; i++) {
						remainingItems[i].KanbanRank = i + 1;
					}
					break;
			}
			RefreshAll();
		}
		protected void ExpandHashTags(TodoItem td) {
			string tempTodo = ExpandHashTagsInString(td.Todo);
			string tempTags = ExpandHashTagsInList(td.Tags);
			td.Todo = tempTags.Trim() + " " + tempTodo.Trim();
		}
		public string ExpandHashTagsInString(string todo) {
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

					s = s.ToLower();
				}

				list.Add(s);
			}

			return list.Where(s => s != "").Aggregate("", (current, s) => current + (s + " "));
		}
		private string ExpandHashTagsInList(ObservableCollection<string> tags) {
			string result = tags.Aggregate("", (current, s) => current + (s + " "));

			result = ExpandHashTagsInString(result);
			return result;
		}
		private void MultiEditItems(List<TodoItem> items) {
			string tagFilter = GetCurrentTagFilterWithoutHash();
			DlgTodoMultiItemEditor dlg = new DlgTodoMultiItemEditor(items, tagFilter);
			dlg.ShowDialog();

			if (dlg.IsRankChangeable) {
				ReRankWithSubsetMoved(items, dlg.ResultRank);
			}
			foreach (TodoItem item in items) {
				if (dlg.IsSeverityChangeable) {
					item.Severity = dlg.ResultSeverity;
				}
				if (dlg.IsTodoChangeable) {
					item.Todo += " " + dlg.ResultTodo;
					Log.Print($"{item.Todo}");
				}
				if (dlg.IsTagChangeable) {
					foreach (string tag in dlg.CommonTags) {
						if (item.Tags.Contains(tag)) {
							item.Tags.Remove(tag);
						}
					}
					foreach (string tag in dlg.ResultTags) {
						item.AddTag(tag);
					}
				}
				if (dlg.IsCompleteChangeable) {
					MarkTodoAsComplete(item);
				}
			}

			RefreshAll();
		}
		public List<TodoItem> GetSelectedListBoxItems() {
			List<TodoItem> items = [];

			foreach (TodoItem ih in SelectedTodoItems) {
				items.Add(ih);
			}
			return items;
		}
		public List<TodoItem> GetSelectedListBoxHolders() {
			List<TodoItem> ihs = [];
			foreach (TodoItem ih in SelectedTodoItems) {
				ihs.Add(ih);
			}
			return ihs;
		}
		public ICommand ContextMenuEditCommand => new RelayCommand(ContextMenuEdit);
		private void ContextMenuEdit() {
			if (SelectedTodoItems[0] is not TodoItem ih) {
				return;
			}

			switch (SelectedTodoItems.Count) {
				case > 1: {
					List<TodoItem> items = [];
					items.AddRange(from TodoItem ihs in SelectedTodoItems select ihs);
					MultiEditItems(items);
					break;
				}
				case 1:
					EditItem(ih);
					break;
				default:
					Log.Error("No items selected.");
					break;
			}
		}
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
			foreach (TodoItem ih in GetSelectedListBoxHolders()) {
				ih.Kanban = 3;
			}
			RefreshAll();
		});
		public ICommand ContextMenuKanban2Command => new RelayCommand(() => {
			foreach (TodoItem ih in GetSelectedListBoxHolders()) {
				ih.Kanban = 2;
			}
			RefreshAll();
		});
		public ICommand ContextMenuKanban1Command => new RelayCommand(() => {
			foreach (TodoItem ih in GetSelectedListBoxHolders()) {
				ih.Kanban = 1;
			}
			RefreshAll();
		});
		public ICommand ContextMenuKanban0Command => new RelayCommand(() => {
			foreach (TodoItem ih in GetSelectedListBoxHolders()) {
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
		public ICommand RankDownCommand => new RelayCommand<TodoItem>(ih => {
			if (ih == null) {
				return;
			}

			var list = DisplayedItems.Cast<TodoItem>()
			   .OrderBy(h => h.CurrentFilterRank)
			   .ToList();
			if (ih != null) {
				int visibleIndex = list.IndexOf(ih);
				if (visibleIndex >= list.Count - 1) {
					return;
				}

				TodoItem nextItem = list.ElementAt(visibleIndex + 1);

				ih.CurrentFilterRank++;
				nextItem.CurrentFilterRank--;
			}
			RefreshAll();
		});
		public ICommand RankUpCommand => new RelayCommand<TodoItem>(ih => {
			if (ih == null) {
				return;
			}

			var list = DisplayedItems.Cast<TodoItem>()
			   .OrderBy(h => h.CurrentFilterRank)
			   .ToList();
			if (ih != null) {
				int visibleIndex = list.IndexOf(ih);
				if (visibleIndex <= 0) {
					return;
				}

				TodoItem prevItem = list.ElementAt(visibleIndex - 1);

				ih.CurrentFilterRank--;
				prevItem.CurrentFilterRank++;
			}
			RefreshAll();
		});
		public ICommand ChangeSeverityCommand => new RelayCommand<TodoItem>(ih => {
			if (ih != null) ih.Severity = (ih.Severity + 1) % 4;
			RefreshAll();
		});
		public ICommand ToggleTimerCommand => new RelayCommand<TodoItem>(ih => {
			if (ih != null) ih.IsTimerOn = !ih.IsTimerOn;
			RefreshAll();
		});

		public ICommand SelectTagCommand => new RelayCommand<FilterButton>(button => { CurrentFilter = button.Filter is null or "All" ? "All" : button.Filter; });
		public ICommand SelectSortCommand => new RelayCommand<string>(sort => { CurrentSort = sort; });
		public ICommand CycleSeverityFilterCommand => new RelayCommand(() => { CurrentSeverityFilter++; });
		public ICommand CycleNewTodoSeverityCommand => new RelayCommand(() => { NewTodoSeverity++; });
		public ICommand EditFilterBarCommand => new RelayCommand(EditFilterBarRelayCommand);
		public void EditFilterBarRelayCommand() {
			DlgEditTabs dlg = new(MasterFilterTags);
			dlg.ShowDialog();
			if (dlg.Result) {
				Log.Print("Filters successfully edited");
				MasterFilterTags.Clear();
				foreach (string filter in dlg.ResultList) {
					MasterFilterTags.Add(filter);
				}
				foreach (TodoItem td in MasterList) {
					CleanTodoHashRanks(td);
				}
				RefreshAll();
			}
		}
		public ICommand NewTodoAddCommand => new RelayCommand(NewTodoAdd);
		public abstract void NewTodoAdd();

		public ICommand AddAndCompleteCommand => new RelayCommand(AddAndComplete);
		public void AddAndComplete() {
			TodoItem item = new TodoItem() { Todo = NewTodoText, Severity = NewTodoSeverity };
			item.DateTimeStarted = DateTime.Now;
			ExpandHashTags(item);
			if (CurrentFilter != "All") {
				item.AddTag(CurrentFilter);
			}
			AddItemToMasterList(item);
			MarkTodoAsComplete(item);
			RefreshAll();
			NewTodoText = "";
		}
		public ICommand RefreshAllCommand => new RelayCommand(RefreshAll);
		public ICommand ChangeSeverityHotkeyCommand => new RelayCommand<string>(s => {
			switch (s) {
				case "up":
					if (NewTodoSeverity >= 3) {
						return;
					}
					NewTodoSeverity++;
					break;
				case "down":
					if (NewTodoSeverity <= 0) {
						return;
					}
					NewTodoSeverity--;
					break;
			}
		});
		public ICommand DebugSearchGuidCommand => new RelayCommand(() => {
			Guid id = new Guid(NewTodoText);
			foreach (TodoItem item in MasterList) {
				var found = item.SearchById(id);
				if (found != null) {
					Log.Print($"Found Todo: {item}");
				}
			}
		});
		public ICommand DebugPrintTodoCommand => new RelayCommand(() => {
			foreach (TodoItem item in GetSelectedListBoxItems()) {
				Log.Debug($"{item}");
			}
		});


		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}