using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
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
				_reverseSort = true;
				RefreshDisplayedItems();
				GetCurrentHashTags();
				OnPropertyChanged();
			}
		}

		// TODO: Change this to RANK after testing
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
		private bool _reverseSort = true;
		private bool _previousReverseSort;

		private int _currentSeverityFilter = -1;
		public int CurrentSeverityFilter {
			get => _currentSeverityFilter;
			set {
				if (value > 3) {
					value = -1;
				}
				_currentSeverityFilter = value;
				RefreshDisplayedItems();
				OnPropertyChanged();
				OnPropertyChanged(nameof(SeverityButtonText));
				OnPropertyChanged(nameof(SeverityButtonBackground));
			}
		}

		private string? _prioritySortTag;
		public string? PrioritySortTag {
			get => _prioritySortTag;
			set {
				if (_prioritySortTag != value) {
					_prioritySortTag = value;
					ApplyPriorityTagSorting();
					OnPropertyChanged();
				}
			}
		}

		public TodoListViewModel(List<ObservableCollection<TodoItemHolder>> allItems, ObservableCollection<string> allTags) {
			AllItems = allItems ?? throw new ArgumentNullException(nameof(allItems));
			FilteredTags = allTags ?? throw new ArgumentNullException(nameof(allTags));
			AllTags = new ObservableCollection<string>(FilteredTags);
			RefreshAvailableTags();
		}
		public string SeverityButtonText => CurrentSeverityFilter switch {
												3 => "High",
												2 => "Med",
												1 => "Low",
												0 => "None",
												_ => ""
											};
		public Brush SeverityButtonBackground => CurrentSeverityFilter switch {
													 3 => new SolidColorBrush(Color.FromRgb(190, 0, 0)), // High = Red
													 2 => new SolidColorBrush(Color.FromRgb(200, 160, 0)), // Med = Yellow/Orange
													 1 => new SolidColorBrush(Color.FromRgb(0, 140, 0)), // Low = Green
													 0 => new SolidColorBrush(Color.FromRgb(50, 50, 50)), // Off = Dark gray (your normal tag color)
													 _ => new SolidColorBrush(Color.FromRgb(25, 25, 25)) // Off = Dark gray (your normal tag color)
												 };
		public void CycleSeverity() {
			Log.Test();
			CurrentSeverityFilter++;
		}
		public void RefreshAvailableTags() {
			if (HashTags == null) {
				HashTags = new ObservableCollection<string>();
			}
			HashTags.Clear();

			HashTags.Add("All");
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
			DisplayedItems.Filter = CombinedFilter;
			ApplySort();

			DisplayedItems?.Refresh();
		}
		private void ApplySort() {
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
					DisplayedItems.SortDescriptions.Add(new SortDescription("Severity", _reverseSort ? ListSortDirection.Ascending : ListSortDirection.Descending));
					break;
				case "active":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Active", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
					break;
				case "kanban":
					DisplayedItems.SortDescriptions.Add(new SortDescription("Kanban", _reverseSort ? ListSortDirection.Descending : ListSortDirection.Ascending));
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
				foreach (string tag in HashTags) {
					if (ih.HasTag(tag)) {
						return false;
					}
				}
				return true;
			}
			return CurrentTagFilter == "#ALL" || CurrentTagFilter == null || ih.HasTag(CurrentTagFilter);
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
		public ICommand SelectTagCommand
			=> new RelayCommand<string>(tag => { CurrentTagFilter = tag == "All" ? null : tag; });
		public ICommand SelectSortCommand
			=> new RelayCommand<string>(sort => { CurrentSort = sort; });
		public ICommand CycleSeverityCommand
			=> new RelayCommand<int>(severity => { CurrentSeverityFilter++; });

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}