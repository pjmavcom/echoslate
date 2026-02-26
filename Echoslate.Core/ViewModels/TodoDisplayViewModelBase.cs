using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Components;
using Echoslate.Core.Models;
using Echoslate.Core.Resources;
using Echoslate.Core.Services;

namespace Echoslate.Core.ViewModels;

public abstract class TodoDisplayViewModelBase : INotifyPropertyChanged {
	public AppData Data { get; set; }
	public ObservableCollection<TodoItem> MasterList { get; set; }
	public IEnumerable<TodoItem> DisplayedItems {
		get {
			if (MasterList == null) {
				return null;
			}
			var items = MasterList.Where(item => CombinedFilter(item));
			return CurrentSortMethod(items);
		}
	}

	private bool _showTodoItemEditorOnAdd;
	public bool ShowTodoItemEditorOnAdd {
		get => _showTodoItemEditorOnAdd;
		set {
			if (_showTodoItemEditorOnAdd == value) {
				return;
			}
			_showTodoItemEditorOnAdd = value;
			OnPropertyChanged();
		}
	}
	private Func<IEnumerable<TodoItem>, IOrderedEnumerable<TodoItem>> _currentSortMethod = items => items.OrderByDescending(i => i.CurrentFilterRank);
	public Func<IEnumerable<TodoItem>, IOrderedEnumerable<TodoItem>> CurrentSortMethod {
		get => _currentSortMethod;
		set {
			_currentSortMethod = value ?? (items => items.OrderByDescending(i => i.Rank));
			OnPropertyChanged(nameof(DisplayedItems));
		}
	}

	public ObservableCollection<HistoryItem> HistoryItems { get; set; }
	public HistoryItem CurrentHistoryItem { get; set; }

	public ObservableCollection<FilterButton> FilterButtons { get; set; } = [];

	public ObservableCollection<string> AllTags { get; set; }
	public ObservableCollection<string> CurrentVisibleTags { get; set; }
	public IEnumerable<string> CurrentVisibleTagsView {
		get {
			if (CurrentVisibleTags == null) {
				return null;
			}
			var items = CurrentVisibleTags.OrderBy(t => t, StringComparer.OrdinalIgnoreCase);
			return items;
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
			if (_currentFilter == value) {
				return;
			}
			_currentFilter = value;
			RefreshAll();
			OnPropertyChanged();
		}
	}

	private int _currentSeverityFilter;
	public int CurrentSeverityFilter {
		get => _currentSeverityFilter;
		set {
			if (value > 3) {
				value = -1;
			}
			_currentSeverityFilter = value;
			CurrentSeverityBrush = AppServices.BrushService.GetBrushForSeverity(CurrentSeverityFilter);

			RefreshAll();
			OnPropertyChanged();
		}
	}
	private object _currentSeverityBrush;
	public object CurrentSeverityBrush {
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
			NewTodoSeverityBrush = AppServices.BrushService.GetBrushForSeverity(NewTodoSeverity);
			OnPropertyChanged();
		}
	}
	private object _newTodoSeverityBrush;
	public object NewTodoSeverityBrush {
		get => _newTodoSeverityBrush;
		set {
			_newTodoSeverityBrush = value;
			OnPropertyChanged();
		}
	}
	public ObservableCollection<TodoItem> SelectedTodoItems { get; } = [];


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

		FilterButtons = [];
		AllTags = [];

		CurrentVisibleTags = new ObservableCollection<string>(MasterFilterTags);
		FilterList = new ObservableCollection<string>(MasterFilterTags);

		CurrentSeverityFilter = -1;
		NewTodoSeverity = 0;
		CleanAllTodoHashRanks();
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
		CurrentVisibleTags.Add("-None-");

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
			PrioritySortTag = currentSortTag;
		}
	}
	public abstract void FixRanks();
	public void RefreshDisplayedItems(bool forceRefresh = false) {
		RefreshAllItems();
		FixRanks();
		ApplySort(forceRefresh);
	}
	public void RefreshAll() {
		RefreshFilter();
		RefreshDisplayedItems(true);
		GetCurrentHashTags();
		RestoreSelection();
		UpdateFilterButtonSelection();
	}
	private void UpdateFilterButtonSelection() {
		foreach (FilterButton b in FilterButtons) {
			b.IsSelected = string.Equals(b.Filter ?? "All", _currentFilter ?? "All", StringComparison.OrdinalIgnoreCase);
		}
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

		switch (CurrentSort) {
			case "date":
				CurrentSortMethod = ReverseSort
					? items => items.OrderByDescending(i => i.DateTimeStarted).ThenBy(i => i.CurrentFilterRank)
					: items => items.OrderBy(i => i.DateTimeStarted).ThenBy(i => i.CurrentFilterRank);
				break;
			case "rank":
				CurrentSortMethod = ReverseSort
					? items => items.OrderByDescending(i => i.CurrentFilterRank)
					: items => items.OrderBy(i => i.CurrentFilterRank);
				break;
			case "severity":
				CurrentSortMethod = ReverseSort
					? items => items.OrderBy(i => i.Severity).ThenBy(i => i.CurrentFilterRank)
					: items => items.OrderByDescending(i => i.Severity).ThenBy(i => i.CurrentFilterRank);
				break;
			case "active":
				CurrentSortMethod = ReverseSort
					? items => items.OrderByDescending(i => i.IsTimerOn).ThenBy(i => i.CurrentFilterRank)
					: items => items.OrderBy(i => i.IsTimerOn).ThenBy(i => i.CurrentFilterRank);
				break;
			case "title":
				CurrentSortMethod = ReverseSort
					? items => items.OrderByDescending(i => i.Todo).ThenBy(i => i.CurrentFilterRank)
					: items => items.OrderBy(i => i.Todo).ThenBy(i => i.CurrentFilterRank);
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
		if (PrioritySortTag == "-None-") {
			foreach (TodoItem ih in DisplayedItems) {
				ih.IsPrioritySorted = false;
			}
		}
		foreach (TodoItem ih in DisplayedItems) {
			if (ih.HasTag(PrioritySortTag)) {
				ih.IsPrioritySorted = true;
			}
		}

		CurrentSortMethod = items => items.OrderByDescending(i => i.IsPrioritySorted).ThenByDescending(i => i.HasTags).ThenBy(i => i.FirstTag);
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
	public async void MarkTodoAsComplete(TodoItem item) {
		HistoryItem? result = await AddItemToHistory();
		if (result == null) {
			return;
		}
		item.IsComplete = true;
		item.CurrentView = View.History;
		result.AddCompletedTodo(item);

		RefreshAllItems();
		SortCompleteTodosToHistory();
	}
	public async void MarkMultiTodoAsComplete(List<TodoItem> items) {
		HistoryItem? result = await AddItemToHistory();
		if (result == null) {
			return;
		}
		foreach (TodoItem item in items) {
			item.IsComplete = true;
			item.CurrentView = View.History;
			result.AddCompletedTodo(item);
		}
		RefreshAllItems();
		SortCompleteTodosToHistory();
	}
	public void SortCompleteTodosToHistory() {
		var completedItems = MasterList.Where(item => item.IsComplete).ToList();
		foreach (TodoItem item in completedItems) {
			Log.Print($"{item} is complete.");
			RemoveItemFromMasterList(item);
			// AddItemToHistory(item);
		}
		RefreshAll();
	}
	public async Task<HistoryItem?> AddItemToHistory() {
		CurrentHistoryItem = HistoryItems[0];
		var uncommittedHistoryItems = HistoryItems.Where(h => !h.IsCommitted).ToList();

		if (uncommittedHistoryItems.Count > 1) {
			Task<ChooseDraftViewModel?> vmTask = AppServices.DialogService.ShowChooseDraftAsync(uncommittedHistoryItems, CurrentHistoryItem);
			ChooseDraftViewModel vm = await vmTask;
			if (vm == null) {
				return null;
			}
			return vm.ResultHistoryItem;

			// item.CurrentView = View.History;
			// vm.ResultHistoryItem.AddCompletedTodo(item);
		}
		return CurrentHistoryItem;
	}
	public async void EditItem(TodoItem item) {
		Task<TodoItemEditorViewModel?> vmTask = AppServices.DialogService.ShowTodoItemEditorAsync(item, GetCurrentTagFilterWithoutHash(), AllTags);
		TodoItemEditorViewModel? vm = await vmTask;
		if (vm != null) {
			TodoItem newItem = vm.ResultTodoItem;
			RemoveItemFromMasterList(item);
			AddItemToMasterList(newItem);

			if (newItem.IsComplete) {
				MarkTodoAsComplete(newItem);
			} else {
				ReRankWithSubsetMoved(newItem, vm.Rank);
			}

			CleanAllTodoHashRanks();
			RefreshAll();
		}
	}
	public void CleanAllTodoHashRanks() {
		foreach (TodoItem item in MasterList) {
			foreach (string tag in MasterFilterTags) {
				if (!item.Rank.ContainsKey(tag) && item.HasTag(tag)) {
					if (tag == tag.ToUpper()) {
						Log.Warn("Problem here!");
					}
					item.Rank.Add(tag, -1);
				}
			}
			foreach (string tag in item.Rank.Keys) {
				if (tag == "All") {
					continue;
				}
				if (!MasterFilterTags.Contains(tag) && !item.HasTag(tag)) {
					item.Rank.Remove(tag);
				}
			}
		}
	}
	private void CleanTodoHashRanks(TodoItem td) {
		List<string> remove = (from pair in td.Rank where !MasterFilterTags.Contains(pair.Key) select pair.Key).ToList();
		foreach (string hash in remove) {
			td.Rank.Remove(hash);
		}
		foreach (string name in MasterFilterTags.Where(name => !td.Rank.ContainsKey(name))) {
			if (name == name.ToUpper()) {
				Log.Warn("Problem here!");
			}
			td.Rank.Add(name, -1);
		}
	}
	public void AddItemToMasterList(TodoItem item) {
		if (MasterListContains(item) >= 0) {
			Log.Warn("MasterList already contains Item.");
			return;
		}
		if (item.CurrentFilterRank < 0) {
			CleanTodoHashRanks(item);
		}
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
		var list = DisplayedItems.OrderBy(i => i.CurrentFilterRank).ToList();
		foreach (TodoItem item in list) {
			allItems.Add(item);
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
		var list = DisplayedItems.OrderBy(i => i.CurrentFilterRank).ToList();
		foreach (TodoItem item in list) {
			allItems.Add(item);
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
	private async void MultiEditItems(List<TodoItem> items) {
		string tagFilter = GetCurrentTagFilterWithoutHash();
		Task<TodoMultiItemEditorViewModel?> vmTask = AppServices.DialogService.ShowTodoMultiItemEditorAsync(items, tagFilter, AllTags);
		TodoMultiItemEditorViewModel? vm = await vmTask;
		if (vm == null) {
			return;
		}

		if (vm.IsRankChangeable) {
			ReRankWithSubsetMoved(items, vm.ResultRank);
		}
		foreach (TodoItem item in items) {
			if (vm.IsSeverityChangeable) {
				item.Severity = vm.ResultSeverity;
			}
			if (vm.IsTodoChangeable) {
				item.Todo += " " + vm.ResultTodo;
				Log.Print($"{item.Todo}");
			}
			if (vm.IsTagChangeable) {
				foreach (string tag in vm.CommonTags) {
					if (item.Tags.Contains(tag)) {
						item.Tags.Remove(tag);
					}
				}
				foreach (string tag in vm.ResultTags) {
					item.AddTag(tag);
				}
			}
		}
		if (vm.IsCompleteChangeable) {
			MarkMultiTodoAsComplete(items);
		}
		CleanAllTodoHashRanks();
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
	public ICommand ContextMenuSetSeverityCommand => new RelayCommand<string>(i => {
		foreach (TodoItem item in GetSelectedListBoxItems()) {
			item.Severity = int.Parse(i);
		}
	});
	public ICommand ContextMenuKanbanCommand => new RelayCommand<string>(i => {
		foreach (TodoItem ih in GetSelectedListBoxHolders()) {
			ih.Kanban = int.Parse(i);
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

		var list = Enumerable.Cast<TodoItem>(DisplayedItems)
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

		var list = Enumerable.Cast<TodoItem>(DisplayedItems)
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

	public async void EditFilterBarRelayCommand() {
		Task<EditTabsViewModel> vmTask = AppServices.DialogService.ShowEditTabsAsync(MasterFilterTags);
		EditTabsViewModel? vm = await vmTask;
		if (vm == null) {
			return;
		}
		if (vm.Result) {
			Log.Print("Filters successfully edited");
			MasterFilterTags.Clear();
			foreach (string filter in vm.ResultList) {
				MasterFilterTags.Add(filter);
			}
			CleanAllTodoHashRanks();
			RefreshAll();
		}
	}
	public ICommand NewTodoAddCommand => new RelayCommand(NewTodoAdd);
	public abstract void NewTodoAdd();
	public ICommand AddAndCompleteCommand => new RelayCommand(AddAndComplete);

	public void AddAndComplete() {
		TodoItem item = new TodoItem() {
			Todo = NewTodoText,
			Severity = NewTodoSeverity
		};
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