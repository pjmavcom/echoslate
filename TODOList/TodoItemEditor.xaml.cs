using System;
using System.Windows;
using System.Windows.Controls;

namespace TODOList
{
	public partial class TodoItemEditor : Window
	{
		private int currentSeverity = 0;
		private TodoItem td;
		public TodoItem Result => td;
//		public string Result = "";
		
		public TodoItemEditor(TodoItem td)
		{
			InitializeComponent();
			this.td = new TodoItem(td.ToString());
			currentSeverity = this.td.Severity;

			cbSev.SelectedIndex = currentSeverity - 1;
			tbTodo.Text = td.Todo;
		}
		
		// METHOD  ///////////////////////////////////// Severity() //
		private void cbTSeverity_SelectionChanged(object sender, EventArgs e)
		{
			ComboBox rb = sender as ComboBox;

			currentSeverity = rb.SelectedIndex + 1;
		}

		// METHOD  ///////////////////////////////////// btnOK() //
		private void btnOK_Click(object sender, EventArgs e)
		{
			td.Todo = tbTodo.Text;
			td.Severity = currentSeverity;
			Close();
		}

		// METHOD  ///////////////////////////////////// btnCancel() //
		private void btnCancel_Click(object sender, EventArgs e)
		{
			td.Severity = 0;
			Close();
		}
	}
}
