using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
			iudRank.Value = td.Rank[_currentListHash];
			iudTime.Value = _td.TimeTakenInMinutes;
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
		private void Rank_OnValueChanged(object sender, EventArgs e)
		{
			_td.Rank[_currentListHash] = (int) iudRank.Value;
			_td.Rank[_currentListHash] = _td.Rank[_currentListHash] > 0 ? _td.Rank[_currentListHash] : 0;
		}
		private void Time_OnValueChanged(object sender, EventArgs e)
		{
			_td.TimeTaken = new DateTime((long) (iudTime.Value * TimeSpan.TicksPerMinute));
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
