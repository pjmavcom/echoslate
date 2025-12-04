using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TODOList
{
	public partial class DlgTodoMultiItemEditor
	{
		public TodoItem ResultTD => _td;
		public List<string> ResultTags;
		public bool Result;
		public bool ResultIsComplete;
		
		public bool ChangeSev { get; private set; }
		public bool ChangeRank { get; private set; }
		public bool ChangeComplete { get; private set; }
		public bool ChangeTodo { get; private set; }
		public bool ChangeTag { get; private set; }
		
		private List<TagHolder> _tags { get; }
		private readonly TodoItem _td;
		private readonly int _previousRank;
		private int _currentSeverity;
		private int _rank;
		private readonly string _currentListHash;
		
		public DlgTodoMultiItemEditor(TodoItem td, string currentListHash, List<string> tags)
		{
			InitializeComponent();
			
			_td = new TodoItem(td.ToString())
			{
				IsTimerOn = td.IsTimerOn
			};
			_currentListHash = currentListHash;
			_currentSeverity = _td.Severity;
			_rank = td.Rank[_currentListHash];
			_previousRank = td.Rank[_currentListHash];
			
			cbSev.SelectedIndex = _currentSeverity;
			tbRank.Text = _rank.ToString();
			btnComplete.Content = td.IsComplete ? "Reactivate" : "Complete";
			
			_tags = new List<TagHolder>();
			foreach (string tag in tags)
				_tags.Add(new TagHolder(tag));
			lbTags.ItemsSource = _tags;
			lbTags.Items.Refresh();
			
			CenterWindowOnMouse();
		}
		private void CenterWindowOnMouse()
		{
			Window win = Application.Current.MainWindow;

			if (win == null)
				return;
			double centerX = win.Width / 2 + win.Left;
			double centerY = win.Height / 2 + win.Top;
			Left = centerX - Width / 2;
			Top = centerY - Height / 2;
		}
		private void SetTodo()
		{
			string tempTodo = MainWindow.ExpandHashTagsInString(tbTodo.Text);
			
			string tempTags = "";
			ResultTags = new List<string>();
			foreach(TagHolder th in _tags)
				if (!ResultTags.Contains(th.Text))
					ResultTags.Add(th.Text);
			foreach (string tag in ResultTags)
				tempTags += tag + " ";
			tempTags = MainWindow.ExpandHashTagsInString(tempTags);
			
			_td.Tags = new ObservableCollection<string>();
			_td.Todo = tempTags.Trim() + " " + tempTodo.Trim();
			_td.Severity = _currentSeverity;
			if (_previousRank > _td.Rank[_currentListHash])
				_td.Rank[_currentListHash]--;
		}
		private void Severity_OnSelectionChange(object sender, EventArgs e)
		{
			if (sender is ComboBox rb) _currentSeverity = rb.SelectedIndex;
		}
		private void DeleteTag_OnClick(object sender, EventArgs e)
		{
			if (!(sender is Button b))
				return;
			TagHolder th = b.DataContext as TagHolder;
			_tags.Remove(th);
			lbTags.Items.Refresh();
		}
		private void AddTag_OnClick(object sender, EventArgs e)
		{
			string name = "#NEWTAG";
			int tagNumber = 0;
			bool nameExists = false;
			do
			{
				foreach (TagHolder t in _tags)
				{
					if (t.Text == name.ToUpper() + tagNumber ||
					    t.Text == "#" + name.ToUpper() + tagNumber)
					{
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
		private void TagsCheckBox_OnClick(object sender, EventArgs e)
		{
			if (ckTags.IsChecked != null)
				ChangeTag = (bool) ckTags.IsChecked;
		}
		private void RankCheckBox_OnClick(object sender, EventArgs e)
		{
			if (ckRank.IsChecked != null)
				ChangeRank = (bool) ckRank.IsChecked;
		}
		private void CompleteCheckBox_OnClick(object sender, EventArgs e)
		{
			if (ckComplete.IsChecked != null)
				ChangeComplete = (bool) ckComplete.IsChecked;
		}
		private void SevCheckBox_OnClick(object sender, EventArgs e)
		{
			if (ckSev.IsChecked != null)
				ChangeSev = (bool) ckSev.IsChecked;
		}
		private void TodoCheckBox_OnClick(object sender, EventArgs e)
		{
			if (ckTodo.IsChecked != null)
				ChangeTodo = (bool) ckTodo.IsChecked;
		}
		private void Rank_OnClick(object sender, EventArgs e)
		{
			Button b = sender as Button;

			if (b == null)
				return;
			string compare = (string) b.CommandParameter;
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
			_td.Rank[_currentListHash] = _rank;
		}
		private void Rank_OnTextChanged(object sender, EventArgs e)
		{
			if (tbRank.Text == "")
				tbRank.Text = "0";
			_td.Rank[_currentListHash] = Convert.ToInt32(tbRank.Text);
		}
		private void Rank_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!(sender is TextBox tb))
				return;
			var fullText = tb.Text.Insert(tb.SelectionStart, e.Text);

			e.Handled = !double.TryParse(fullText, out _);
		}
		private void Ok_OnClick(object sender, EventArgs e)
		{
			Result = true;
			SetTodo();
			
			Close();
		}
		private void Complete_OnClick(object sender, EventArgs e)
		{
			Result = true;
			ResultIsComplete = true;
			_td.IsComplete = !_td.IsComplete;
			SetTodo();
			
			Close();
		}
		private void Cancel_OnClick(object sender, EventArgs e)
		{
			Close();
		}
	}
}
