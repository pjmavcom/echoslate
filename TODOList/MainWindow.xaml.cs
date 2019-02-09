using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;
using MenuItem = System.Windows.Controls.MenuItem;
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

		private bool _tReverseSort;
		private string _tCurrentSort = "sev";
		private int _tCurrentHashTagSortIndex = -1;
		private bool _tDidHashChange;
		
		
		// HISTORY TAB ITEMS
		private readonly List<HistoryItem> _hHistoryList;
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


			_tIncompleteTodoList.Add(new TodoItem(DateTime.Now, "Test1", 1));
			_tIncompleteTodoList.Add(new TodoItem(new DateTime(1994,1,2,15,55,42), "Test2", 1));
			_tIncompleteTodoList.Add(new TodoItem(new DateTime(1990,1,2,15,55,42), "Test3", 1));
			
			_tIncompleteTodoList.Add(new TodoItem(new DateTime(1999,1,2,15,55,42), "Test4", 1));
			_tIncompleteTodoList.Add(new TodoItem(new DateTime(1999,1,2,15,55,43), "Test5", 2));
			_tIncompleteTodoList.Add(new TodoItem(new DateTime(1999,1,2,15,55,44), "Test6", 3));

			
			
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
			RefreshHistory();
			_hCurrentHistoryItem = _hHistoryList[0];
			_hCurrentHistoryItem.AddCompletedTodo(td);
			RefreshHistory();
		}

		// METHOD  ///////////////////////////////////// Notes() //
		private void tbHNotes_LoseFocus(object sender, EventArgs e)
		{
			_hCurrentHistoryItem.Notes = tbHNotes.Text;
		}
			
			
		// METHOD  ///////////////////////////////////// lbHHistory_SelectionChanged() //
		private void lbHHistory_SelectionChanged(object sender, EventArgs e)
		{
			int index = lbHHistory.SelectedIndex;
			if (_hHistoryList.Count > 0 && index >= _hHistoryList.Count)
				index = _hHistoryList.Count - 1;
			if (_hHistoryList.Count == 0) 
			{
				_hCurrentHistoryItem = new HistoryItem("", "");
				return;
			}
			if (index < 0)
			{
				index = 0;
			}

			_hCurrentHistoryItem = lbHHistory.Items[index] as HistoryItem;
			RefreshHistory();
		}
		
		// METHOD  ///////////////////////////////////// RefreshHistory() //
		private void RefreshHistory()
		{
			List<HistoryItem> sorted = _hHistoryList.OrderByDescending(o => o.DateTimeAdded).ToList();
			_hHistoryList.Clear();
			foreach (HistoryItem hi in sorted)
				_hHistoryList.Add(hi);
			
			tbHNotes.Text = _hCurrentHistoryItem.Notes;
			lbHCompletedTodos.ItemsSource = _hCurrentHistoryItem.CompletedTodos;
			lbHCompletedTodos.Items.Refresh();
			
			lbHHistory.Items.Refresh();
		}

		// METHOD  ///////////////////////////////////// DeleteTodo() //
		private void btnHDeleteTodo_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			TodoItem td = b?.DataContext as TodoItem;
			if(MessageBox.Show("Delete", "Are you sure", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
				_hCurrentHistoryItem.CompletedTodos.Remove(td);
			RefreshHistory();
		}

		// METHOD  ///////////////////////////////////// DeleteHistory() //
		private void btnHDeleteHistory_Click(object sender, EventArgs e)
		{
			if (_hHistoryList.Count == 0)
				return;
			if (MessageBox.Show("Delete", "Are you sure", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
				return;

			_hHistoryList.Remove(_hCurrentHistoryItem);

			_hCurrentHistoryItem = _hHistoryList.Count > 0 ? _hHistoryList[0] : new HistoryItem("", "");
			RefreshHistory();
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

		// METHOD  ///////////////////////////////////// CopyHistory() //
		private void btnHCopyHistory_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			if (b?.DataContext is HistoryItem item)
				Clipboard.SetText(item.ToClipboard());
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
			if ((string) v?.CommandParameter == "incomplete")
			{
				_tIncompleteTodoList.RemoveAt(lbTIncompleteTodos.SelectedIndex);
			}
			else if ((string) v?.CommandParameter == "complete")
			{
				_tCompleteTodoList.RemoveAt(lbTCompleteTodos.SelectedIndex);
			}
			ResortTodoList();
		}

		// METHOD  ///////////////////////////////////// Edit() //
		private void mnuTEdit_Click(object sender, EventArgs e)
		{
			MenuItem v = sender as MenuItem;
			if ((string) v?.CommandParameter == "incomplete")
			{
				TodoItemEditor tdie = new TodoItemEditor(_tIncompleteTodoList[lbTIncompleteTodos.SelectedIndex]);
				tdie.ShowDialog();
				if (tdie.Result.Severity != 0)
				{
					_tIncompleteTodoList.RemoveAt(lbTIncompleteTodos.SelectedIndex);
					_tIncompleteTodoList.Add(tdie.Result);
				}
			}
			else if ((string) v?.CommandParameter == "complete")
			{
				TodoItemEditor tdie = new TodoItemEditor(_tCompleteTodoList[lbTCompleteTodos.SelectedIndex]);
				tdie.ShowDialog();
				if (tdie.Result.Severity != 0)
				{
					_tCompleteTodoList.RemoveAt(lbTCompleteTodos.SelectedIndex);
					_tCompleteTodoList.Add(tdie.Result);
				}
			}
			ResortTodoList();
		}
		
		// METHOD  ///////////////////////////////////// Severity() //
		private void rdoTSeverity_Checked(object sender, EventArgs e)
		{
			RadioButton rb = sender as RadioButton;
			_tCurrentSeverity = Convert.ToInt16(rb?.CommandParameter.ToString());
		}
		
		// METHOD  ///////////////////////////////////// btnT1Complete() //
		private void btnTComplete_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			object item = b?.DataContext;
			
			int index = 0;
			TodoItem td = new TodoItem();
			if ((string) b?.CommandParameter == "incomplete")
			{
				index = lbTIncompleteTodos.Items.IndexOf(item);
				td = _tIncompleteTodoList[index];
			}
			else if ((string) b?.CommandParameter == "complete")
			{
				if (item != null)
					index = lbTCompleteTodos.Items.IndexOf(item);
				td = _tCompleteTodoList[index];
			}

			td.IsComplete = !td.IsComplete;
			td.DateCompleted = td.IsComplete ? DateTime.Now.ToString(DATE) : "-";
			td.TimeCompleted = td.IsComplete ? DateTime.Now.ToString(TIME) : "-";

			if((string) b?.CommandParameter == "incomplete")
				AddTodoToHistory(td);
			
			ResortTodoList();
		}
		
		// METHOD  ///////////////////////////////////// Sort() //
		private void btnTSort_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			if (_tCurrentSort != (string) b?.CommandParameter)
			{
				_tReverseSort = false;
				_tCurrentSort = (string) b?.CommandParameter;
			}

			if ((string) b?.CommandParameter == "hash")
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
				foreach (TodoItem td in temp)
				{
					foreach (string t in td.Tags)
					{
						if (!s.Equals(t))
							continue;
						incompleteItems.Add(td);
						_tIncompleteTodoList.Remove(td);
					}
				}
			}
			foreach (TodoItem td in _tIncompleteTodoList)
				incompleteItems.Add(td);
			return incompleteItems;
		}
		private void ResortTodoList()
		{
			List<TodoItem> completeItems = new List<TodoItem>();
			List<TodoItem> incompleteItems = new List<TodoItem>();
			List<string> hashTagList = new List<string>();
			
			_tIncompleteTodoList.InsertRange(0, _tCompleteTodoList);
			foreach (TodoItem td in _tIncompleteTodoList)
			{
				if (td.IsComplete)
					completeItems.Add(td);
				else
				{
					incompleteItems.Add(td);
					foreach (string s in td.Tags)
					{
						if (!hashTagList.Contains(s))
							hashTagList.Add(s);
					}
				}
			}
			_tIncompleteTodoList = incompleteItems;
			_tCompleteTodoList = completeItems;
			
			hashTagList = hashTagList.OrderBy(o => o).ToList();
			_tHashTags = _tHashTags.OrderBy(o => o).ToList();
			_tDidHashChange = false;
			if (_tHashTags.Count != hashTagList.Count)
				_tDidHashChange = true;
			else
				for (int i = 0; i < _tHashTags.Count; i++)
					if (_tHashTags[i] != hashTagList[i])
						_tDidHashChange = true;
			_tHashTags = hashTagList;
			
			switch (_tCurrentSort)
			{
				case "sev":
					_tIncompleteTodoList = _tReverseSort ? _tIncompleteTodoList.OrderByDescending(o => o.Severity).ToList() : _tIncompleteTodoList.OrderBy(o => o.Severity).ToList();
					break;
				case "date":
					_tIncompleteTodoList = _tReverseSort ? _tIncompleteTodoList.OrderByDescending(o => o.TimeStarted).ToList() : _tIncompleteTodoList.OrderBy(o => o.TimeStarted).ToList();
					_tIncompleteTodoList = _tReverseSort ? _tIncompleteTodoList.OrderByDescending(o => o.DateStarted).ToList() : _tIncompleteTodoList.OrderBy(o => o.DateStarted).ToList();
					break;
				case "hash":
					_tIncompleteTodoList = SortByHashTag();
					break;
			}
			
			_tCompleteTodoList = _tCompleteTodoList.OrderBy(o => o.TimeCompleted).ToList();
			_tCompleteTodoList = _tCompleteTodoList.OrderBy(o => o.DateCompleted).ToList();
			lbTIncompleteTodos.ItemsSource = _tIncompleteTodoList;
			lbTIncompleteTodos.Items.Refresh();
			lbTCompleteTodos.ItemsSource = _tCompleteTodoList;
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
			
			stream.WriteLine("====================================VCS");
			foreach (HistoryItem hi in _hHistoryList)
			{
				stream.Write(hi.ToString());
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
				if (line == "====================================VCS")
				{
					line = stream.ReadLine();
					break;
				}
				TodoItem td = new TodoItem(line);
				_tIncompleteTodoList.Add(td);
				line = stream.ReadLine();
			}
			
			List<string> history = new List<string>();
			while (line != null)
			{
				if (line == "NewVCS")
				{
					history = new List<string>();
					line = stream.ReadLine();
					continue;
				}
				if (line == "EndVCS")
				{
					_hHistoryList.Add(new HistoryItem(history));
					line = stream.ReadLine();
					continue;
				}
				history.Add(line);
				line = stream.ReadLine();
			}

			stream.Close();
			
			ResortTodoList();
			RefreshHistory();
		}
	}
}