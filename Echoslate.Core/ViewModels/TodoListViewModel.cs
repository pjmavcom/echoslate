using System.Collections.ObjectModel;
using Echoslate.Core.Components;
using Echoslate.Core.Models;
using Echoslate.Core.Services;

namespace Echoslate.Core.ViewModels;

public class TodoListViewModel : TodoDisplayViewModelBase {
	protected override void RefreshFilter() {
		FilterList.Clear();

		FilterList.Add("All");
		FilterList.Add("#OTHER");
		FilterList.Add("#BUG");
		FilterList.Add("#FEATURE");

		foreach (string filter in MasterFilterTags) {
			if (filter == "All") {
				continue;
			}
			string newFilter = "#" + filter.ToUpper();
			if (FilterList.Contains(newFilter)) {
				continue;
			}
			FilterList.Add(newFilter);
		}
		FilterButtons.Clear();
		List<TodoItem> otherList = MasterList.ToList();
		foreach (string filter in FilterList) {
			int count = 0;
			if (filter == "All") {
				FilterButtons.Add(new FilterButton(filter, MasterList.Count, SelectTagCommand));
				continue;
			}
			foreach (TodoItem item in MasterList) {
				if (item.Tags.Contains(filter)) {
					count++;
					if (otherList.Contains(item)) {
						otherList.Remove(item);
					}
				}
			}
			FilterButtons.Add(new FilterButton(filter, count, SelectTagCommand));
		}
		FilterButtons[1].Count = otherList.Count;
		OnPropertyChanged(nameof(FilterButtons));
	}
	protected override bool MatchFilter(ObservableCollection<string> filterList, TodoItem ih) {
		if (CurrentFilter == "#OTHER") {
			foreach (string tag in FilterList) {
				if (ih.HasTag(tag)) {
					return false;
				}
			}
			return true;
		}
		return CurrentFilter == "All" || CurrentFilter == null || ih.HasTag(CurrentFilter);
	}
	protected override void RefreshAllItems() {
		foreach (TodoItem item in MasterList) {
			item.CurrentView = View.TodoList;
			item.CurrentFilter = CurrentFilter;
		}
	}
	public override void NewTodoAdd() {
		TodoItem item = new TodoItem() {
			Todo = NewTodoText,
			Severity = NewTodoSeverity
		};
		item.DateTimeStarted = DateTime.Now;
		ExpandHashTags(item);
		if (CurrentFilter != "All" && CurrentFilter != "Other") {
			item.AddTag(CurrentFilter);
		}
		AddItemToMasterList(item);
		SelectedTodoItemId = item.Id;
		CleanAllTodoHashRanks();
		RefreshAll();
		NewTodoText = "";
	}
	public override void FixRanks() {
		if (DisplayedItems == null) {
			return;
		}
		var orderedForRanking = DisplayedItems
		   .OrderBy(i => i.CurrentFilterRank)
		   .ToList();
		for (int i = 0; i < orderedForRanking.Count; i++) {
			orderedForRanking[i].CurrentFilterRank = i + 1;
		}
	}
}