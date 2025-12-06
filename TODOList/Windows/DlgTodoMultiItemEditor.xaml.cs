using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TODOList {
	public partial class DlgTodoMultiItemEditor : INotifyPropertyChanged {
		// public TodoItem ResultTD => _td;
		public List<string> ResultTags;
		public string ResultTodo;
		public int ResultRank;
		public int ResultSeverity;
		public bool ResultIsComplete;
		
		public bool Result;
		
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
		// private readonly TodoItem _td;
		// private readonly int _previousRank;
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
				_currentSeverity = value;
				OnPropertyChanged();
			}
		}
		private readonly string _currentFilter;

		public string CompleteButtonContent { get; set; }

		private List<TodoItem> items;

		public DlgTodoMultiItemEditor(List<TodoItem> items, string currentFilter) {
			InitializeComponent();
			DataContext = this;

			List<string> commonTags = GetCommonTags(items);

			_currentFilter = currentFilter;
			_currentSeverity = items[0].Severity;
			_rank = items[0].Rank[_currentFilter];

			cbSev.SelectedIndex = _currentSeverity;
			CompleteButtonContent = items[0].IsComplete ? "Reactivate" : "Complete";

			_tags = new ObservableCollection<TagHolder>();
			foreach (string tag in commonTags) {
				_tags.Add(new TagHolder(tag));
			}
			lbTags.ItemsSource = _tags;
			lbTags.Items.Refresh();

			CenterWindowOnMouse();
		}
		private static List<string> GetCommonTags(List<TodoItem> items) {
			return new(items.Select(x => x.Tags ?? Enumerable.Empty<string>()).Aggregate((a, b) => a.Intersect(b).ToList()));
		}
		private static void RemoveNonCommonTags(List<TodoItem> items, List<string> commonTags) {
			foreach (TodoItem item in items) {
				if (!item.Tags.Any()) {
					continue;
				}

				for (int i = item.Tags.Count - 1; i >= 0; i--) {
					if (!commonTags.Contains(item.Tags[i])) {
						item.Tags.RemoveAt(i);
					}
				}
			}
		}
		public DlgTodoMultiItemEditor(TodoItem td, string currentFilter, List<string> tags) {
			InitializeComponent();
			Close();
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
				ResultSeverity = cbSev.SelectedIndex;
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
			lbTags.Items.Refresh();
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
			lbTags.Items.Refresh();
		}
		private void Ok() {
			Result = true;
			SetResult();

			Close();
		}
		private void Complete() {
			Result = true;
			ResultIsComplete = true;
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