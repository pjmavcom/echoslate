using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TODOList
{
	public partial class DlgTodoItemEditor
	{
		private readonly TodoItem _todoItem;
		public TodoItem ResultTodoItem => _todoItem;
		public List<string> ResultTags;
		public bool Result;
		
		private int _currentSeverity;
		private readonly int _previousRank;
		private List<TagHolder> Tags { get; set; }
		private string _currentListHash;
		
		public DlgTodoItemEditor(TodoItem td, string currentListHash)
		{
			InitializeComponent();
			
			_todoItem = new TodoItem(td.ToString())
			{
				IsTimerOn = td.IsTimerOn
			};
			_currentSeverity = _todoItem.Severity;
			_currentListHash = currentListHash;
			_previousRank = td.Rank[_currentListHash];

			cbSev.SelectedIndex = _currentSeverity;
			tbTodo.Text = td.Todo;

			
			string tempNote = string.Empty;
			tempNote = td.Notes;
			if (tempNote.Contains("/n"))
			{
				tempNote = tempNote.Replace("/n", Environment.NewLine);
			}
			tbNotes.Text = tempNote;
			 
			iudRank.Value = td.Rank[_currentListHash];
			iudTime.Value = _todoItem.TimeTakenInMinutes;
			btnComplete.Content = td.IsComplete ? "Reactivate" : "Complete";

			Tags = new List<TagHolder>();
			foreach (string tag in td.Tags)
				Tags.Add(new TagHolder(tag));
			lbTags.ItemsSource = Tags;
			lbTags.Items.Refresh();

			tbKanban.Text = _todoItem.Kanban.ToString();
			
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
			foreach(TagHolder th in Tags)
				if (!ResultTags.Contains(th.Text))
					ResultTags.Add(th.Text);
			foreach (string tag in ResultTags)
				tempTags += tag + " ";
			tempTags = MainWindow.ExpandHashTagsInString(tempTags);
			
			_todoItem.Tags = new List<string>();
			_todoItem.Todo = tempTags.Trim() + " " + tempTodo.Trim();
			_todoItem.Notes = tbNotes.Text;
			_todoItem.Severity = _currentSeverity;
			if (_previousRank > _todoItem.Rank[_currentListHash])
				_todoItem.Rank[_currentListHash]--;
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
				foreach (TagHolder t in Tags)
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
			Tags.Add(th);
			lbTags.Items.Refresh();
		}
		private void DeleteTag_OnClick(object sender, EventArgs e)
		{
			if (!(sender is Button b))
				return;
			
			TagHolder th = b.DataContext as TagHolder;
			Tags.Remove(th);
			lbTags.Items.Refresh();
		}
		private void Rank_OnValueChanged(object sender, EventArgs e)
		{
			_todoItem.Rank[_currentListHash] = (int) iudRank.Value;
			_todoItem.Rank[_currentListHash] = _todoItem.Rank[_currentListHash] > 0 ? _todoItem.Rank[_currentListHash] : 0;
		}
		private void Time_OnValueChanged(object sender, EventArgs e)
		{
			if (iudTime != null)
				_todoItem.TimeTaken = new DateTime((long)(iudTime.Value * TimeSpan.TicksPerMinute));
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
			_todoItem.IsComplete = !_todoItem.IsComplete;
			SetTodo();
			
			Close();
		}
		private void Cancel_OnClick(object sender, EventArgs e)
		{
			Close();
		}
	}
}
