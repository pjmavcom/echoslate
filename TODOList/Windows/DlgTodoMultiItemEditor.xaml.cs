using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TODOList {
	public partial class DlgTodoMultiItemEditor {
		public TodoItem ResultTD => _td;
		public List<string> ResultTags;
		public bool Result;
		public bool ResultIsComplete;

		public bool IsSeverityEnabled { get; set; }
		public bool IsRankEnabled { get; set; }
		public bool IsCompleteEnabled { get; set; }
		public bool IsTodoEnabled { get; set; }
		public bool IsTagEnabled { get; set; }

		[ObservableProperty] public ObservableCollection<TagHolder> _tags;
		private readonly TodoItem _td;
		private readonly int _previousRank;
		private int _currentSeverity;
		public int CurrentSeverity {
			get => _currentSeverity;
			set => _currentSeverity = value;
		}
		private int _rank;
		private readonly string _currentFilter;

		private List<TodoItem> items;

		public DlgTodoMultiItemEditor(List<TodoItem> items, string currentFilter) {
			InitializeComponent();
			DataContext = this;

			List<string> commonTags = GetCommonTags(items);
			RemoveNonCommonTags(items, commonTags);

			_td = items[0];
			_currentFilter = currentFilter;
			_currentSeverity = _td.Severity;
			_rank = _td.Rank[_currentFilter];
			_previousRank = _rank;

			cbSev.SelectedIndex = _currentSeverity;
			tbRank.Text = _rank.ToString();
			// btnComplete.Content = _td.IsComplete ? "Reactivate" : "Complete";

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

			_td = new TodoItem(td.ToString()) {
												  IsTimerOn = td.IsTimerOn
											  };
			_currentFilter = currentFilter;
			_currentSeverity = _td.Severity;
			_rank = td.Rank[_currentFilter];
			_previousRank = td.Rank[_currentFilter];

			cbSev.SelectedIndex = _currentSeverity;
			tbRank.Text = _rank.ToString();
			// btnComplete.Content = td.IsComplete ? "Reactivate" : "Complete";

			_tags = new ObservableCollection<TagHolder>();
			foreach (string tag in tags)
				_tags.Add(new TagHolder(tag));
			lbTags.ItemsSource = _tags;
			lbTags.Items.Refresh();

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
			string tempTodo = MainWindow.ExpandHashTagsInString(tbTodo.Text);

			string tempTags = "";
			ResultTags = new List<string>();
			foreach (TagHolder th in _tags)
				if (!ResultTags.Contains(th.Text))
					ResultTags.Add(th.Text);
			foreach (string tag in ResultTags)
				tempTags += tag + " ";
			tempTags = MainWindow.ExpandHashTagsInString(tempTags);

			_td.Tags = new ObservableCollection<string>();
			_td.Todo = tempTags.Trim() + " " + tempTodo.Trim();
			_td.Severity = _currentSeverity;
			if (_previousRank > _td.Rank[_currentFilter])
				_td.Rank[_currentFilter]--;
		}
		private void DeleteTag_OnClick(object sender, EventArgs e) {
			if (!(sender is Button b))
				return;
			TagHolder th = b.DataContext as TagHolder;
			_tags.Remove(th);
			lbTags.Items.Refresh();
		}
		private void AddTag_OnClick(object sender, EventArgs e) {
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
		private void Rank_OnClick(object sender, EventArgs e) {
			Button b = sender as Button;

			if (b == null)
				return;
			string compare = (string)b.CommandParameter;
			if (compare == "up")
				_rank--;
			else if (compare == "down")
				_rank++;
			else if (compare == "top")
				_rank = 0;
			else if (compare == "bottom")
				_rank = int.MaxValue;

			_rank = _rank > 0 ? _rank : 0;
			tbRank.Text = _rank.ToString();
			_td.Rank[_currentFilter] = _rank;
		}
		private void RankChanged() {
			if (tbRank.Text == "")
				tbRank.Text = "0";
			_td.Rank[_currentFilter] = Convert.ToInt32(tbRank.Text);
		}
		private void Rank_OnPreviewTextInput(object sender, TextCompositionEventArgs e) {
			if (!(sender is TextBox tb))
				return;
			var fullText = tb.Text.Insert(tb.SelectionStart, e.Text);

			e.Handled = !double.TryParse(fullText, out _);
		}
		private void Ok_OnClick(object sender, EventArgs e) {
			Result = true;
			SetTodo();

			Close();
		}
		private void Complete_OnClick(object sender, EventArgs e) {
			Result = true;
			ResultIsComplete = true;
			_td.IsComplete = !_td.IsComplete;
			SetTodo();

			Close();
		}
		private void Cancel_OnClick() {
			Log.Debug($"{IsSeverityEnabled},{IsCompleteEnabled},{IsTagEnabled},{IsRankEnabled},{IsTodoEnabled}");
			string test = "";
			if (_tags.Count > 0) test = _tags[0].Text;
			Log.Debug($"{cbSev.SelectionBoxItem},{tbRank.Text}, {tbTodo.Text},{test}");
			// Close();
		}
		public ICommand CancelCommand => new RelayCommand(Cancel_OnClick);
		public ICommand RankChangedCommand => new RelayCommand(RankChanged);
	}
}