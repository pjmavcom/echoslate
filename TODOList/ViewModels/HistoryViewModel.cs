using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Echoslate.ViewModels {
	public class HistoryViewModel : INotifyPropertyChanged {
		private ObservableCollection<TodoItem> _todoList;
		private ObservableCollection<HistoryItem> _allHistoryItems;

		private HistoryItem _currentHistoryItem;
		public HistoryItem CurrentHistoryItem {
			get => _currentHistoryItem;
			private set {
				SetProperty(ref _currentHistoryItem, value);
				UpdateCategorizedLists();
			}
		}
		public bool IsCurrentSelected => SelectedHistoryItem == CurrentHistoryItem;

		private HistoryItem _selectedHistoryItem;
		public HistoryItem SelectedHistoryItem {
			get => _selectedHistoryItem;
			set {
				if (SetProperty(ref _selectedHistoryItem, value)) {
					UpdateCategorizedLists();
					OnPropertyChanged(nameof(IsCurrentSelected));
				}
			}
		}

		public ICollectionView CommittedHistoryItems { get; set; }

		public ObservableCollection<TodoItemHolder> BugsCompleted { get; } = new();
		public ObservableCollection<TodoItemHolder> FeaturesCompleted { get; } = new();
		public ObservableCollection<TodoItemHolder> OtherCompleted { get; } = new();

		public int BugsCount => BugsCompleted.Count;
		public int FeaturesCount => FeaturesCompleted.Count;
		public int OtherCount => OtherCompleted.Count;

		public ICommand CommitCommand { get; }
		public ICommand CopyCommitMessageCommand { get; }


		public HistoryViewModel() {
			CommitCommand = new RelayCommand(CommitCurrent);//, CanCommit);
			CopyCommitMessageCommand = new RelayCommand(CopyCommitMessage);//, () => SelectedHistoryItem?.IsCommitted == true);
		}
		public void Initialize(MainWindowViewModel mainWindowVM) {
			_todoList = mainWindowVM.MasterTodoItemsList;
			_allHistoryItems = mainWindowVM.MasterHistoryItemsList;
			CommittedHistoryItems = CollectionViewSource.GetDefaultView(_allHistoryItems);
			LoadData();
			OnPropertyChanged(nameof(CommittedHistoryItems));

			CurrentHistoryItem = mainWindowVM.CurrentHistoryItem;
			CurrentHistoryItem.CompletedTodoItems.CollectionChanged += (s, e) => UpdateCategorizedLists();
		}
		public void RebuildView() {
			CommittedHistoryItems = CollectionViewSource.GetDefaultView(_allHistoryItems);
		}

		public void LoadData() {
			foreach (HistoryItem historyItem in _allHistoryItems) {
				historyItem.SortCompletedTodoItems();
			}
			CurrentHistoryItem = _allHistoryItems.FirstOrDefault(h => !h.IsCommitted) ??
								 new HistoryItem { Title = "Work in progressioning.", Version = new Version(3, 40, 40, 1) };
			if (!ReferenceEquals(CurrentHistoryItem, _allHistoryItems.FirstOrDefault())) {
				_allHistoryItems.Insert(0, CurrentHistoryItem);
			}
			SelectedHistoryItem = CurrentHistoryItem;
		}
		public void UpdateCategorizedLists() {
			if (SelectedHistoryItem == null) {
				BugsCompleted.Clear();
				FeaturesCompleted.Clear();
				OtherCompleted.Clear();
				return;
			}

			var completed = SelectedHistoryItem.CompletedTodoItems;

			BugsCompleted.Clear();
			FeaturesCompleted.Clear();
			OtherCompleted.Clear();

			foreach (var todo in completed) {
				if (todo.Tags?.Contains("#BUG", StringComparer.OrdinalIgnoreCase) == true) {
					BugsCompleted.Add(new TodoItemHolder(todo));
				} else if (todo.Tags?.Contains("#FEATURE", StringComparer.OrdinalIgnoreCase) == true) {
					FeaturesCompleted.Add(new TodoItemHolder(todo));
				} else {
					OtherCompleted.Add(new TodoItemHolder(todo));
				}
			}
			OnPropertyChanged(nameof(BugsCount));
			OnPropertyChanged(nameof(BugsCompleted));
			OnPropertyChanged(nameof(FeaturesCount));
			OnPropertyChanged(nameof(FeaturesCompleted));
			OnPropertyChanged(nameof(OtherCount));
			OnPropertyChanged(nameof(OtherCompleted));
		}
		public bool CanCommit() => CurrentHistoryItem?.CompletedTodoItems.Any() == true || !string.IsNullOrWhiteSpace(CurrentHistoryItem?.Notes);
		public void CommitCurrent() {
			// CurrentHistoryItem.Version = IncrementVersion(CurrentHistoryItem.Version, selectedSegment);

			CurrentHistoryItem.IsCommitted = true;
			CurrentHistoryItem.CommitDate = DateTime.Now;

			CurrentHistoryItem.GenerateCommitMessage();
			CopyCommitMessage();

			CurrentHistoryItem = new HistoryItem { Title = "Work in progress", Version = new Version(CurrentHistoryItem.Version.Major, CurrentHistoryItem.Version.Minor, CurrentHistoryItem.Version.Build + 1, 0) };

			_allHistoryItems.Insert(0, CurrentHistoryItem);
			SelectedHistoryItem = CurrentHistoryItem;

			CommandManager.InvalidateRequerySuggested();
		}
		public void CopyCommitMessage() {
			if (SelectedHistoryItem?.FullCommitMessage != null) {
				Clipboard.SetText(SelectedHistoryItem.FullCommitMessage);
			}
		}
		public void ReactivateTodo(TodoItemHolder ih) {
			Log.Test();
			TodoItem item = ih.TD;
			item.IsComplete = false;
			if (CurrentHistoryItem.CompletedTodoItems.Contains(item)) {
				CurrentHistoryItem.CompletedTodoItems.Remove(item);
			}
			_todoList.Add(item);
		}
		public ICommand ReactivateTodoCommand => new RelayCommand<TodoItemHolder>(ReactivateTodo);

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
			if (Equals(field, value)) {
				return false;
			}
			field = value;
			OnPropertyChanged(propertyName);
			return true;
		}
	}
}