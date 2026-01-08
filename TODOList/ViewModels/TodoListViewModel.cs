using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Echoslate.Core.Components;
using Echoslate.Core.Models;

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
				FilterButtons.Add(new FilterButton(filter, MasterList.Count));
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
			FilterButtons.Add(new FilterButton(filter, count));
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
		}
	}
	public override void NewTodoAdd() {
		TodoItem item = new TodoItem() { Todo = NewTodoText, Severity = NewTodoSeverity };
		item.DateTimeStarted = DateTime.Now;
		ExpandHashTags(item);
		if (CurrentFilter != "All" && CurrentFilter != "Other") {
			item.AddTag(CurrentFilter);
		}
		AddItemToMasterList(item);
		SelectedTodoItemId = item.Id;
		RefreshAll();
		NewTodoText = "";
	}
	public override void FixRanks() {
		if (DisplayedItems == null) {
			return;
		}
		// DisplayedItems.SortDescriptions.Clear();
		// DisplayedItems.SortDescriptions.Add(new SortDescription("CurrentFilterRank", ListSortDirection.Ascending));
		CurrentSortMethod = items => items.OrderBy(i => i.CurrentFilterRank);
		int index = 1;
		foreach (TodoItem ih in DisplayedItems) {
			ih.CurrentFilterRank = index++;
		}
	}
}