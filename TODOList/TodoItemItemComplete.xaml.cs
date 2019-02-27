using System;
using System.Windows;

namespace TODOList
{
	public partial class TodoItemComplete
	{
		private readonly TodoItem td;
		public TodoItem Result => td;
		public bool isOk;

		public TodoItemComplete(TodoItem td)
		{
			InitializeComponent();
			this.td = new TodoItem(td.ToString());
			tbTodo.Text = td.Todo;
			tbNotes.Text = td.Notes;
			lblTime.Content = string.Format("{0}:{1}",td.TimeTakenInMinutes, td.TimeTaken.Second);
			
			CenterWindowOnMouse();
		}
		private void CenterWindowOnMouse()
		{
			Window win = Application.Current.MainWindow;

			double centerX = win.Width / 2 + win.Left;
			double centerY = win.Height / 2 + win.Top;
			Left = centerX - Width / 2;
			Top = centerY - Height / 2;
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
