using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using TODOList.Resources;

// using System.Windows.Input;

namespace TODOList.ViewModels {
	public class TodoListViewModel : INotifyPropertyChanged {
		public List<ObservableCollection<TodoItemHolder>> AllItems { get; }
		private ICollectionView _displayedItems;
		public ICollectionView DisplayedItems {
			get => _displayedItems;
			private set {
				_displayedItems = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<string> AllTags { get; }
		public ObservableCollection<string> FilteredTags { get; }
		public ObservableCollection<string> HashTags { get; set; }


		private string _currentTagFilter = null;
		public string CurrentTagFilter {
			get => _currentTagFilter;
			set {
				_currentTagFilter = value;
				RefreshDisplayedItems();
				GetCurrentHashTags();
				OnPropertyChanged();
			}
		}

		private string _currentSort = "severity";
		public string CurrentSort {
			get => _currentSort;
			set {
				_currentSort = value;
				Log.Test($"{value}");
				RefreshDisplayedItems();
				OnPropertyChanged();
			}
		}
		private string _previousSort = "";
		private bool _reverseSort = true;
		private bool _previousReverseSort;

		private string? _prioritySortTag;
		public string? PrioritySortTag {
			get => _prioritySortTag;
			set {
				if (_prioritySortTag != value) {
					_prioritySortTag = value;
					OnPropertyChanged();
					ApplyPriorityTagSorting(); // ← this runs every time the ComboBox selection changes
				}
			}
		}

		public TodoListViewModel(List<ObservableCollection<TodoItemHolder>> allItems, ObservableCollection<string> allTags) {
			AllItems = allItems ?? throw new ArgumentNullException(nameof(allItems));
			FilteredTags = allTags ?? throw new ArgumentNullException(nameof(allTags));
			AllTags = new ObservableCollection<string>(FilteredTags);
			RefreshAvailableTags();
		}
		public void RefreshAvailableTags() {
			if (HashTags == null) {
				HashTags = new ObservableCollection<string>();
			}
			HashTags.Clear();

			HashTags.Add("#ALL");
			HashTags.Add("#OTHER");
			HashTags.Add("#BUG");
			HashTags.Add("#FEATURE");

			foreach (string tag in FilteredTags) {
				string hash = "#" + tag.ToUpper();
				if (HashTags.Contains(hash)) {
					continue;
				}
				HashTags.Add(hash);
			}
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
		public void RefreshDisplayedItems() {
			if (AllItems[0].Count == 0) {
				Log.Warn("AllItems is empty.");
				return;
			}

			DisplayedItems = CollectionViewSource.GetDefaultView(AllItems[0]);
			DisplayedItems.Filter = FilterByTag;
			ApplySort();
			DisplayedItems?.Refresh();
		}
		private void ApplySort() {
			if (_currentSort == _previousSort && _reverseSort == _previousReverseSort) {
				return;
			}
			if (_currentSort != _previousSort) {
				_reverseSort = false;
				_previousReverseSort = true;
			} else {
				_previousReverseSort = _reverseSort;
				_reverseSort = !_reverseSort;
			}
			_previousSort = _currentSort;

			DisplayedItems.SortDescriptions.Clear();
			switch (CurrentSort) {
				case "date":
					DisplayedItems.SortDescriptions.Add(new SortDescription("StartDateTime", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "rank":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Rank", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "severity":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Severity", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "active":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Active", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "kanban":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Kanban", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
			}
		}
		private bool FilterByTag(object item) {
			if (item is not TodoItemHolder holder) {
				return false;
			}
			if (CurrentTagFilter == "#OTHER") {
				foreach (string tag in HashTags) {
					if (holder.HasTag(tag)) {
						return false;
					}
				}
				return true;
			}
			return CurrentTagFilter == "#ALL" || CurrentTagFilter == null || holder.HasTag(CurrentTagFilter);
		}
		private void ApplyPriorityTagSorting() {
			DisplayedItems.SortDescriptions.Clear();

			if (DisplayedItems is ListCollectionView lcv) {
				if (!string.IsNullOrEmpty(PrioritySortTag)) {
					lcv.CustomSort = new DynamicPriorityTagComparer(PrioritySortTag);
				} else {
					lcv.CustomSort = null;
				}
			}

			DisplayedItems.SortDescriptions.Add(new SortDescription(nameof(TodoItemHolder.FirstTag), ListSortDirection.Descending));
			DisplayedItems.SortDescriptions.Add(new SortDescription(nameof(TodoItemHolder.Rank), ListSortDirection.Descending));
			DisplayedItems.SortDescriptions.Add(new SortDescription(nameof(TodoItemHolder.StartDateTime), ListSortDirection.Ascending));
		}

		private void HashTags_OnSelectedItem(object sender, SelectionChangedEventArgs e) {
			Log.Test();
		}
		public ICommand SelectTagCommand
			=> new RelayCommand<string>(tag => { CurrentTagFilter = tag == "All" ? null : tag; });
		public ICommand SelectSortCommand
			=> new RelayCommand<string>(sort => { CurrentSort = sort; });
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


	}
}