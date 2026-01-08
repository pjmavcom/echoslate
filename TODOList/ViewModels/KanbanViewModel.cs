using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Echoslate.Core.Components;
using Echoslate.Core.Models;

namespace Echoslate.Core.ViewModels {
	public class KanbanViewModel : TodoDisplayViewModelBase {
		public override void Initialize(MainWindowViewModel mainWindowVM) {
			base.Initialize(mainWindowVM);
			CurrentFilter = "Current";
			CurrentSort = "severity";
			ReverseSort = false;
			RefreshAll();
		}
		protected override void RefreshFilter() {
			FilterList.Clear();

			FilterList.Add("None");
			FilterList.Add("Backlog");
			FilterList.Add("Next");
			FilterList.Add("Current");

			FilterButtons.Clear();
			int kanbanIndex = 0;
			foreach (string filter in FilterList) {
				int count = 0;
				foreach (TodoItem item in MasterList) {
					if (item.Kanban == kanbanIndex) {
						count++;
					}
				}
				FilterButtons.Add(new FilterButton(filter, count));
				kanbanIndex++;
			}
			OnPropertyChanged(nameof(FilterButtons));
		}
		protected override bool MatchFilter(ObservableCollection<string> filterList, TodoItem ih) {
			return ih.CurrentKanbanFilter == ih.Kanban;
		}
		protected override void RefreshAllItems() {
			foreach (TodoItem item in MasterList) {
				item.CurrentKanbanFilter = GetCurrentKanbanFilter;
				item.CurrentView = View.Kanban;
			}
		}

		public int GetCurrentKanbanFilter => CurrentFilter switch {
			"None" => 0,
			"Backlog" => 1,
			"Next" => 2,
			"Current" => 3,
			_ => 0
		};
		public override void NewTodoAdd() {
			TodoItem item = new TodoItem() { Todo = NewTodoText, Severity = NewTodoSeverity };
			item.DateTimeStarted = DateTime.Now;
			ExpandHashTags(item);
			item.Kanban = CurrentFilter switch {
				"None" => 0,
				"BackLog" => 1,
				"Next" => 2,
				"Current" => 3,
				_ => 0
			};

			AddItemToMasterList(item);
			SelectedTodoItemId = item.Id;
			RefreshAll();
			NewTodoText = "";
		}
		public override void FixRanks() {
			if (DisplayedItems == null) {
				return;
			}
			DisplayedItems.SortDescriptions.Clear();
			DisplayedItems.SortDescriptions.Add(new SortDescription("KanbanRank", ListSortDirection.Ascending));
			int index = 1;
			foreach (TodoItem ih in DisplayedItems) {
				ih.CurrentFilterRank = index++;
			}
		}
	}
}