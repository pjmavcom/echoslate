using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Echoslate.Components;


namespace Echoslate.ViewModels {
	public class TodoListViewModel : TodoDisplayViewModelBase {
		public TodoListViewModel() {
		}

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
		protected override bool MatchFilter(ObservableCollection<string> filterList, TodoItemHolder ih) {
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
		protected override bool RefreshAllItems() {
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
				return true;
			}
			return false;
		}
	}
}