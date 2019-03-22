using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TODOList
{
	public partial class DlgTodoItemEditor
	{
		private int currentSeverity;
		private readonly TodoItem td;
		public TodoItem Result => td;
		public List<string> ResultTags;
		public bool isOk;
		private readonly int previousRank;
		private List<TagHolder> Tags { get; set; }
		
		public DlgTodoItemEditor(TodoItem td)
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
			tbRank.Text = td.Rank.ToString();
			lblTime.Content = $"{td.TimeTakenInMinutes}:{td.TimeTaken.Second}";
			previousRank = td.Rank;

			Tags = new List<TagHolder>();
			foreach (string tag in td.Tags)
				Tags.Add(new TagHolder(tag));
			lbTags.ItemsSource = Tags;
			lbTags.Items.Refresh();
			
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
		private void cbTSeverity_SelectionChanged(object sender, EventArgs e)
		{
			if (sender is ComboBox rb) currentSeverity = rb.SelectedIndex;
		}
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
		private void AddTag_OnClick(object sender, EventArgs e)
		{
			string name = "#NewTag";
			int tagNumber = 0;
			bool nameExists = false;
			do
			{
				foreach (TagHolder t in Tags)
				{
					if (t.Text == name + tagNumber.ToString())
					{
						tagNumber++;
						nameExists = true;
						break;
					}
					else 
						nameExists = false;
				}
			} while (nameExists);
			TagHolder th = new TagHolder(name + tagNumber);
			Tags.Add(th);
			lbTags.Items.Refresh();
		}
		private void DeleteTag_OnClick(object sender, EventArgs e)
		{
			if (sender is Button b)
			{
				TagHolder th = b.DataContext as TagHolder;
				Tags.Remove(th);
				lbTags.Items.Refresh();
				
			}
		}
		private void btnOK_Click(object sender, EventArgs e)
		{
			
//			MainWindow.ExpandHashTags(td);
			string tempTodo = MainWindow.ExpandHashTagsInString(tbTodo.Text);
			string tempTags = "";
			ResultTags = new List<string>();
			foreach(TagHolder th in Tags)
				if (!ResultTags.Contains(th.Text))
					ResultTags.Add(th.Text);
			foreach (string tag in ResultTags)
				tempTags += tag + " ";
			tempTags = MainWindow.ExpandHashTagsInString(tempTags);
			td.Todo = tempTags.Trim() + " " + tempTodo.Trim();
			td.Notes = tbNotes.Text;

//			td.Tags = ParseTags(tbTags.Text);
			
//			td.ParseTags();
			td.Severity = currentSeverity;
			isOk = true;
			
			if (previousRank > td.Rank)
				td.Rank--;
			
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
			string tempTodo = MainWindow.ExpandHashTagsInString(tbTodo.Text);
			string tempTags = "";
			ResultTags = new List<string>();
			foreach(TagHolder th in Tags)
				if (!ResultTags.Contains(th.Text))
					ResultTags.Add(th.Text);
			foreach (string tag in ResultTags)
				tempTags += tag + " ";
			tempTags = MainWindow.ExpandHashTagsInString(tempTags);
			td.Todo = tempTags.Trim() + " " + tempTodo.Trim();
//			td.Todo = tbTodo.Text;
			td.Notes = tbNotes.Text;
//			td.Tags = ParseTags(tbTags.Text);
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
