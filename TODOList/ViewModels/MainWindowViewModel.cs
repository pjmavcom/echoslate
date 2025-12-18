using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Echoslate.ViewModels {
	public class MainWindowViewModel : INotifyPropertyChanged {
		public TodoListViewModel TodoListVM { get; }
		public KanbanViewModel KanbanVM { get; }
		public HistoryViewModel HistoryVM { get; }

		private readonly List<TodoItem> _masterTodoItemsList;
		public List<TodoItem> MasterTodoItemsList {
			get => _masterTodoItemsList;
			private init {
				_masterTodoItemsList = value;
				OnPropertyChanged();
			}
		}
		private readonly List<HistoryItem> _masterHistoryItemsList;
		public List<HistoryItem> MasterHistoryItemsList {
			get => _masterHistoryItemsList;
			private init {
				_masterHistoryItemsList = value;
				OnPropertyChanged();
			}
		}
		private readonly List<string> _masterFilterTags;
		public List<string> MasterFilterTags {
			get => _masterFilterTags;
			private init {
				_masterFilterTags = value;
				OnPropertyChanged();
			}
		}


		public MainWindowViewModel(Settings settings) {
			TodoListVM = new TodoListViewModel();
			KanbanVM = new KanbanViewModel();
			HistoryVM = new HistoryViewModel();

			if (File.Exists(settings.RecentFiles[0])) {
				Log.Print($"Loading recent file {settings.RecentFiles[0]}");
				Load saveFile = new Load(settings.RecentFiles[0]);
				settings.SortRecentFiles(settings.RecentFiles[0]);
				
				MasterTodoItemsList = saveFile.MasterList;
				MasterHistoryItemsList = saveFile.HistoryItems;
				MasterFilterTags = saveFile.FilterTags;
			} else {
				Log.Error($"{settings.RecentFiles[0]} does not exist.");
				settings.RecentFiles.RemoveAt(0);
				
				MasterTodoItemsList = [];
				MasterHistoryItemsList = [];
				MasterFilterTags = [];
			}
			
			TodoListVM.Initialize(this);
			KanbanVM.Initialize(this);
			HistoryVM.Initialize(this);
		}


		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged([CallerMemberName] string? name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}