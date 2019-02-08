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
using MenuItem = System.Windows.Controls.MenuItem;
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
		private const string DATE = "yyyMMdd";
		private const string TIME = "HHmmss";
		
		// TODO TAB ITEMS
		private List<TodoItem> _tIncompleteTodoList;
		private List<TodoItem> _tCompleteTodoList;
		private List<string> _tHashTags;
		
		private int _tCurrentSeverity;
		private int _tLastIndex;

		private bool _tReverseSort = false;
		private string _tCurrentSort = "sev";
		private int _tCurrentHashTagSortIndex = -1;
		private bool _tDidHashChange = false;
		
		
		// HISTORY TAB ITEMS
		private List<HistoryItem> _hHistoryList;
		private HistoryItem _hViewingHistoryItem;
		private HistoryItem _hCurrentHistoryItem;
		

		private string basePath = "";
			
		public MainWindow()
		{
			InitializeComponent();
			_tIncompleteTodoList = new List<TodoItem>();
			_tCompleteTodoList = new List<TodoItem>();
			_hHistoryList = new List<HistoryItem>();
			_tHashTags = new List<string>();

			_hCurrentHistoryItem = new HistoryItem("", "");
			_hViewingHistoryItem = _hCurrentHistoryItem;

//			btnT1Add.Click += btnT1Add_Click;
//			mnuSave.Click += mnuSave_Click;
//			mnuLoad.Click += mnuLoad_Click;

			_tIncompleteTodoList.Add(new TodoItem() {Todo = "Test1", Severity = 1});
			_tIncompleteTodoList.Add(new TodoItem() {Todo = "Test2", Severity = 2});
			_tIncompleteTodoList.Add(new TodoItem() {Todo = "Test3", Severity = 3});
			_tIncompleteTodoList.Add(new TodoItem(new DateTime(1999,91,92,15,55,44), "Test4", 1));
			_tIncompleteTodoList.Add(new TodoItem(new DateTime(1999,91,92,15,55,44), "Test5", 2));
			_tIncompleteTodoList.Add(new TodoItem(new DateTime(1999,91,92,15,55,44), "Test6", 3));

//			TodoItem test = new TodoItem(todoList[2].ToString());

			lbTIncompleteTodos.ItemsSource = _tIncompleteTodoList;
			lbTCompleteTodos.ItemsSource = _tCompleteTodoList;
			
			lbHHistory.ItemsSource = _hHistoryList;
			ResortTodoList();
		}
		
		
		// METHOD  ///////////////////////////////////// HISTORY TAB() //
		// METHOD  ///////////////////////////////////// AddTodoToHistory() //
		private void AddTodoToHistory(TodoItem td)
		{
			if (_hCurrentHistoryItem.DateAdded == "")
				AddNewHistoryItem();
			_hCurrentHistoryItem.AddCompletedTodo(td);
			RefreshHistory();
		}

		// METHOD  ///////////////////////////////////// RefreshHistory() //
		private void RefreshHistory()
		{
			tbHNotes.Text = _hCurrentHistoryItem.Notes;
			lbHCompletedTodos.ItemsSource = _hCurrentHistoryItem.CompletedTodos;
			lbHCompletedTodos.Items.Refresh();
			lbHHistory.Items.Refresh();
		}

		// METHOD  ///////////////////////////////////// DeleteTodo() //
		private void btnHDeleteTodo_Click(object sender, EventArgs e)
		{
			
		}

		// METHOD  ///////////////////////////////////// DeleteHistory() //
		private void btnHDeleteHistory_Click(object sender, EventArgs e)
		{
			
		}

		// METHOD  ///////////////////////////////////// NewHistory() //
		private void btnHNewHistory_Click(object sender, EventArgs e)
		{
			AddNewHistoryItem();
		}
		private void AddNewHistoryItem()
		{
			_hCurrentHistoryItem = new HistoryItem(DateTime.Now);
			_hHistoryList.Add(_hCurrentHistoryItem);
			RefreshHistory();
		}

		// METHOD  ///////////////////////////////////// TODO TAB() //
		// METHOD  ///////////////////////////////////// btnT1Add() //
		private void btnTAdd_Click(object sender, EventArgs e)
		{
			TodoItem td = new TodoItem() {Todo = txtT1NewTodo.Text, Severity = _tCurrentSeverity};
			
			_tIncompleteTodoList.Add(td);
			ResortTodoList();
			txtT1NewTodo.Clear();
		}
		
		// METHOD  ///////////////////////////////////// Delete() //
		private void mnuTDelete_Click(object sender, EventArgs e)
		{
			MenuItem v = sender as MenuItem;
			if ((string) v.CommandParameter == "incomplete")
			{
				_tIncompleteTodoList.RemoveAt(lbTIncompleteTodos.SelectedIndex);
			}
			else if ((string) v.CommandParameter == "complete")
			{
				_tCompleteTodoList.RemoveAt(lbTCompleteTodos.SelectedIndex);
			}
			ResortTodoList();
		}

		// METHOD  ///////////////////////////////////// Edit() //
		private void mnuTEdit_Click(object sender, EventArgs e)
		{
			MenuItem v = sender as MenuItem;
			if ((string) v.CommandParameter == "incomplete")
			{
				TodoItemEditor tdie = new TodoItemEditor(_tIncompleteTodoList[lbTIncompleteTodos.SelectedIndex]);
				tdie.ShowDialog();
				if (tdie.Result.Severity != 0)
				{
					_tIncompleteTodoList.RemoveAt(lbTIncompleteTodos.SelectedIndex);
					_tIncompleteTodoList.Add(tdie.Result);
				}
			}
			else if ((string) v.CommandParameter == "complete")
			{
//				completeTodoList.RemoveAt(lbT1CompleteTodos.SelectedIndex);
			}
			ResortTodoList();
		}
		
		// METHOD  ///////////////////////////////////// Severity() //
		private void rdoTSeverity_Checked(object sender, EventArgs e)
		{
			RadioButton rb = sender as RadioButton;
			_tCurrentSeverity = Convert.ToInt16(rb.CommandParameter.ToString());
		}
		
		// METHOD  ///////////////////////////////////// btnT1Complete() //
		private void btnTComplete_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			object item = b.DataContext;
			
			int index;
			TodoItem td = new TodoItem();
			if ((string) b.CommandParameter == "incomplete")
			{
				index = lbTIncompleteTodos.Items.IndexOf(item);
				td = _tIncompleteTodoList[index];
			}
			else if ((string) b.CommandParameter == "complete")
			{
				index = lbTCompleteTodos.Items.IndexOf(item);
				td = _tCompleteTodoList[index];
			}

			td.IsComplete = !td.IsComplete;
			td.DateCompleted = td.IsComplete ? DateTime.Now.ToString(DATE) : "-";
			td.TimeCompleted = td.IsComplete ? DateTime.Now.ToString(TIME) : "-";

			AddTodoToHistory(td);
			ResortTodoList();
		}
		
		// METHOD  ///////////////////////////////////// Sort() //
		private void btnTSort_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			if (_tCurrentSort != (string) b.CommandParameter)
			{
				_tReverseSort = false;
				_tCurrentSort = (string) b.CommandParameter;
			}

			if ((string) b.CommandParameter == "hash")
			{
				_tCurrentHashTagSortIndex++;
				if (_tCurrentHashTagSortIndex >= _tHashTags.Count)
					_tCurrentHashTagSortIndex = 0;
			}
			
			_tReverseSort = !_tReverseSort;
			ResortTodoList();
		}
		private List<TodoItem> SortByHashTag()
		{
			if (_tDidHashChange)
				_tCurrentHashTagSortIndex = 0;
			
			List<TodoItem> incompleteItems = new List<TodoItem>();
			List<string> sortedHashTags = new List<string>();

			for (int i = 0 + _tCurrentHashTagSortIndex; i < _tHashTags.Count; i++)
			{
				sortedHashTags.Add(_tHashTags[i]);
			}
			for (int i = 0; i < _tCurrentHashTagSortIndex; i++)
			{
				sortedHashTags.Add(_tHashTags[i]);
			}

			foreach (string s in sortedHashTags)
			{
				List<TodoItem> temp = _tIncompleteTodoList.ToList();
				for (int i = 0; i < _tIncompleteTodoList.Count; i++)
				{
					TodoItem td = temp[i];
					foreach (string h in td.Tags)
					{
						if (h.Equals(s))
						{
							incompleteItems.Add(td);
							_tIncompleteTodoList.Remove(td);
						}
					}
				}
			}
			foreach (TodoItem td in _tIncompleteTodoList)
			{
				incompleteItems.Add(td);
			}
			return incompleteItems;
		}
		private void ResortTodoList()
		{
			List<TodoItem> completeItems = new List<TodoItem>();
			List<TodoItem> incompleteItems = new List<TodoItem>();
			List<string> previousHashs = _tHashTags.OrderBy(o => o).ToList();
			_tHashTags.Clear();

			_tIncompleteTodoList.InsertRange(0, _tCompleteTodoList);
			foreach (TodoItem td in _tIncompleteTodoList)
			{
				foreach (string s in td.Tags)
				{
					if (!_tHashTags.Contains(s))
					{
						_tHashTags.Add(s);
					}
				}
				if (td.IsComplete)
					completeItems.Add(td);
				else
					incompleteItems.Add(td);
			}
			
			_tHashTags = _tHashTags.OrderBy(o => o).ToList();
			_tDidHashChange = false;
			if (_tHashTags.Count != previousHashs.Count)
				_tDidHashChange = true;
			else
			{
				for (int i = 0; i < _tHashTags.Count; i++)
				{
					if (_tHashTags[i] != previousHashs[i])
					{
						_tDidHashChange = true;
					}
				}
			}
			
			switch (_tCurrentSort)
			{
				case "sev":
					incompleteItems = _tReverseSort ? incompleteItems.OrderByDescending(o => o.Severity).ToList() : incompleteItems.OrderBy(o => o.Severity).ToList();
					break;
				case "date":
					incompleteItems = _tReverseSort ? incompleteItems.OrderByDescending(o => o.TimeStarted).ToList() : incompleteItems.OrderBy(o => o.TimeStarted).ToList();
					incompleteItems = _tReverseSort ? incompleteItems.OrderByDescending(o => o.DateStarted).ToList() : incompleteItems.OrderBy(o => o.DateStarted).ToList();
					break;
				case "hash":
					incompleteItems = SortByHashTag();
					break;
			}
			
			completeItems = completeItems.OrderBy(o => o.TimeCompleted).ToList();
			completeItems = completeItems.OrderBy(o => o.DateCompleted).ToList();
			
			_tIncompleteTodoList.Clear();
			_tCompleteTodoList.Clear();
			foreach (TodoItem td in incompleteItems)
				_tIncompleteTodoList.Add(td);
			foreach (TodoItem td in completeItems)
				_tCompleteTodoList.Add(td);
			lbTIncompleteTodos.Items.Refresh();
			lbTCompleteTodos.Items.Refresh();
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

			foreach (TodoItem td in _tIncompleteTodoList)
			{
				stream.WriteLine(td.ToString());
			}
			stream.WriteLine("====================================");
			foreach (TodoItem td in _tCompleteTodoList)
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

			_tIncompleteTodoList.Clear();
			_tCompleteTodoList.Clear();
			
			string line = stream.ReadLine();

			while (line != null)
			{
				if (line == "====================================")
				{
					line = stream.ReadLine();
					continue;
				}
				TodoItem td = new TodoItem(line);
				_tIncompleteTodoList.Add(td);
				line = stream.ReadLine();
			}

			stream.Close();
			
			ResortTodoList();
		}
	}
}