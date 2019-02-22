using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TODOList
{
	public partial class TodoItemComplete : Window
	{
//		private int currentSeverity = 0;
		private TodoItem td;
		public TodoItem Result => td;
//		public bool isComplete = false;
		public bool isOk = false;
//		public bool doAdjustRank = false;
		private int previousRank = 0;
//		public string Result = "";
		
		public TodoItemComplete(TodoItem td)
		{
			InitializeComponent();
			this.td = new TodoItem(td.ToString());
//			currentSeverity = this.td.Severity;

//			cbSev.SelectedIndex = currentSeverity - 1;
			tbTodo.Text = td.Todo;
			tbNotes.Text = td.Notes;
			lblTime.Content = td.TimeTakenInMinutes + td.TimeTaken.Second;
//			lblRank.Content = td.Rank;
//			previousRank = td.Rank;
			
			CenterWindowOnMouse();
//			btnComplete.Content = td.IsComplete ? "Reactivate" : "Complete";
		}
		private void CenterWindowOnMouse()
		{
			Point mousePositionInApp = Mouse.GetPosition(Application.Current.MainWindow);
			Point mousePositionInScreenCoordinates = Application.Current.MainWindow.PointToScreen(mousePositionInApp);

			double halfWidth = Width / 2;
			double halfHeight = Height / 2;
			Top = mousePositionInScreenCoordinates.Y - halfHeight;
			Left = mousePositionInScreenCoordinates.X - halfWidth;
		}

		// METHOD  ///////////////////////////////////// btnComplete_Click() //
		private void btnComplete_Click(object sender, EventArgs e)
		{
			isOk = true;
//			isComplete = true;
			td.IsComplete = !td.IsComplete;
			td.Todo = tbTodo.Text;
			td.Notes = tbNotes.Text;
			td.ParseTags();
//			td.Severity = currentSeverity;
			Close();
		}

		// METHOD  ///////////////////////////////////// btnCancel() //
		private void btnCancel_Click(object sender, EventArgs e)
		{
//			td.Severity = 0;
			Close();
		}
	}
}
