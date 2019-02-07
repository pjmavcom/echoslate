using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using ProgressBar = System.Windows.Controls.ProgressBar;
using RadioButton = System.Windows.Controls.RadioButton;

namespace TODOList
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>

	
	public partial class MainWindow
	{
		private List<TodoItem> todoList;
		private int currentSeverity;
		private int lastIndex;

		private bool reverseSort = false;
		private string currentSort = "severity";

		private string basePath = "";
			
		public MainWindow()
		{
			InitializeComponent();
			todoList = new List<TodoItem>();

			btnT1Add.Click += btnT1Add_Click;
			mnuSave.Click += mnuSave_Click;
			mnuLoad.Click += mnuLoad_Click;

//			todoList.Add(new TodoItem() {Time = DateTime.Now.ToString("yyyy-MM-dd"), Todo = "Test1", Severity = 1});
//			todoList.Add(new TodoItem() {Time = DateTime.Now.ToString("yyyy-MM-dd"), Todo = "Test2", Severity = 1});
//			todoList.Add(new TodoItem() {Time = DateTime.Now.ToString("yyyy-MM-dd"), Todo = "Test3", Severity = 3});

//			TodoItem test = new TodoItem(todoList[2].ToString());

			lbT1Todos.ItemsSource = todoList;
			ResortTodoList();
		}

		// METHOD  ///////////////////////////////////// btnT1Add() //
		private void btnT1Add_Click(object sender, EventArgs e)
		{
			todoList.Add(new TodoItem() {Time = DateTime.Now.ToString("yyyy-MM-dd"), Todo = txtT1NewTodo.Text, Severity = currentSeverity});
			ResortTodoList();
			txtT1NewTodo.Clear();
		}
		// METHOD  ///////////////////////////////////// Severity() //
		private void RdoSeverity(object sender, EventArgs e)
		{
			RadioButton rb = sender as RadioButton;
			currentSeverity = Convert.ToInt16(rb.CommandParameter.ToString());
		}
		private void pbSeverity_OnMouseUp(object sender, MouseEventArgs e)
		{
			ProgressBar pb = sender as ProgressBar;
			object item = pb.DataContext;
			int index = lbT1Todos.Items.IndexOf(item);

			lastIndex = index;
			todoList[index].Severity++;
			if (todoList[index].Severity > 3)
				todoList[index].Severity = 0;
			lbT1Todos.Items.Refresh();
		}
		private void pbSeverity_OnMouseEnter(object sender, MouseEventArgs e)
		{
			ProgressBar pb = sender as ProgressBar;
			object item = pb.DataContext;
			int index = lbT1Todos.Items.IndexOf(item);
			if (lastIndex != index)
			{
				lastIndex = index;
				ResortTodoList();
			}
		}
		private void pbSeverity_OnMouseLeave(object sender, MouseEventArgs e)
		{
//			ProgressBar pb = sender as ProgressBar;
//			object item = pb.DataContext;
//			int index = lbT1Todos.Items.IndexOf(item);
//
		}
		// METHOD  ///////////////////////////////////// btnT1Move() //
		private void btnT1MoveUp_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			object item = b.DataContext;
			int index = lbT1Todos.Items.IndexOf(item);
			if (index == 0)
				return;
			
			todoList.Remove((TodoItem) item);
			todoList.Insert(index - 1, (TodoItem) item);
			ResortTodoList();
		}
		private void btnT1MoveDown_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			object item = b.DataContext;
			int index = lbT1Todos.Items.IndexOf(item);
			if (index == todoList.Count - 1)
				return;
			
			todoList.Remove((TodoItem) item);
			todoList.Insert(index + 1, (TodoItem) item);
			ResortTodoList();
		}
		// METHOD  ///////////////////////////////////// btnT1Complete() //
		private void btnT1Complete_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			object item = b.DataContext;
			int index = lbT1Todos.Items.IndexOf(item);

			todoList[index].Complete = !todoList[index].Complete;
			todoList[index].CompletedTime = todoList[index].Complete ? DateTime.Now.ToString() : " ----- ----- ";

			ResortTodoList();
		}
		// METHOD  ///////////////////////////////////// Sort() //
		private void SortBySeverity_Click(object sender, EventArgs e)
		{
			if (currentSort != "severity")
			{
				reverseSort = false;
				currentSort = "severity";
			}
			reverseSort = !reverseSort;
			
			ResortTodoList();
		}
		private void SortByDate_Click(object sender, EventArgs e)
		{
			if (currentSort != "date")
			{
				reverseSort = false;
				currentSort = "date";
			}
			reverseSort = !reverseSort;
			
			ResortTodoList();
		}
		private void ResortTodoList()
		{
			List<TodoItem> completeItems = new List<TodoItem>();
			List<TodoItem> incompleteItems = new List<TodoItem>();
			foreach (TodoItem td in todoList)
			{
				if (td.Complete)
					completeItems.Add(td);
				else
					incompleteItems.Add(td);
			}
			
			List<TodoItem> sortedIncomplete;
			switch (currentSort)
			{
				case "severity":
					sortedIncomplete = reverseSort ? incompleteItems.OrderByDescending(o => o.Severity).ToList() : incompleteItems.OrderBy(o => o.Severity).ToList();
					break;
				case "date":
					sortedIncomplete = reverseSort ? incompleteItems.OrderByDescending(o => o.Time).ToList() : incompleteItems.OrderBy(o => o.Time).ToList();
					break;
				default:
					sortedIncomplete = incompleteItems;
					break;
			}
			
			List<TodoItem> sortedComplete = completeItems.OrderBy(o => o.CompletedTime).ToList();
			todoList.Clear();
			foreach (TodoItem td in sortedIncomplete)
				todoList.Add(td);
			foreach (TodoItem td in sortedComplete)
				todoList.Add(td);
			
			lbT1Todos.Items.Refresh();
		}
		// METHOD  ///////////////////////////////////// Save() //
		private void mnuSave_Click(object sender, EventArgs e)
		{
			SaveFileDialog fileDialog = new SaveFileDialog
			{
				Title  = "Select folder to save game in.",
				FileName = basePath == "" ? AppDomain.CurrentDomain.BaseDirectory : basePath
			};

			DialogResult folderResult = fileDialog.ShowDialog();

			if (folderResult != System.Windows.Forms.DialogResult.OK)
				return;
			
			StreamWriter stream   = new StreamWriter(File.Open(fileDialog.FileName, FileMode.Create));

			foreach (TodoItem td in todoList)
			{
				stream.WriteLine(td.ToString());
			}
			stream.Close();
		}

		// METHOD  ///////////////////////////////////// Load() //
		private void mnuLoad_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog
			{
				Title = "Open file: ",
				FileName = basePath == "" ? AppDomain.CurrentDomain.BaseDirectory : basePath
			};

			DialogResult ofdResult = ofd.ShowDialog();
			
			if (ofdResult != System.Windows.Forms.DialogResult.OK)
				return;

			StreamReader stream = new StreamReader(File.Open(ofd.FileName, FileMode.Open));

			todoList.Clear();
			
			string line = stream.ReadLine();

			while (line != null)
			{
				TodoItem td = new TodoItem(line);
				todoList.Add(td);
				line = stream.ReadLine();
			}

			stream.Close();
			
			ResortTodoList();
		}
	}
}