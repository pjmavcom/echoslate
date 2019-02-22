using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TODOList
{
	public partial class TodoItemComplete : Window
	{
		private TodoItem td;
		public TodoItem Result => td;
		public bool isOk = false;
		private int previousRank = 0;
		
		public TodoItemComplete(TodoItem td)
		{
			InitializeComponent();
			this.td = new TodoItem(td.ToString());
			tbTodo.Text = td.Todo;
			tbNotes.Text = td.Notes;
			lblTime.Content = td.TimeTakenInMinutes + td.TimeTaken.Second;
			
			CenterWindowOnMouse();
		}
		private void CenterWindowOnMouse()
		{
			Point mousePositionInApp = Mouse.GetPosition(Application.Current.MainWindow);
			Point mousePositionInScreenCoordinates = Application.Current.MainWindow.PointToScreen(mousePositionInApp);

			Top = mousePositionInScreenCoordinates.Y;
			Left = mousePositionInScreenCoordinates.X;
		}

		// METHOD  ///////////////////////////////////// btnComplete_Click() //
		private void btnComplete_Click(object sender, EventArgs e)
		{
			isOk = true;
			td.IsComplete = !td.IsComplete;
			td.Todo = tbTodo.Text;
			td.Notes = tbNotes.Text;
			td.ParseTags();
			Close();
		}

		// METHOD  ///////////////////////////////////// btnCancel() //
		private void btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
