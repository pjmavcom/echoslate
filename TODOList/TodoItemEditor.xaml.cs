using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TODOList
{
	public partial class TodoItemEditor
	{
		private int currentSeverity;
		private readonly TodoItem td;
		public TodoItem Result => td;
		public bool isOk;
		private readonly int previousRank;
		
		public TodoItemEditor(TodoItem td)
		{
			InitializeComponent();
			this.td = new TodoItem(td.ToString())
			{
				IsTimerOn = td.IsTimerOn
			};
			currentSeverity = this.td.Severity;

			cbSev.SelectedIndex = currentSeverity;
			tbTodo.Text = td.Todo;
			tbNotes.Text = td.Notes;
			tbTags.Text = td.TagsList;
			tbRank.Text = td.Rank.ToString();
			lblTime.Content = $"{td.TimeTakenInMinutes}:{td.TimeTaken.Second}";
			previousRank = td.Rank;
			
			CenterWindowOnMouse();
			btnComplete.Content = td.IsComplete ? "Reactivate" : "Complete";
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

		// METHOD  ///////////////////////////////////// Severity() //
		private void cbTSeverity_SelectionChanged(object sender, EventArgs e)
		{
			if (sender is ComboBox rb) currentSeverity = rb.SelectedIndex;
		}

		// METHOD  ///////////////////////////////////// Rank() //
		private void btnRank_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			if (b == null)
				return;
			string compar = (string) b.CommandParameter;

			
			if (compar == "up")
			{
				td.Rank--;
			}
			else if (compar == "down")
			{
				td.Rank++;
			}
			else if (compar == "top")
			{
				td.Rank = 0;
			}
			else if (compar == "bottom")
			{
				td.Rank = int.MaxValue;
			}

			td.Rank = td.Rank > 0 ? td.Rank : 0;
			tbRank.Text = td.Rank.ToString();
		}

		// METHOD  ///////////////////////////////////// btnOK() //
		private void btnOK_Click(object sender, EventArgs e)
		{
			td.Todo = tbTodo.Text;
			td.Notes = tbNotes.Text;

			td.Tags = ParseTags(tbTags.Text);
			
//			td.ParseTags();
			td.Severity = currentSeverity;
			isOk = true;
			
			if (previousRank > td.Rank)
				td.Rank--;
			
			Close();
		}

		private List<string> ParseTags(string tags)
		{
			List<string> result = new List<string>(); // TODO: //td.Tags.ToList();
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
				
				if(!result.Contains(newTag))
					result.Add(newTag);
			}
			return result;
		}
		// METHOD  ///////////////////////////////////// btnComplete_Click() //
		private void btnComplete_Click(object sender, EventArgs e)
		{
			isOk = true;
			td.IsComplete = !td.IsComplete;
			td.Todo = tbTodo.Text;
			td.Notes = tbNotes.Text;
			td.Tags = ParseTags(tbTags.Text);
//			td.ParseTags();
			td.Severity = currentSeverity;
			Close();
		}

		// METHOD  ///////////////////////////////////// btnCancel() //
		private void btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}
		
		private void tbRank_Changed(object sender, EventArgs e)
		{
			if (tbRank.Text == "")
				tbRank.Text = "0";
			td.Rank = Convert.ToInt32(tbRank.Text);
		}
		
		private void tbRank_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			var textBox = sender as TextBox;
			// Use SelectionStart property to find the caret position.
			// Insert the previewed text into the existing text in the textbox.
			var fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
		
			double val;
			// If parsing is successful, set Handled to false
			e.Handled = !double.TryParse(fullText, out val);
		}

		private void btnTime_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			if ((string) b.CommandParameter == "up")
			{
				td.TimeTaken = td.TimeTaken.AddMinutes(5);
				lblTime.Content = $"{td.TimeTakenInMinutes}:{td.TimeTaken.Second}";
			}
			else if ((string) b.CommandParameter == "down")
			{
				if (td.TimeTaken.Ticks >= (5 * TimeSpan.TicksPerMinute))
					td.TimeTaken = td.TimeTaken.AddMinutes(-5);
				else
					td.TimeTaken = td.TimeTaken.AddTicks(-td.TimeTaken.Ticks);
				
				lblTime.Content = $"{td.TimeTakenInMinutes}:{td.TimeTaken.Second}";
			}
		}
	}
}
