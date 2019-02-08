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

			switch (td.Severity)
			{
				case 1:
					rdoSev1.IsChecked = true;
					currentSeverity = 1;
					break;
				case 2:
					rdoSev2.IsChecked = true;
					currentSeverity = 2;
					break;
				case 3:
					rdoSev3.IsChecked = true;
					currentSeverity = 3;
					break;
			}
			tbTodo.Text = td.Todo;
		}
		
		// METHOD  ///////////////////////////////////// Severity() //
		private void RdoSeverity_Checked(object sender, EventArgs e)
		{
			RadioButton rb = sender as RadioButton;
			currentSeverity = Convert.ToInt16(rb.CommandParameter.ToString());
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
