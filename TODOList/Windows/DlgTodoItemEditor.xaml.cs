using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;

namespace Echoslate {
	public partial class DlgTodoItemEditor : INotifyPropertyChanged {
		public ObservableCollection<string> SeverityOptions { get; } = new() { "None", "Low", "Med", "High" };
		private int _currentSeverity;
		public int CurrentSeverity {
			get => _currentSeverity;
			set {
				if (value > 3) {
					value = 0;
				}
				_currentSeverity = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(SeverityButtonBackground));
			}
		}
		public Brush SeverityButtonBackground => CurrentSeverity switch {
													 3 => new SolidColorBrush(Color.FromRgb(190, 0, 0)), // High = Red
													 2 => new SolidColorBrush(Color.FromRgb(200, 160, 0)), // Med = Yellow/Orange
													 1 => new SolidColorBrush(Color.FromRgb(0, 140, 0)), // Low = Green
													 0 => new SolidColorBrush(Color.FromRgb(50, 50, 50)), // Off = Dark gray (your normal tag color)
													 _ => new SolidColorBrush(Color.FromRgb(25, 25, 25)) // Off = Dark gray (your normal tag color)
												 };

		private int _rank;
		public int Rank {
			get => _rank;
			set {
				_rank = value;
				OnPropertyChanged();
			}
		}

		private long _timeInMinutes;
		public long TimeInMinutes {
			get => _timeInMinutes;
			set {
				_timeInMinutes = value;
				OnPropertyChanged();
			}
		}

		private int _kanbanId;
		public int KanbanId {
			get => _kanbanId;
			set {
				_kanbanId = value;
				OnPropertyChanged();
			}
		}

		private string _todoText;
		public string TodoText {
			get => _todoText;
			set {
				_todoText = value;
				OnPropertyChanged();
			}
		}

		private string _notes;
		public string Notes {
			get => _notes;
			set {
				_notes = value;
				OnPropertyChanged();
			}
		}

		private readonly TodoItem _todoItem;
		public TodoItem ResultTodoItem => _todoItem;
		public bool Result;

		private List<string> Tags { get; set; }
		private List<TagHolder> _tagHolders;
		public List<TagHolder> TagHolders {
			get => _tagHolders;
			set {
				_tagHolders = value;
				OnPropertyChanged();
			}
		}
		
		private List<string> ResultTags { get; set; }
		public string SelectedTag { get; set; }
		private readonly string _currentListHash;

		private int _previousRank;

		public DlgTodoItemEditor(TodoItem td, string? currentListHash) {
			InitializeComponent();
			DataContext = this;

			_todoItem = new TodoItem(td.ToString()) {
														IsTimerOn = td.IsTimerOn
													};
			_currentListHash = currentListHash ?? "All";
			_previousRank = td.Rank[_currentListHash];

			CurrentSeverity = _todoItem.Severity;
			Rank = _todoItem.Rank[_currentListHash];
			TimeInMinutes = _todoItem.TimeTakenInMinutes;
			KanbanId = _todoItem.Kanban;
			TodoText = _todoItem.Todo;
			Notes = _todoItem.Notes;
			
			Tags = new List<string>(_todoItem.Tags);
			TagHolders = new List<TagHolder>();
			foreach (string tag in Tags) {
				TagHolders.Add(new TagHolder(tag));
			}

			Notes = td.Notes;
			if (Notes.Contains("/n")) {
				Notes = Notes.Replace("/n", Environment.NewLine);
			}

			CenterWindowOnMouse();
		}
		private void CenterWindowOnMouse() {
			Window win = Application.Current.MainWindow;

			if (win == null)
				return;
			double centerX = win.Width / 2 + win.Left;
			double centerY = win.Height / 2 + win.Top;
			Left = centerX - Width / 2;
			Top = centerY - Height / 2;
		}
		private void SetTodo() {
			ResultTodoItem.Severity = CurrentSeverity;
			ResultTodoItem.Rank[_currentListHash] = Rank;
			ResultTodoItem.TimeTakenInMinutes = TimeInMinutes;
			ResultTodoItem.Notes = Notes;
			
			string tempTodo = ExpandHashTagsInString(TodoText);
			string tempTags = "";
			ResultTags = new List<string>();
			foreach (TagHolder th in TagHolders)
				if (!ResultTags.Contains(th.Text))
					ResultTags.Add(th.Text);
			foreach (string tag in ResultTags)
				tempTags += tag + " ";
			tempTags = ExpandHashTagsInString(tempTags);
			
			ResultTodoItem.Tags = new ObservableCollection<string>();
			ResultTodoItem.Todo = tempTags.Trim() + " " + tempTodo.Trim();
		}
		public static string ExpandHashTagsInString(string todo) {
			string[] pieces = todo.Split(' ');

			List<string> list = [];
			foreach (string piece in pieces) {
				string s = piece;
				if (s.Contains('#')) {
					s = s.ToUpper();
					if (s.Equals("#FEATURES"))
						s = "#FEATURE";

					if (s.Equals("#BUGS"))
						s = "#BUG";

					s = s.ToLower();
				}

				list.Add(s);
			}

			return list.Where(s => s != "").Aggregate("", (current, s) => current + (s + " "));
		}
		private void Ok() {
			Result = true;
			SetTodo();

			Close();
		}
		private void Complete() {
			Result = true;
			_todoItem.IsComplete = true;
			SetTodo();

			Close();
		}
		private void Cancel() {
			Close();
		}
		public void CycleSeverity() {
			CurrentSeverity++;
		}
		
		public ICommand CycleSeverityCommand => new RelayCommand(CycleSeverity);
		public ICommand CompleteCommand => new RelayCommand(Complete);
		public ICommand OkCommand => new RelayCommand(Ok);
		public ICommand CancelCommand => new RelayCommand(Cancel);	
		
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}