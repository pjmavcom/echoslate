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
	public partial class DlgTodoMultiItemEditor : INotifyPropertyChanged {
		public List<string> ResultTags;
		public string ResultTodo;
		public int ResultRank;
		public int ResultSeverity;
		
		private string _todo;
		public string Todo {
			get => _todo;
			set {
				_todo = value;
				OnPropertyChanged();
			}
		}

		public bool IsSeverityEnabled { get; set; }
		public bool IsRankEnabled { get; set; }
		public bool IsCompleteEnabled { get; set; }
		public bool IsTodoEnabled { get; set; }
		public bool IsTagEnabled { get; set; }

		public ObservableCollection<TagHolder> _tags;
		public ObservableCollection<TagHolder> Tags {
			get => _tags;
			set {
				_tags = value;
				OnPropertyChanged();
			}
		}
		private int _rank;
		public int Rank {
			get => _rank;
			set {
				_rank = value;
				OnPropertyChanged();
			}
		}
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
		
		private readonly string _currentFilter;

		public string CompleteButtonContent { get; set; }

		private List<TodoItem> items;
		public readonly List<string> CommonTags;

		public DlgTodoMultiItemEditor(List<TodoItem> items, string currentFilter) {
			InitializeComponent();
			DataContext = this;

			CommonTags = GetCommonTags(items);

			_currentFilter = currentFilter;
			_currentSeverity = items[0].Severity;
			if (items[0].CurrentView == View.Kanban) {
				_rank = items[0].KanbanRank;
			} else if (items[0].CurrentView == View.TodoList) {
				_rank = items[0].Rank[_currentFilter];
			}

			CurrentSeverity = _currentSeverity;
			CompleteButtonContent = items[0].IsComplete ? "Reactivate" : "Complete";

			_tags = new ObservableCollection<TagHolder>();
			foreach (string tag in CommonTags) {
				_tags.Add(new TagHolder(tag));
			}

			CenterWindowOnMouse();
		}
		private static List<string> GetCommonTags(List<TodoItem> items) {
			return new(items.Select(x => x.Tags ?? Enumerable.Empty<string>()).Aggregate((a, b) => a.Intersect(b).ToList()));
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
		private void SetResult() {
			if (IsTagEnabled) {
				ResultTags = new List<string>();
				foreach (TagHolder th in _tags) {
					string tag = th.Text.ToUpper();
					if (!ResultTags.Contains(tag))
						ResultTags.Add(tag);}
			}
			if (IsSeverityEnabled) {
				ResultSeverity = CurrentSeverity;
			}
			if (IsRankEnabled) {
				ResultRank = Rank;
			}
			if (IsTodoEnabled) {
				ResultTodo = Todo;
			}
		}
		private void Delete(TagHolder th) {
			_tags.Remove(th);
		}
		private void AddTag() {
			string name = "#NEWTAG";
			int tagNumber = 0;
			bool nameExists = false;
			do {
				foreach (TagHolder t in _tags) {
					if (t.Text == name.ToUpper() + tagNumber ||
						t.Text == "#" + name.ToUpper() + tagNumber) {
						tagNumber++;
						nameExists = true;
						break;
					}
					nameExists = false;
				}
			} while (nameExists);

			TagHolder th = new TagHolder(name + tagNumber);
			_tags.Add(th);
		}
		private void Ok() {
			SetResult();

			Close();
		}
		private void Complete() {
			SetResult();

			Close();
		}
		private void Cancel() {
			IsTagEnabled = false;
			IsSeverityEnabled = false;
			IsRankEnabled = false;
			IsTodoEnabled = false;
			IsCompleteEnabled = false;
			
			Close();
		}
		private void RankToTop() {
			Rank = 0;
		}
		private void RankToBottom() {
			Rank = int.MaxValue;
		}
		public void CycleSeverity() {
			CurrentSeverity++;
		}
		
		public ICommand CycleSeverityCommand => new RelayCommand(CycleSeverity);
		public ICommand CancelCommand => new RelayCommand(Cancel);
		public ICommand RankToTopCommand => new RelayCommand(RankToTop);
		public ICommand RankToBottomCommand => new RelayCommand(RankToBottom);
		public ICommand CompleteCommand => new RelayCommand(Complete);
		public ICommand OkCommand => new RelayCommand(Ok);
		public ICommand AddTagCommand => new RelayCommand(AddTag);
		public ICommand DeleteTagCommand => new RelayCommand<TagHolder>(Delete);

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}