using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Echoslate.ViewModels {
	public enum IncrementMode {
		None,
		Major,
		Minor,
		Build,
		Revision
	}

	public class HistoryViewModel : INotifyPropertyChanged {
		private AppData Data { get; set; }
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
		public bool CanCommit => IsCurrentSelected && CurrentHistoryItem.CompletedTodoItems.Any() && CurrentHistoryItem.Title != "";

		private HistoryItem _selectedHistoryItem;
		public HistoryItem SelectedHistoryItem {
			get => _selectedHistoryItem;
			set {
				if (_selectedHistoryItem != value) {
					_selectedHistoryItem = value;
					UpdateCategorizedLists();
					OnPropertyChanged(nameof(Title));
					OnPropertyChanged(nameof(Notes));
				}
			}
		}
		public string Title {
			get => SelectedHistoryItem.Title;
			set {
				if (SelectedHistoryItem.Title != value) {
					SelectedHistoryItem.Title = value;
					OnPropertyChanged();
					SelectedHistoryItem.GenerateCommitMessage();
				}
			}
		}
		public string Notes {
			get => SelectedHistoryItem.Notes;
			set {
				if (SelectedHistoryItem.Notes != value) {
					SelectedHistoryItem.Notes = value;
					OnPropertyChanged();
					SelectedHistoryItem.GenerateCommitMessage();
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

		private IncrementMode _selectedIncrementMode = IncrementMode.None;
		public IncrementMode SelectedIncrementMode {
			get => _selectedIncrementMode;
			set {
				_selectedIncrementMode = value;
				OnPropertyChanged();
			}
		}


		public HistoryViewModel() {
			CurrentHistoryItem = new HistoryItem { Title = "Work in progress...", Version = new Version(0, 0, 0, 0) };
			_todoList = [];
			_allHistoryItems = [];
			SelectedHistoryItem = CurrentHistoryItem;
			BugsCompleted = [];
			FeaturesCompleted = [];
			OtherCompleted = [];
		}
		public void Initialize(MainWindowViewModel mainWindowVM) {
			Data = mainWindowVM.Data;
			SelectedIncrementMode = mainWindowVM.Data.FileSettings.IncrementMode;
			_todoList = mainWindowVM.MasterTodoItemsList;
			_allHistoryItems = mainWindowVM.MasterHistoryItemsList;
			CommittedHistoryItems = CollectionViewSource.GetDefaultView(_allHistoryItems);
			LoadData();
			OnPropertyChanged(nameof(CommittedHistoryItems));

			CurrentHistoryItem = mainWindowVM.CurrentHistoryItem;
			CurrentHistoryItem.CompletedTodoItems.CollectionChanged += (s, e) => UpdateCategorizedLists();

			foreach (var h in _allHistoryItems) {
				foreach (var todoItem in h.CompletedTodoItems) {
					todoItem.CurrentView = View.History;
				}
			}
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
			OnPropertyChanged(nameof(IsCurrentSelected));
			OnPropertyChanged(nameof(CanCommit));
			OnPropertyChanged(nameof(BugsCount));
			OnPropertyChanged(nameof(BugsCompleted));
			OnPropertyChanged(nameof(FeaturesCount));
			OnPropertyChanged(nameof(FeaturesCompleted));
			OnPropertyChanged(nameof(OtherCount));
			OnPropertyChanged(nameof(OtherCompleted));
			SelectedHistoryItem.SortCompletedTodoItems();
			SelectedHistoryItem.GenerateCommitMessage();
		}
		public ICommand CommitCommand => new RelayCommand(CommitCurrent);
		public void CommitCurrent() {
			CurrentHistoryItem.IsCommitted = true;
			CurrentHistoryItem.CommitDate = DateTime.Now;
			CurrentHistoryItem.CompletedTodoItems.CollectionChanged -= (s, e) => UpdateCategorizedLists();

			CurrentHistoryItem.GenerateCommitMessage();
			CopyCommitMessage();
			CurrentHistoryItem = new HistoryItem { Title = "Work in progress", Version = IncrementVersion(CurrentHistoryItem.Version, SelectedIncrementMode) };
			CurrentHistoryItem.CompletedTodoItems.CollectionChanged += (s, e) => UpdateCategorizedLists();

			_allHistoryItems.Insert(0, CurrentHistoryItem);
			SelectedHistoryItem = CurrentHistoryItem;
		}
		public Version IncrementVersion(Version currentVersion, IncrementMode mode) {
			return mode switch {
				IncrementMode.Major => new Version(currentVersion.Major + 1, currentVersion.Minor, currentVersion.Build, currentVersion.Revision),
				IncrementMode.Minor => new Version(currentVersion.Major, currentVersion.Minor + 1, currentVersion.Build, currentVersion.Revision),
				IncrementMode.Build => new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build + 1, currentVersion.Revision),
				IncrementMode.Revision => new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build, currentVersion.Revision + 1),
				_ => currentVersion
			};
		}
		public ICommand CopyCommitMessageCommand => new RelayCommand(CopyCommitMessage);
		public void CopyCommitMessage() {
			if (SelectedHistoryItem?.FullCommitMessage != null) {
				Clipboard.SetText(SelectedHistoryItem.FullCommitMessage);
			}
		}
		public ICommand ReactivateTodoCommand => new RelayCommand<TodoItemHolder>(ReactivateTodo);
		public void ReactivateTodo(TodoItemHolder ih) {
			Log.Test();
			TodoItem item = ih.TD;
			item.IsComplete = false;
			if (CurrentHistoryItem.CompletedTodoItems.Contains(item)) {
				CurrentHistoryItem.CompletedTodoItems.Remove(item);
			}
			_todoList.Add(item);
		}
		public ICommand EditTodoCommand => new RelayCommand<TodoItemHolder>(EditTodo);
		public void EditTodo(TodoItemHolder ih) {
			TodoItem item = ih.TD;
			DlgTodoItemEditor dlg = new DlgTodoItemEditor(item, null, new ObservableCollection<string>(Data.AllTags));
			dlg.ShowDialog();

			if (dlg.Result) {
				TodoItem newItem = dlg.ResultTodoItem;
				if (CurrentHistoryItem.CompletedTodoItems.Contains(item)) {
					CurrentHistoryItem.CompletedTodoItems.Remove(item);
				}
				CurrentHistoryItem.CompletedTodoItems.Add(newItem);
			}
		}
		public ICommand SelectIncrementCommand => new RelayCommand(() => {
			var values = Enum.GetValues(typeof(IncrementMode)).Cast<IncrementMode>();
			int currentIndex = Array.IndexOf(values.ToArray(), SelectedIncrementMode);
			int nextIndex = (currentIndex + 1) % values.Count();

			SelectedIncrementMode = values.ElementAt(nextIndex);
		});

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