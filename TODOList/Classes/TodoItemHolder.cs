/*	TodoListHolder.cs
 * 20-Mar-2019
 * 11:46:32
 *
 *
 */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Echoslate {
	public enum View {
		TodoList,
		Kanban,
		History
	}

	public class TodoItemHolder : INotifyPropertyChanged {
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
		private TodoItem _td;
		private DateTime _timeTaken;
		private long _timeTakenInMinutes;

		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		public TodoItem TD {
			get => _td;
			set => _td = value;
		}
		public Guid Id {
			get => _td.Id;
			set {
				_td.Id = value;
				OnPropertyChanged();
			}
		}

		private int _rank;
		public int Rank {
			get {
				return CurrentView switch {
					View.Kanban => KanbanRank,
					View.TodoList when TD.Rank.ContainsKey(_currentFilter) => TD.Rank[_currentFilter],
					_ => -1
				};
			}
			set {
				switch (CurrentView) {
					case View.Kanban:
						KanbanRank = value;
						break;
					case View.TodoList:
						TD.Rank[_currentFilter] = value;
						break;
				}
				OnPropertyChanged();
			}
		}
		private string _currentFilter = "All";
		public string CurrentFilter {
			get => _currentFilter;
			set {
				_currentFilter = value;
				OnPropertyChanged();
			}
		}
		private View _currentView;
		public View CurrentView {
			get => TD.CurrentView;
			set {
				TD.CurrentView = value;
				
				OnPropertyChanged();
			}
		}

		public int CurrentKanbanFilter { get; set; }

		public DateTime DateTimeStarted => _td.DateTimeStarted;

		public string Todo {
			get => _td.Todo;
			set => _td.Todo = value;
		}
		public string Notes {
			get => _td.Notes;
			set => _td.Notes = value;
		}
		public string Problem {
			get => _td.Problem;
			set => _td.Problem = value;
		}
		public string Solution {
			get => _td.Solution;
			set => _td.Solution = value;
		}
		public string NotesAndTags => _td.NotesAndTags;
		public string TagsSorted => _td.TagsSorted;
		// public string StartDateTime => _td.StartDateTime;
		public ObservableCollection<string> Tags => _td.Tags;
		public string FirstTag => Tags.Count > 0 ? Tags[0] : "";

		private bool _isPrioritySorted;
		public bool IsPrioritySorted {
			get => _isPrioritySorted;
			set {
				_isPrioritySorted = value;
				OnPropertyChanged();
			}
		}

		public bool HasTags => Tags.Count > 0;

		public int Severity {
			get => _td.Severity;
			set => _td.Severity = value;
		}
		public int Kanban {
			get => _td.Kanban;
			set {
				_td.Kanban = value;
				KanbanRank = int.MaxValue;
				OnPropertyChanged();
			}
		}
		public int KanbanRank {
			get => _td.KanbanRank;
			set {
				_td.KanbanRank = value;
				OnPropertyChanged();
			}
		}
		public bool IsTimerOn => _td.IsTimerOn;
		// public long TimeTakenInMinutes => _td.TimeTakenInMinutes;
		private string _timeTakenDisplay;
		public string TimeTakenDisplay {
			get => $"{TimeTaken.Minutes:00.##} : {TimeTaken.Seconds:00}";
		}
		public TimeSpan TimeTaken {
			get => _td.TimeTaken;
			set {
				_td.TimeTaken = value;
				OnPropertyChanged();
			}
		}
		// public string TimeStarted => TD.TimeStarted;
		// public string DateStarted => TD.DateStarted;

		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public TodoItemHolder(TodoItem item) {
			_td = item;
			_td.PropertyChanged += (s, e) => {
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
				if (e.PropertyName is nameof(TodoItem.TimeTaken) or
					// nameof(TodoItem.TimeTakenInMinutes) or
					nameof(TodoItem.IsTimerOn)) {
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeTakenDisplay)));
				}
			};
		}
		public TodoItemHolder? SearchById(Guid id) {
			if (Id == id) {
				return this;
			}
			return null;
		}
		public bool HasId(Guid id) {
			return Id == id;
		}
		public bool HasTag(string tag) {
			return Tags.Contains(tag);
		}
		public void CleanNotes() {
			_td.CleanNotes();
		}

		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS //
		public override string ToString() {
			string result = "";
			result += Rank + " | " + TD.Ranks;
			return result;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}