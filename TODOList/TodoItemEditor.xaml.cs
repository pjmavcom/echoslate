using System;
using System.Windows;
using System.Windows.Controls;

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

			cbSev.SelectedIndex = currentSeverity - 1;
			tbTodo.Text = td.Todo;
			tbNotes.Text = td.Notes;
			lblRank.Content = td.Rank;
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
			if (sender is ComboBox rb) currentSeverity = rb.SelectedIndex + 1;
		}

		// METHOD  ///////////////////////////////////// Rank() //
		private void btnRank_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;

			if (b != null && (string) b.CommandParameter == "up")
			{
				td.Rank--;
			}
			else if (b != null && (string) b.CommandParameter == "down")
			{
				td.Rank++;
			}

			td.Rank = td.Rank > 0 ? td.Rank : 0;
			lblRank.Content = td.Rank;
		}

		// METHOD  ///////////////////////////////////// btnOK() //
		private void btnOK_Click(object sender, EventArgs e)
		{
			td.Todo = tbTodo.Text;
			td.Notes = tbNotes.Text;
			td.ParseTags();
			td.Severity = currentSeverity;
			isOk = true;
			
			if (previousRank > td.Rank)
				td.Rank--;
			
			Close();
		}

		// METHOD  ///////////////////////////////////// btnComplete_Click() //
		private void btnComplete_Click(object sender, EventArgs e)
		{
			isOk = true;
			td.IsComplete = !td.IsComplete;
			td.Todo = tbTodo.Text;
			td.Notes = tbNotes.Text;
			td.ParseTags();
			td.Severity = currentSeverity;
			Close();
		}

		// METHOD  ///////////////////////////////////// btnCancel() //
		private void btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
