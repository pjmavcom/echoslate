using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace TODOList
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>

	public class TodoItem
	{
		public string Todo { get; set; }
		public string Time { get; set; }
		public int Severity { get; set; }
	}
	
	public partial class MainWindow
	{
		private List<TodoItem> todoList;
		private int currentSeverity;
		
			
		public MainWindow()
		{
			InitializeComponent();
			todoList = new List<TodoItem>();

			btnT1Add.Click += btnT1Add_Click;
			
			todoList.Add(new TodoItem() { Time=DateTime.Now.ToString(), Todo="Test1", Severity=2 });
			todoList.Add(new TodoItem() { Time=DateTime.Now.ToString(), Todo="Test2", Severity=1 });
			todoList.Add(new TodoItem() { Time=DateTime.Now.ToString(), Todo="Test3", Severity=3 });


//			lbT1TodosRefresh();
			lbT1Todos.ItemsSource = todoList;
//			lbT1Todos.Items.Refresh();
		}

		public void lbT1TodosRefresh()
		{
			lbT1Todos.Items.Clear();
			
		}

		private void btnT1Add_Click(object sender, EventArgs e)
		{
			todoList.Add(new TodoItem() { Time = DateTime.Now.ToString(), Todo = txtT1NewTodo.Text, Severity = currentSeverity });
			lbT1Todos.Items.Refresh();
		}
		private void RdoSeverity(object sender, EventArgs e)
		{
			RadioButton rb = sender as RadioButton;
			currentSeverity = Convert.ToInt16(rb.CommandParameter.ToString());
		}
	}
}