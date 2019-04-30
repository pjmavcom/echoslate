using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TODOList
{
	public partial class DlgTodoItemEditor
	{
		private readonly TodoItem _td;
		public TodoItem ResultTD => _td;
		public List<string> ResultTags;
		public bool Result;
		
		private int _currentSeverity;
		private readonly int _previousRank;
		private List<TagHolder> _tags { get; set; }
		private string _currentListHash;
		
		public DlgTodoItemEditor(TodoItem td, string currentListHash)
		{
			InitializeComponent();
			
			_td = new TodoItem(td.ToString())
			{
				IsTimerOn = td.IsTimerOn
			};
			_currentSeverity = _td.Severity;
			_currentListHash = currentListHash;
			_previousRank = td.Rank[_currentListHash];

			cbSev.SelectedIndex = _currentSeverity;
			tbTodo.Text = td.Todo;
			tbNotes.Text = td.Notes;
			tbRank.Text = td.Rank[_currentListHash].ToString();
			lblTime.Content = $"{td.TimeTakenInMinutes:00}:{td.TimeTaken.Second:00}";
			btnComplete.Content = td.IsComplete ? "Reactivate" : "Complete";

			_tags = new List<TagHolder>();
			foreach (string tag in td.Tags)
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
			
			_td.Tags = new List<string>();
			_td.Todo = tempTags.Trim() + " " + tempTodo.Trim();
			_td.Notes = tbNotes.Text;
			_td.Severity = _currentSeverity;
			if (_previousRank > _td.Rank[_currentListHash])
				_td.Rank[_currentListHash]--;
		}
		private void Severity_OnSelectionChange(object sender, EventArgs e)
		{
			if (sender is ComboBox rb) _currentSeverity = rb.SelectedIndex;
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
			
			TagHolder th = new TagHolder(name.ToUpper() + tagNumber);
			_tags.Add(th);
			lbTags.Items.Refresh();
		}
		private void DeleteTag_OnClick(object sender, EventArgs e)
		{
			if (!(sender is Button b))
				return;
			
			TagHolder th = b.DataContext as TagHolder;
			_tags.Remove(th);
			lbTags.Items.Refresh();
		}
		private void Rank_OnClick(object sender, EventArgs e)
		{
			if (!(sender is Button b))
				return;
			
			string compare = (string) b.CommandParameter;
			
			if (compare == "up")
				_td.Rank[_currentListHash]--;
			else if (compare == "down")
				_td.Rank[_currentListHash]++;
			else if (compare == "top")
				_td.Rank[_currentListHash] = 0;
			else if (compare == "bottom")
				_td.Rank[_currentListHash] = int.MaxValue;

			_td.Rank[_currentListHash] = _td.Rank[_currentListHash] > 0 ? _td.Rank[_currentListHash] : 0;
			tbRank.Text = _td.Rank[_currentListHash].ToString();
		}
		private void Rank_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!(sender is TextBox tb))
				return;
			var fullText = tb.Text.Insert(tb.SelectionStart, e.Text);
			e.Handled = !double.TryParse(fullText, out _);
		}
		private void Rank_OnTextChange(object sender, EventArgs e)
		{
			if (tbRank.Text == "")
				tbRank.Text = "0";
			_td.Rank[_currentListHash] = Convert.ToInt32(tbRank.Text);
		}
		private void Time_OnClick(object sender, EventArgs e)
		{
			if (!(sender is Button b))
				return;
			
			int inc = 0;
			switch ((string) b.CommandParameter)
			{
				case "down10":
					inc = -10;
					break;
				case "down5":
					inc = -5;
					break;
				case "up5":
					inc = 5;
					break;
				case "up10":
					inc = 10;
					break;
			}
			_td.TimeTaken = _td.TimeTaken.Ticks >= ((-inc) * TimeSpan.TicksPerMinute)
				? _td.TimeTaken.AddMinutes(inc)
				: _td.TimeTaken.AddTicks(-_td.TimeTaken.Ticks);
			lblTime.Content = $"{_td.TimeTakenInMinutes:00}:{_td.TimeTaken.Second:00}";
		}
		private void OK_OnClick(object sender, EventArgs e)
		{
			Result = true;
			SetTodo();
			
			Close();
		}
		private void Complete_OnClick(object sender, EventArgs e)
		{
			Result = true;
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
