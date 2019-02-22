using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Button = System.Windows.Controls.Button;
using Clipboard = System.Windows.Forms.Clipboard;
using ComboBox = System.Windows.Controls.ComboBox;
using ListBox = System.Windows.Controls.ListBox;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.Forms.MessageBox;
using ProgressBar = System.Windows.Forms.ProgressBar;
using RadioButton = System.Windows.Controls.RadioButton;


namespace TODOList
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>

	
	public partial class MainWindow
	{
		public const string DATE = "yyyMMdd";
		public const string TIME = "HHmmss";
		public const string VERSION = "1.2a";

		private bool _isChanged = false;
		
		// Hotkey stuff
		private const int HOTKEY_ID = 9000;
		private const uint MOD_NONE = 0x0000; //[NONE]
		private const uint MOD_ALT = 0x0001; //ALT
		private const uint MOD_CONTROL = 0x0002; //CTRL
		private const uint MOD_SHIFT = 0x0004; //SHIFT
		private const uint MOD_WIN = 0x0008; //WINDOWS
		private const uint VK_CAPITAL = 0x14;
		private HwndSource source;
		
		// TODO TAB ITEMS
		private List<TodoItem> _tIncompleteItems;
//		private List<TodoItem> _tCompleteTodoList;
		private List<string> _tHashTags;
		
		private int _tCurrentSeverity;

		private bool _tReverseSort = false;
		private string _tCurrentSort = "rank";
		private int _tCurrentHashTagSortIndex = -1;
		private bool _tDidHashChange;
		private string _hashToSortBy = "";
		private bool _hashSortSelected;

		private DispatcherTimer _timer;
		
		// HISTORY TAB ITEMS
		private List<HistoryItem> _hHistoryItems;
		private HistoryItem _hCurrentHistoryItem;
		
		// FILE IO
		private const string basePath = @"C:\MyBinaries\";
		private List<string> recentFiles;
		private string currentOpenFile;

		private double top = 0;
		private double left = 0;
		private double width = 1080;
		private double height = 1920;
		private bool maximized = false;

		public string WindowTitle => "TodoList v" + VERSION + " " + currentOpenFile;

		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
		
		public List<string> RecentFiles
		{
			get => recentFiles;
			set => recentFiles = value;
		}

		public MainWindow()
		{
			Closing += Window_Closed;
			InitializeComponent();
			LoadSettings();
			this.DataContext = this;

			Top = top;
			Left = left;
			Height = height;
			Width = width;
			
			_tIncompleteItems = new List<TodoItem>();
//			_tCompleteTodoList = new List<TodoItem>();
			_hHistoryItems = new List<HistoryItem>();
			_tHashTags = new List<string>();
			
			_hCurrentHistoryItem = new HistoryItem("", "");

			_timer = new DispatcherTimer();
			_timer.Tick += _timer_Tick;
			_timer.Interval = new TimeSpan(TimeSpan.TicksPerSecond);
			_timer.Start();

			
			
			cbSaveFiles.ItemsSource = recentFiles;
			cbLoadFiles.ItemsSource = recentFiles;
			cbSaveFiles.Items.Refresh();
			cbLoadFiles.Items.Refresh();

			cbHashtags.ItemsSource = _tHashTags;
			lbTIncompleteItems.ItemsSource = _tIncompleteItems;
//			lbTCompleteTodos.ItemsSource = _tCompleteTodoList;
			
			lbHHistory.ItemsSource = _hHistoryItems;
			ResortTodoList();
			
			Load(recentFiles[0]);

			ProgressBar pb = new ProgressBar();
			
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			IntPtr handle = new WindowInteropHelper(this).Handle;
			source = HwndSource.FromHwnd(handle);
			source.AddHook(HwndHook);
			RegisterHotKey(handle, HOTKEY_ID, MOD_WIN, 0x73);
		}
		private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			const int WM_HOTKEY = 0x0312;
			switch (msg)
			{
				case WM_HOTKEY:
					switch (wParam.ToInt32())
					{
						case HOTKEY_ID:
							int vkey = (((int) lParam >> 16) & 0xFFFF);
							if (vkey == 0x73)
							{
								Activate();
								FocusManager.SetFocusedElement(FocusManager.GetFocusScope(tbHNotes), tbHNotes);
								txtT1NewTodo.Focus();
								Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate() { txtT1NewTodo.Focus(); }));
							}
							handled = true;
							break;
					}
					break;
			}
			return IntPtr.Zero;
		}
		
		// METHOD  ///////////////////////////////////// Timer() //
		private void _timer_Tick(object sender, EventArgs e)
		{
			foreach (TodoItem td in _tIncompleteItems)
			{
				if (td.IsTimerOn)
				{
					td.TimeTaken = td.TimeTaken.AddSeconds(1);
					
				}
			}
//			lbTIncompleteItems.Items.Refresh();
		}
		// METHOD  ///////////////////////////////////// HISTORY TAB() //
		// METHOD  ///////////////////////////////////// AddTodoToHistory() //
		private void AddTodoToHistory(TodoItem td)
		{
			if (_hCurrentHistoryItem.DateAdded == "")
				AddNewHistoryItem();
			RefreshHistory();
			_hCurrentHistoryItem = _hHistoryItems[0];
			_hCurrentHistoryItem.AddCompletedTodo(td);
			RefreshHistory();
			_isChanged = true;
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
			if (_hHistoryItems.Count > 0 && index >= _hHistoryItems.Count)
				index = _hHistoryItems.Count - 1;
			if (_hHistoryItems.Count == 0) 
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
			_hHistoryItems = _hHistoryItems.OrderByDescending(o => o.DateTimeAdded).ToList();
			
			if (_hHistoryItems.Count == 0)
				_hCurrentHistoryItem = new HistoryItem("", "");

			if (_hHistoryItems.Count > 0 && _hCurrentHistoryItem.DateAdded == "")
				lbHHistory.SelectedIndex = 0;
			
			tbHNotes.Text = _hCurrentHistoryItem.Notes;
			lbHCompletedTodos.ItemsSource = _hCurrentHistoryItem.CompletedTodos;
			lbHCompletedTodos.Items.Refresh();

			lbHHistory.ItemsSource = _hHistoryItems;
			lbHHistory.Items.Refresh();
			_isChanged = true;
		}

		// METHOD  ///////////////////////////////////// DeleteTodo() //
		private void btnHDeleteTodo_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			TodoItem td = b?.DataContext as TodoItem;
			if (MessageBox.Show("Delete", "Are you sure", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
				return;

			td.IsComplete = false;
			_tIncompleteItems.Add(td);
			td.Rank = _tIncompleteItems.Count;
			ResortTodoList();
			_hCurrentHistoryItem.CompletedTodos.Remove(td);
			RefreshHistory();
		}

		// METHOD  ///////////////////////////////////// DeleteHistory() //
		private void btnHDeleteHistory_Click(object sender, EventArgs e)
		{
			if (_hHistoryItems.Count == 0)
				return;
			if (MessageBox.Show("Delete", "Are you sure", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
				return;

			_hHistoryItems.Remove(_hCurrentHistoryItem);

			_hCurrentHistoryItem = _hHistoryItems.Count > 0 ? _hHistoryItems[0] : new HistoryItem("", "");
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
			_hHistoryItems.Add(_hCurrentHistoryItem);
			RefreshHistory();
		}

		// METHOD  ///////////////////////////////////// CopyHistory() //
		private void btnHCopyHistory_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			if (b?.DataContext is HistoryItem item)
				Clipboard.SetText(item.ToClipboard());
			if (lbHHistory.SelectedIndex == 0)
				AddNewHistoryItem();
		}
		// METHOD  ///////////////////////////////////// TODO TAB() //
		// METHOD  ///////////////////////////////////// ProgressBar() //
		private void ProgressBar()
		{
			
		}
		// METHOD  ///////////////////////////////////// btnT1Add() //
		private void btnTAdd_Click(object sender, EventArgs e)
		{
			TodoItem td = new TodoItem() {Todo = txtT1NewTodo.Text, Severity = _tCurrentSeverity};
			
			_tIncompleteItems.Add(td);
			td.Rank = _tIncompleteItems.Count;
			ResortTodoList();
			txtT1NewTodo.Clear();
		}
		
		// METHOD  ///////////////////////////////////// Delete() //
		private void mnuTDelete_Click(object sender, EventArgs e)
		{
			_tIncompleteItems.RemoveAt(lbTIncompleteItems.SelectedIndex);
			ResortTodoList();
		}

		// METHOD  ///////////////////////////////////// Edit() //
		private void mnuTEdit_Click(object sender, EventArgs e)
		{
			EditItem(sender, _tIncompleteItems);
		}
		
		private void mnuHEdit_Click(object sender, EventArgs e)
		{
			EditItem(sender, _hCurrentHistoryItem.CompletedTodos);
		}

		private void EditItem(object sender, List<TodoItem> list)
		{
			ListBox lb = sender as ListBox;
			int index = lb.SelectedIndex;
			if (index < 0)
				return;
			TodoItem td = list[index];
			TodoItemEditor tdie = new TodoItemEditor(td);
			
			tdie.ShowDialog();
			if (tdie.isOk)
			{
				list.Remove(td);
//				tdie.Result.Rank = _tIncompleteTodoList.Count;
				_tIncompleteItems.Add(tdie.Result);
			}
			
			ResortTodoList();
			RefreshHistory();
		}
		
		// METHOD  ///////////////////////////////////// Severity() //
		private void cbTSeverity_SelectionChanged(object sender, EventArgs e)
		{
			ComboBox rb = sender as ComboBox;

			_tCurrentSeverity = rb.SelectedIndex + 1;
		}
		
		// METHOD  ///////////////////////////////////// btnT1Complete() //
		private void btnTComplete_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			object item = b?.DataContext;
			
//			int index = 0;
//			TodoItem td = new TodoItem();

			int index = lbTIncompleteItems.Items.IndexOf(item);
			TodoItem td = _tIncompleteItems[index];

			TodoItemComplete tdc = new TodoItemComplete(td);
			tdc.ShowDialog();
			if (tdc.isOk)
			{
				_tIncompleteItems.RemoveAt(index);
				_tIncompleteItems.Add(tdc.Result);
			}
			
//			td.IsComplete = true;
			ResortTodoList();
		}

		// METHOD  ///////////////////////////////////// Rank() //
		private void btnRank_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			TodoItem td = b.DataContext as TodoItem;
			int index = lbTIncompleteItems.SelectedIndex;
			index = _tIncompleteItems.IndexOf(td);
			
			if ((string) b.CommandParameter == "up")
			{
				if (index == 0)
					return;
				int newRank = _tIncompleteItems[index - 1].Rank;
				_tIncompleteItems[index - 1].Rank = td.Rank;
				td.Rank = newRank;
			}
			else if ((string) b.CommandParameter == "down")
			{
				if (index >= _tIncompleteItems.Count)
					return;
				int newRank = _tIncompleteItems[index + 1].Rank;
				_tIncompleteItems[index + 1].Rank = td.Rank;
				td.Rank = newRank;
			}
			ResortTodoList();
		}

		// METHOD  ///////////////////////////////////// TimeTaken() //
		private void btnTimeTaken_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			TodoItem td = b.DataContext as TodoItem;
			int index = lbTIncompleteItems.SelectedIndex;

			if((string) b.CommandParameter == "start")
				td.IsTimerOn = true;
			else if ((string) b.CommandParameter == "stop")
				td.IsTimerOn = false;
			
			lbTIncompleteItems.Items.Refresh();
		}

		// METHOD  ///////////////////////////////////// ResetTimer() //
		private void mnuResetTimer_Click(object sender, EventArgs e)
		{
			int index = lbTIncompleteItems.SelectedIndex;
			if (index < 0)
				return;
			TodoItem td = _tIncompleteItems[index];
			td.TimeTaken = new DateTime();
			td.IsTimerOn = false;
			lbTIncompleteItems.Items.Refresh();
		}
		// METHOD  ///////////////////////////////////// Sort() //
		private void cbTHashtags_SelectionChanged(object sender, EventArgs e)
		{
			ComboBox cb = sender as ComboBox;
			if (cb == null)
				return;

			_hashToSortBy = _tHashTags[0];
			if (cb.SelectedItem != null)
				_hashToSortBy = cb.SelectedItem.ToString();
			_hashSortSelected = true;
			_tCurrentSort = "hash";
			ResortTodoList();
		}
		
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
			if (_hashSortSelected)
			{
				_tCurrentHashTagSortIndex = 0;
				foreach (string s in _tHashTags)
				{
					if (s.Equals(_hashToSortBy))
						break;
					_tCurrentHashTagSortIndex++;
				}
			}
				
			for (int i = 0 + _tCurrentHashTagSortIndex; i < _tHashTags.Count; i++)
				sortedHashTags.Add(_tHashTags[i]);
			for (int i = 0; i < _tCurrentHashTagSortIndex; i++)
				sortedHashTags.Add(_tHashTags[i]);

			foreach (string s in sortedHashTags)
			{
				List<TodoItem> temp = _tIncompleteItems.ToList();
				foreach (TodoItem td in temp)
				{
					foreach (string t in td.Tags)
					{
						if (!s.Equals(t))
							continue;
						incompleteItems.Add(td);
						_tIncompleteItems.Remove(td);
					}
				}
			}
			foreach (TodoItem td in _tIncompleteItems)
				incompleteItems.Add(td);
			return incompleteItems;
		}
		private void FixRankings()
		{
			_tIncompleteItems = _tIncompleteItems.OrderBy(o => o.Rank).ToList();
			for (int i = 0; i < _tIncompleteItems.Count; i++)
			{
				_tIncompleteItems[i].Rank = i + 1;
			}

		}
		private void ResortTodoList()
		{
			// TODO: Get rid of this completeItems
//			List<TodoItem> completeItems = new List<TodoItem>();
			List<TodoItem> incompleteItems = new List<TodoItem>();
			List<string> hashTagList = new List<string>();
			
//			_tIncompleteTodoList.InsertRange(0, _tCompleteTodoList);
			foreach (TodoItem td in _tIncompleteItems)
			{
				if (td.IsComplete)
				{
					td.Rank = int.MaxValue;
					AddTodoToHistory(td);
				}
//					completeItems.Add(td);
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
			_tIncompleteItems = incompleteItems;
//			_tCompleteTodoList = completeItems;
			
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

			FixRankings();
			
			switch (_tCurrentSort)
			{
				case "sev":
					_tIncompleteItems = _tReverseSort ? _tIncompleteItems.OrderByDescending(o => o.Severity).ToList() : _tIncompleteItems.OrderBy(o => o.Severity).ToList();
					break;
				case "date":
					_tIncompleteItems = _tReverseSort ? _tIncompleteItems.OrderByDescending(o => o.TimeStarted).ToList() : _tIncompleteItems.OrderBy(o => o.TimeStarted).ToList();
					_tIncompleteItems = _tReverseSort ? _tIncompleteItems.OrderByDescending(o => o.DateStarted).ToList() : _tIncompleteItems.OrderBy(o => o.DateStarted).ToList();
					break;
				case "hash":
					_tIncompleteItems = SortByHashTag();
					break;
				case "rank":
					_tIncompleteItems = _tReverseSort ? _tIncompleteItems.OrderByDescending(o => o.Rank).ToList() : _tIncompleteItems.OrderBy(o => o.Rank).ToList();
					break;
				case "active":
					_tIncompleteItems = _tReverseSort ? _tIncompleteItems.OrderByDescending(o => o.IsTimerOn).ToList() : _tIncompleteItems.OrderBy(o => o.IsTimerOn).ToList();
					break;
			}
			
//			_tCompleteTodoList = _tCompleteTodoList.OrderBy(o => o.TimeCompleted).ToList();
//			_tCompleteTodoList = _tCompleteTodoList.OrderBy(o => o.DateCompleted).ToList();
			lbTIncompleteItems.ItemsSource = _tIncompleteItems;
			lbTIncompleteItems.Items.Refresh();
//			lbTCompleteTodos.ItemsSource = _tCompleteTodoList;
//			lbTCompleteTodos.Items.Refresh();
			cbHashtags.ItemsSource = _tHashTags;
			cbHashtags.Items.Refresh();
			_isChanged = true;
//			ColorTodos();
		}

		// METHOD  ///////////////////////////////////// NewFile() //
		private void mnuNew_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Are you sure?", "New File", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
				return;
			_hHistoryItems.Clear();
			_tIncompleteItems.Clear();
//			_tCompleteTodoList.Clear();

			_hCurrentHistoryItem = new HistoryItem("", ""); 
			RefreshHistory();
			ResortTodoList();

			MessageBox.Show("If you dont save as a new file, clicking save will overwrite the previous file. Dont feel like fixing that now");
			_isChanged = true;
			SaveAs();
		}
		
		// METHOD  ///////////////////////////////////// Save() //
		private void cbSaveFiles_SelectionChanged(object sender, EventArgs e)
		{
			var lb = sender as ComboBox;
			int index = lb.SelectedIndex;

			if (MessageBox.Show("Save over " + recentFiles[index], "Are you sure you want to save?", MessageBoxButtons.YesNo) ==
				System.Windows.Forms.DialogResult.No)
				return;
			Save(recentFiles[index]);
		}
		private void mnuSaveAs_Click(object sender, EventArgs e)
		{
			SaveAs();
		}
		private void mnuSave_Click(object sender, EventArgs e)
		{
			Save(recentFiles[0]);
		}
		private void SaveAs()
		{
			SaveFileDialog sfd = new SaveFileDialog
			{
				Title  = "Select folder to save game in.",
				FileName = basePath
			};

			DialogResult dr = sfd.ShowDialog();

			if (dr != System.Windows.Forms.DialogResult.OK)
				return;
			Save(sfd.FileName);
		}
		private void Save(string path)
		{
			SortRecentFiles(path);
			SaveSettings();
			
			StreamWriter stream   = new StreamWriter(File.Open(path, FileMode.Create));

			foreach (TodoItem td in _tIncompleteItems)
			{
				stream.WriteLine(td.ToString());
			}
//			stream.WriteLine("====================================");
//			foreach (TodoItem td in _tCompleteTodoList)
//			{
//				stream.WriteLine(td.ToString());
//			}
			
			stream.WriteLine("====================================VCS");
			foreach (HistoryItem hi in _hHistoryItems)
			{
				stream.Write(hi.ToString());
			}
			stream.Close();
			currentOpenFile = path;
			Title = WindowTitle;
			_isChanged = false;
		}
		// METHOD  ///////////////////////////////////// Load() //
		public void cbLoadFiles_SelectionChanged(object sender, EventArgs e)
		{
			var lb = sender as ComboBox;
			int index = lb.SelectedIndex;
			if (MessageBox.Show("Load " + recentFiles[index], "Are you sure you want to load?", MessageBoxButtons.YesNo) ==
				System.Windows.Forms.DialogResult.No)
				return;
			Load(recentFiles[index]);
		}
		private void mnuLoad_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog
			{
				Title = "Open file: ",
				FileName = basePath
			};

			DialogResult dr = ofd.ShowDialog();
			
			if (dr != System.Windows.Forms.DialogResult.OK)
				return;
			Load(ofd.FileName);
		}
		private void Load(string path)
		{
			SortRecentFiles(path);
			SaveSettings();
			
			StreamReader stream = new StreamReader(File.Open(path, FileMode.Open));

			_tIncompleteItems.Clear();
//			_tCompleteTodoList.Clear();
			_hHistoryItems.Clear();
			
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
				_tIncompleteItems.Add(td);
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
					_hHistoryItems.Add(new HistoryItem(history));
					line = stream.ReadLine();
					continue;
				}
				history.Add(line);
				line = stream.ReadLine();
			}
			stream.Close();
			
			ResortTodoList();
			RefreshHistory();
			currentOpenFile = path;
			Title = WindowTitle;
			
			_isChanged = false;
		}

		// METHOD  ///////////////////////////////////// Settings() //
		private void LoadSettings()
		{
			recentFiles = new List<string>();
			StreamReader stream = new StreamReader(File.Open(basePath + "TDHistory.settings", FileMode.Open));
			string line = stream.ReadLine();
			recentFiles.Clear();
			while (line != null)
			{
				recentFiles.Add(line);
				line = stream.ReadLine();
				if (line == "WINDOWPOSITION")
					break;
			}

			top = Convert.ToDouble(stream.ReadLine());
			left = Convert.ToDouble(stream.ReadLine());
			height = Convert.ToDouble(stream.ReadLine());
			width = Convert.ToDouble(stream.ReadLine());
			
			stream.Close();
			cbSaveFiles.Items.Refresh();
			cbLoadFiles.Items.Refresh();
		}
		private void SaveSettings()
		{
			StreamWriter stream = new StreamWriter(File.Open(basePath + "TDHistory.settings", FileMode.Create));
			foreach (string s in recentFiles)
				stream.WriteLine(s);
			stream.WriteLine("WINDOWPOSITION");
			stream.WriteLine(Top);
			stream.WriteLine(Left);
			stream.WriteLine(Height);
			stream.WriteLine(Width);
			stream.Close();
			cbSaveFiles.Items.Refresh();
			cbLoadFiles.Items.Refresh();
		}
		// METHOD  ///////////////////////////////////// SortRecentFiles() //
		private void SortRecentFiles(string recent)
		{
			if (recentFiles.Contains(recent))
				recentFiles.Remove(recent);
				
			recentFiles.Insert(0, recent);
				
			while (recentFiles.Count >= 10)
			{
				recentFiles.RemoveAt(recentFiles.Count - 1);
			}
		}

		// METHOD  ///////////////////////////////////// OnExit() //
		public void Window_Closed(object sender, CancelEventArgs e)
		{
			SaveSettings();
			if(_isChanged)
				if (MessageBox.Show("Maybe save first?", "Close", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
					e.Cancel = true;
		}
	}
	
}