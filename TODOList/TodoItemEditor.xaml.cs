using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TODOList
{
	public partial class TodoItemEditor : Window
	{
		private int currentSeverity = 0;
		private TodoItem td;
		public TodoItem Result => td;
		public bool isOk = false;
		private int previousRank = 0;
		
		public TodoItemEditor(TodoItem td)
		{
			InitializeComponent();
			this.td = new TodoItem(td.ToString());
			this.td.IsTimerOn = td.IsTimerOn;
			currentSeverity = this.td.Severity;

			cbSev.SelectedIndex = currentSeverity - 1;
			tbTodo.Text = td.Todo;
			tbNotes.Text = td.Notes;
			lblRank.Content = td.Rank;
			lblTime.Content = td.TimeTakenInMinutes + td.TimeTaken.Second;
			previousRank = td.Rank;
			
			CenterWindowOnMouse();
			btnComplete.Content = td.IsComplete ? "Reactivate" : "Complete";
		}
		private void CenterWindowOnMouse()
		{
			Point mousePositionInApp = Mouse.GetPosition(Application.Current.MainWindow);
			Point mousePositionInScreenCoordinates = Application.Current.MainWindow.PointToScreen(mousePositionInApp);

			Top = mousePositionInScreenCoordinates.Y;
			Left = mousePositionInScreenCoordinates.X;
		}

		// METHOD  ///////////////////////////////////// Severity() //
		private void cbTSeverity_SelectionChanged(object sender, EventArgs e)
		{
			ComboBox rb = sender as ComboBox;
			currentSeverity = rb.SelectedIndex + 1;
		}

		// METHOD  ///////////////////////////////////// Rank() //
		private void btnRank_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;

			if ((string) b.CommandParameter == "up")
			{
				td.Rank--;
			}
			else if ((string) b.CommandParameter == "down")
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
