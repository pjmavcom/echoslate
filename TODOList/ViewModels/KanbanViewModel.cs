using System.Collections.Generic;
using System.Collections.ObjectModel;
using Echoslate.Components;


namespace Echoslate.ViewModels {
	public class KanbanViewModel : TodoDisplayViewModelBase {
		public KanbanViewModel() {
		}
		public override void Initialize(List<TodoItem> masterList, List<string> masterFilterTags, Dictionary<string, string> hashShortcuts, List<HistoryItem> historyItems) {
			base.Initialize(masterList, masterFilterTags, hashShortcuts, historyItems);
			CurrentFilter = "Current";
			CurrentSort = "severity";
			_reverseSort = false;
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
		protected override bool MatchFilter(ObservableCollection<string> filterList, TodoItemHolder ih) {
			return ih.CurrentKanbanFilter == ih.Kanban;
		}
		protected override bool RefreshAllItems() {
			AllItems.Clear();
			foreach (TodoItem item in MasterList) {
				TodoItemHolder ih = new TodoItemHolder(item);
				ih.CurrentKanbanFilter = GetCurrentKanbanFilter;
				AllItems.Add(ih);
			}

			if (AllItems.Count == 0) {
				Log.Warn("AllItems is empty.");
				return true;
			}
			return false;
		}

		public int GetCurrentKanbanFilter => CurrentFilter switch {
												 "None" => 0,
												 "Backlog" => 1,
												 "Next" => 2,
												 "Current" => 3,
												 _ => 0
											 };
	}
}