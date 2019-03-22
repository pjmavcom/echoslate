using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TODOList
{
	public partial class DlgTodoItemComplete
	{
		private readonly TodoItem td;
		public TodoItem Result => td;
		public bool isOk;

		public DlgTodoItemComplete(TodoItem td)
		{
			InitializeComponent();
			this.td = new TodoItem(td.ToString());
			tbTodo.Text = td.Todo;
			tbNotes.Text = td.Notes;
			tbTags.Text = td.TagsList;
			lblTime.Content = $"{td.TimeTakenInMinutes}:{td.TimeTaken.Second}";
			
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

		// METHOD  ///////////////////////////////////// btnComplete_Click() //
		private void btnComplete_Click(object sender, EventArgs e)
		{
			isOk = true;
			td.IsComplete = true;
			string tempTodo = MainWindow.ExpandHashTagsInString(tbTodo.Text);
			string tempTags = MainWindow.ExpandHashTagsInString(tbTags.Text);
			td.Todo = tempTags.Trim() + " " + tempTodo.Trim();
//			td.Todo = tbTodo.Text;
//			td.Tags = ParseTags(tbTags.Text);
			td.Notes = tbNotes.Text;
//			td.ParseTags();
			Close();
		}

		private List<string> ParseTags(string tags)
		{
			List<string> result = td.Tags.ToList();
			string[] lines = tags.Split('\r');
			
			foreach (string s in lines)
			{
				
				string trimmed = s.Trim();
				if (trimmed == "")
					continue;
				if (trimmed.Contains("\n"))
				{
					int index = trimmed.IndexOf("\n");
					trimmed = trimmed.Remove(index, 1);
				}
				string newTag = "";
				if (trimmed.Contains("#"))
					newTag = trimmed.ToUpper();
				else
					newTag = "#" + trimmed.ToUpper();
				result.Add(newTag);
			}
			return result;
		}
		// METHOD  ///////////////////////////////////// btnCancel() //
		private void btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}
		
		private void btnTime_Click(object sender, EventArgs e)
		{
			if (sender is Button b)
			{
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
					if (td.TimeTaken.Ticks >= ((-inc) * TimeSpan.TicksPerMinute))
						td.TimeTaken = td.TimeTaken.AddMinutes(inc);
					else
						td.TimeTaken = td.TimeTaken.AddTicks(-td.TimeTaken.Ticks);
					lblTime.Content = $"{td.TimeTakenInMinutes}:{td.TimeTaken.Second}";
			}
		}
	}
}
