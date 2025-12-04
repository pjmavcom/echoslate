
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

// using System.Windows.Input;

namespace TODOList.ViewModels {
	public class TodoListViewModel : INotifyPropertyChanged {
		public ObservableCollection<TodoItemHolder> AllItems { get; }
		public ICollectionView DisplayedItems { get; }
		public ObservableCollection<string> AvailableTags { get; } = new();
		public ObservableCollection<string> AllTags { get; }

		private string _currentTagFilter = null;
		public string CurrentTagFilter {
			get => _currentTagFilter;
			set {
				_currentTagFilter = value;
				DisplayedItems?.Refresh();
				OnPropertyChanged();
				OnPropertyChanged(nameof(CurrentTagFilter));
			}
		}
		

		public TodoListViewModel(ObservableCollection<TodoItemHolder> allItems, ObservableCollection<string> allTags) {
			AllItems = allItems ?? throw new ArgumentNullException(nameof(allItems));
			AllTags = allTags ?? throw new ArgumentNullException(nameof(allTags));
			
			DisplayedItems = CollectionViewSource.GetDefaultView(AllItems);
			DisplayedItems.Filter = FilterByTag;
			RefreshAvailableTags();
		}
		public ICommand SelectTagCommand => new RelayCommand<string>(tag => {
			CurrentTagFilter = tag == "All" ? null : tag;
		});
		public void RefreshAvailableTags() {
			HashSet<string> tags = new HashSet<string> { "All" };

			foreach (TodoItemHolder item in AllItems) {
				if (!string.IsNullOrWhiteSpace(item.FirstTag)) {
					tags.Add(item.FirstTag);
				}
			}

			AvailableTags.Clear();
			foreach (string tag in tags.OrderBy(t => t == "All" ? 0 : 1).ThenBy(t => t)) {
				AvailableTags.Add(tag);
			}
		}
		private bool FilterByTag(object item) {
			if (item is not TodoItemHolder holder) {
				return false;
			}
			return CurrentTagFilter == null || holder.HasTag(CurrentTagFilter);
		}
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}