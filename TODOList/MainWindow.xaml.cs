using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Button = System.Windows.Controls.Button;
using Clipboard = System.Windows.Forms.Clipboard;
using ComboBox = System.Windows.Controls.ComboBox;
using ListBox = System.Windows.Controls.ListBox;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.Forms.MessageBox;


namespace TODOList
{
	public partial class MainWindow
	{
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
		public const string DATE = "yyyMMdd";
		public const string TIME = "HHmmss";
		public const string VERSION = "1.4b";

		// TO DO TAB ITEMS
		private List<TodoItem> _tIncompleteItems;
		private List<string> _tHashTags;
		private int _tCurrentSeverity;

		// Sorting
		private bool _tReverseSort;
		private string _tCurrentSort = "rank";
		private int _tCurrentHashTagSortIndex = -1;
		private bool _tDidHashChange;
		private string _hashToSortBy = "";
		private bool _hashSortSelected;

		// HISTORY TAB ITEMS
		private List<HistoryItem> _hHistoryItems;
		private HistoryItem _hCurrentHistoryItem;

		// FILE IO
		private const string basePath = @"C:\MyBinaries\";
		private ObservableCollection<string> _recentFiles;
		private string _currentOpenFile;
		private bool _isChanged;
		private int _recentFilesIndex;

		// WINDOW ITEMS
		private double top;
		private double left;
		private double width = 1080;
		private double height = 1920;

		// HOTKEY STUFF
		private const int HOTKEY_ID = 9000;
		private const uint MOD_WIN = 0x0008; //WINDOWS
		private HwndSource source;
		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);


		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		public List<string> HashTags => _tHashTags;
		public List<TodoItem> IncompleteItems => _tIncompleteItems;
		public List<HistoryItem> HistoryItems => _hHistoryItems;
		public string WindowTitle => "TodoList v" + VERSION + " " + _currentOpenFile;
		public ObservableCollection<string> RecentFiles
		{
			get => _recentFiles;
			set => _recentFiles = value;
		}

		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public MainWindow()
		{
			left = 0;
			InitializeComponent();
			LoadSettings();
			DataContext = this;

			Top = top;
			Left = left;
			Height = height;
			Width = width;
			var timer = new DispatcherTimer();
			timer.Tick += Timer_Tick;
			timer.Interval = new TimeSpan(TimeSpan.TicksPerSecond);
			timer.Start();

			Closing += Window_Closed;

			_tIncompleteItems = new List<TodoItem>();
			_hHistoryItems = new List<HistoryItem>();
			_tHashTags = new List<string>();
			_hCurrentHistoryItem = new HistoryItem("", "");

			if (_recentFiles.Count > 0)
				Load(_recentFiles[0]);

			lbHHistory.SelectedIndex = 0;
			lbHCompletedTodos.SelectedIndex = 0;
			lbTIncompleteItems.SelectedIndex = 0;
		}

		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS //
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// MainWindow Methods //
		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			IntPtr handle = new WindowInteropHelper(this).Handle;
			source = HwndSource.FromHwnd(handle);
			if (source != null) 
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
								Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate { txtT1NewTodo.Focus(); }));
							}
							handled = true;
							break;
					}
					break;
			}
			return IntPtr.Zero;
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			foreach (TodoItem td in _tIncompleteItems)
				if (td.IsTimerOn)
					td.TimeTaken = td.TimeTaken.AddSeconds(1);
		}
		
		private void Window_Closed(object sender, CancelEventArgs e)
		{
			SaveSettings();
			if (!_isChanged)
				return;

			if (MessageBox.Show("Maybe save first?", "Close", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
				Save(_currentOpenFile);
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Hotkeys //
		private void hkSwitchTab(object sender, ExecutedRoutedEventArgs e)
		{
			int index = TabControl.SelectedIndex;
			if ((string) e.Parameter == "right")
			{
				index++;
				if (index >= TabControl.Items.Count)
					index = 0;
			}
			else if ((string) e.Parameter == "left")
			{
				index--;
				if (index < 0)
					index = TabControl.Items.Count - 1;
			}
			TabControl.SelectedIndex = index;
		}

		private void hkSwitchSeverity(object sender, ExecutedRoutedEventArgs e)
		{
			int index = cbSeverity.SelectedIndex;
			if ((string) e.Parameter == "down")
			{
				index++;
				if (index >= cbSeverity.Items.Count)
					index = cbSeverity.Items.Count - 1;
			}
			else if ((string) e.Parameter == "up")
			{
				index--;
				if (index < 0)
					index = 0;
			}
			cbSeverity.SelectedIndex = index;
			cbSeverity.Items.Refresh();
		}

		private void hkComplete(object sender, ExecutedRoutedEventArgs e)
		{
			if (tabHistory.IsSelected)
				return;

			TodoItem td = (TodoItem) lbTIncompleteItems.SelectedItem;
			if (td != null)
			{
				TodoItemComplete tdc = new TodoItemComplete(td);
				tdc.ShowDialog();
				if (tdc.isOk)
				{
					_tIncompleteItems.RemoveAt(lbTIncompleteItems.SelectedIndex);
					_tIncompleteItems.Add(tdc.Result);
				}
			}
			RefreshTodo();
		}

		private void hkAdd(object sender, ExecutedRoutedEventArgs e)
		{
			if (tabHistory.IsSelected)
				return;
			btnTAdd_Click(sender, e);
		}

		private void hkEdit(object sender, EventArgs e)
		{
			if(tabTodo.IsSelected)
				EditItem(lbTIncompleteItems, _tIncompleteItems);
			else if(tabHistory.IsSelected)
				EditItem(lbHCompletedTodos, _hCurrentHistoryItem.CompletedTodos);
		}

		private void hkQuickSave(object sender, ExecutedRoutedEventArgs e)
		{
			Save(_recentFiles[0]);
		} 


		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// MenuCommands //
		private void mnuNew_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Are you sure?", "New File", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
				return;
			_hHistoryItems.Clear();
			_tIncompleteItems.Clear();

			_hCurrentHistoryItem = new HistoryItem("", "");
			RefreshHistory();
			RefreshTodo();

			_currentOpenFile = "";
			Title = WindowTitle;
			_isChanged = true;
			SaveAs();
		}
		
		private void mnuRemoveFile_Click(object sender, RoutedEventArgs e)
		{
			if (_recentFilesIndex < 0)
				return;
			_recentFiles.RemoveAt(_recentFilesIndex);
			mnuRecentSaves.Items.Refresh();
			mnuRecentLoads.Items.Refresh();
		}

		private void mnuRecentSavesRMB(object sender, MouseButtonEventArgs e)
		{
			_recentFilesIndex = -1;
			var mi = e.OriginalSource as TextBlock;
			if (mi != null)
			{
				string path = (string) mi.DataContext;
				_recentFilesIndex = mnuRecentSaves.Items.IndexOf(path);
			}
		}
		
		private void mnuRecentLoadsRMB(object sender, MouseButtonEventArgs e)
		{
			_recentFilesIndex = -1;
			var mi = e.OriginalSource as TextBlock;
			if (mi != null)
			{
				string path = (string) mi.DataContext;
				_recentFilesIndex = mnuRecentLoads.Items.IndexOf(path);
			}
		}
		
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
		
		private void mnuQuit_Click(object sender, EventArgs e)
		{
			Close();
		}
		
		private void mnuSaveFiles_Click(object sender, RoutedEventArgs e)
		{
			if (_recentFilesIndex < 0)
				return;
			
			MenuItem mi = (MenuItem) e.OriginalSource;
			var path = mi.DataContext as string;
			if (path == null)
				return;
			
			if (MessageBox.Show("Save over " + path, "Are you sure you want to save?", MessageBoxButtons.YesNo) ==
				System.Windows.Forms.DialogResult.No)
				return;
			Save(path);
		}
		
		private void mnuSaveAs_Click(object sender, EventArgs e)
		{
			SaveAs();
		}

		private void mnuSave_Click(object sender, EventArgs e)
		{
			if (_currentOpenFile == "")
			{
				MessageBox.Show("No current file", "Nope");
				return;
			}
			Save(_currentOpenFile);
		}
		
		private void mnuLoadFiles_Click(object sender, RoutedEventArgs e)
		{
			if (_recentFilesIndex < 0)
				return;
			
			MenuItem mi = (MenuItem) e.OriginalSource;
			var path = mi.DataContext as string;
			if (path == null)
				return;
			
			if (MessageBox.Show("Load " + path, "Are you sure you want to load?", MessageBoxButtons.YesNo) ==
				System.Windows.Forms.DialogResult.No)
				return;
			Load(path);
		}

		private void mnuLoad_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog
			{
				Title = "Open file: ",
				InitialDirectory = GetFilePath(),
				Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
			};

			DialogResult dr = ofd.ShowDialog();

			if (dr != System.Windows.Forms.DialogResult.OK)
				return;
			Load(ofd.FileName);
		}
		
		private void mnuTDelete_Click(object sender, EventArgs e)
		{
			_tIncompleteItems.RemoveAt(lbTIncompleteItems.SelectedIndex);
			_isChanged = true;
			RefreshTodo();
		}

		private void mnuTEdit_Click(object sender, EventArgs e)
		{
			EditItem(sender, _tIncompleteItems);
		}
		
		private void mnuHEdit_Click(object sender, EventArgs e)
		{
			EditItem(sender, _hCurrentHistoryItem.CompletedTodos);
		}
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// HISTORY TAB //
		private void tbHTitle_TextChanged(object sender, EventArgs e)
		{
			_hCurrentHistoryItem.Title = tbHTitle.Text;
			lbHHistory.Items.Refresh();
		}
		
		private void tbHNotes_TextChanged(object sender, EventArgs e)
		{
			_hCurrentHistoryItem.Notes = tbHNotes.Text;
			lbHHistory.Items.Refresh();
		}

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
				index = 0;

			_hCurrentHistoryItem = lbHHistory.Items[index] as HistoryItem;
			RefreshHistory();
		}

		private void btnHDeleteTodo_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			TodoItem td = b?.DataContext as TodoItem;
			if (MessageBox.Show("Delete", "Are you sure", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
				return;

			if (td != null)
			{
				td.IsComplete = false;
				_tIncompleteItems.Add(td);
				td.Rank = _tIncompleteItems.Count;
				RefreshTodo();
				_hCurrentHistoryItem.CompletedTodos.Remove(td);
				_isChanged = true;
			}
			RefreshHistory();
		}

		private void btnHDeleteHistory_Click(object sender, EventArgs e)
		{
			if (_hHistoryItems.Count == 0)
				return;
			if (MessageBox.Show("Delete", "Are you sure", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
				return;

			_hHistoryItems.Remove(_hCurrentHistoryItem);

			_hCurrentHistoryItem = _hHistoryItems.Count > 0 ? _hHistoryItems[0] : new HistoryItem("", "");
			_isChanged = true;
			RefreshHistory();
		}

		private void btnHNewHistory_Click(object sender, EventArgs e)
		{
			AddNewHistoryItem();
		}
		
		private void btnHCopyHistory_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			if (b?.DataContext is HistoryItem item)
				Clipboard.SetText(item.ToClipboard());
			if (lbHHistory.SelectedIndex == 0)
				AddNewHistoryItem();
		}
		
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
		
		private void AddNewHistoryItem()
		{
			_hCurrentHistoryItem = new HistoryItem(DateTime.Now);
			_hHistoryItems.Add(_hCurrentHistoryItem);
			_isChanged = true;
			RefreshHistory();
		}
		
		private void RefreshHistory()
		{
			List<HistoryItem> temp = _hHistoryItems.OrderByDescending(o => o.DateTimeAdded).ToList();
			_hHistoryItems.Clear();
			foreach (HistoryItem hi in temp)
				_hHistoryItems.Add(hi);

			if (_hHistoryItems.Count == 0)
				_hCurrentHistoryItem = new HistoryItem("", "");

			if (_hHistoryItems.Count > 0 && _hCurrentHistoryItem.DateAdded == "")
				lbHHistory.SelectedIndex = 0;

			tbHNotes.Text = _hCurrentHistoryItem.Notes;
			tbHTitle.Text = _hCurrentHistoryItem.Title;
			lbHCompletedTodos.ItemsSource = _hCurrentHistoryItem.CompletedTodos;
			lbHCompletedTodos.Items.Refresh();

			lbHHistory.Items.Refresh();
		}
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// TO DO TAB //
		private void cbTSeverity_SelectionChanged(object sender, EventArgs e)
		{
			if (sender is ComboBox rb)
				_tCurrentSeverity = rb.SelectedIndex + 1;
		}
		
		private void btnTAdd_Click(object sender, EventArgs e)
		{
			TodoItem td = new TodoItem() {Todo = txtT1NewTodo.Text, Severity = _tCurrentSeverity};

			_tIncompleteItems.Add(td);
			td.Rank = _tIncompleteItems.Count;
			_isChanged = true;
			RefreshTodo();
			txtT1NewTodo.Clear();
		}

		private void btnTComplete_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			object item = b?.DataContext;

			if (item != null)
			{
				int index = lbTIncompleteItems.Items.IndexOf(item);
				TodoItem td = _tIncompleteItems[index];

				TodoItemComplete tdc = new TodoItemComplete(td);
				tdc.ShowDialog();
				if (tdc.isOk)
				{
					_tIncompleteItems.RemoveAt(index);
					_tIncompleteItems.Add(tdc.Result);
					_isChanged = true;
				}
			}
			RefreshTodo();
		}

		private void btnRank_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			if (b != null)
			{
				TodoItem td = b.DataContext as TodoItem;
				var index = _tIncompleteItems.IndexOf(td);

				if ((string) b.CommandParameter == "up")
				{
					if (index == 0)
						return;
					int newRank = _tIncompleteItems[index - 1].Rank;
					if (td != null)
					{
						_tIncompleteItems[index - 1].Rank = td.Rank;
						td.Rank = newRank;
						_isChanged = true;
					}
				}
				else if ((string) b.CommandParameter == "down")
				{
					if (index >= _tIncompleteItems.Count)
						return;
					int newRank = _tIncompleteItems[index + 1].Rank;
					if (td != null)
					{
						_tIncompleteItems[index + 1].Rank = td.Rank;
						td.Rank = newRank;
						_isChanged = true;
					}
				}
			}
			RefreshTodo();
		}

		private void btnTimeTaken_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			if (b != null)
			{
				TodoItem td = b.DataContext as TodoItem;

				if ((string) b.CommandParameter == "start")
				{
					if (td != null)
						td.IsTimerOn = true;
				}
				else if ((string) b.CommandParameter == "stop")
				{
					if (td != null)
						td.IsTimerOn = false;
				}
				
				_isChanged = true;
			}

			lbTIncompleteItems.Items.Refresh();
		}

		private void EditItem(object sender, List<TodoItem> list)
		{
			ListBox lb = sender as ListBox;
			if (lb != null)
			{
				int index = lb.SelectedIndex;
				if (index < 0)
					return;
				TodoItem td = list[index];
				TodoItemEditor tdie = new TodoItemEditor(td);

				tdie.ShowDialog();
				if (tdie.isOk)
				{
					list.Remove(td);
					_tIncompleteItems.Add(tdie.Result);
					_isChanged = true;
				}
			}

			RefreshTodo();
			RefreshHistory();
		}
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Sorting //
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
			RefreshTodo();
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
			RefreshTodo();
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
					List<string> sortedTags = new List<string>();
					List<string> unsortedTags = td.Tags.ToList();
					foreach (string u in td.Tags)
					{
						if (u == s)
						{
							sortedTags.Add(u);
							unsortedTags.Remove(u);
						}
					}
					sortedTags.AddRange(unsortedTags);
					td.Tags = sortedTags;
					
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
		
		private void RefreshTodo()
		{
			// TODO: Get rid of this completeItems
			List<TodoItem> incompleteItems = new List<TodoItem>();
			List<string> hashTagList = new List<string>();

			foreach (TodoItem td in _tIncompleteItems)
			{
				if (td.IsComplete)
				{
					td.Rank = int.MaxValue;
					AddTodoToHistory(td);
				}
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
					_tIncompleteItems = _tReverseSort
						? _tIncompleteItems.OrderByDescending(o => o.Severity).ToList()
						: _tIncompleteItems.OrderBy(o => o.Severity).ToList();
					break;
				case "date":
					_tIncompleteItems = _tReverseSort
						? _tIncompleteItems.OrderByDescending(o => o.TimeStarted).ToList()
						: _tIncompleteItems.OrderBy(o => o.TimeStarted).ToList();
					_tIncompleteItems = _tReverseSort
						? _tIncompleteItems.OrderByDescending(o => o.DateStarted).ToList()
						: _tIncompleteItems.OrderBy(o => o.DateStarted).ToList();
					break;
				case "hash":
					_tIncompleteItems = SortByHashTag();
					break;
				case "rank":
					_tIncompleteItems = _tReverseSort
						? _tIncompleteItems.OrderByDescending(o => o.Rank).ToList()
						: _tIncompleteItems.OrderBy(o => o.Rank).ToList();
					break;
				case "active":
					_tIncompleteItems = _tReverseSort
						? _tIncompleteItems.OrderByDescending(o => o.IsTimerOn).ToList()
						: _tIncompleteItems.OrderBy(o => o.IsTimerOn).ToList();
					break;
			}

			lbTIncompleteItems.ItemsSource = _tIncompleteItems;
			lbTIncompleteItems.Items.Refresh();
			cbHashtags.ItemsSource = _tHashTags;
			cbHashtags.Items.Refresh();
		}
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// FileIO //
		private string GetFilePath()
		{
			string result = "";
			if (_recentFiles.Count == 0)
				return basePath;

			string[] sa = _recentFiles[0].Split('\\');
			for (int i = 0; i < sa.Length - 1; i++)
			{
				result += sa[i] + "\\";
			}
			return result;
		}

		private string GetFileName()
		{
			string result = "";
			if (_recentFiles.Count == 0)
				return "";

			string[] sa = _recentFiles[0].Split('\\');
			return sa[sa.Count() - 1];
		}
		
		private void SaveAs()
		{
			string path = GetFilePath();
			SaveFileDialog sfd = new SaveFileDialog
			{
				Title = "Select folder to save game in.",
				FileName = GetFileName(),
				InitialDirectory = GetFilePath(),
				Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
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

			StreamWriter stream = new StreamWriter(File.Open(path, FileMode.Create));

			foreach (TodoItem td in _tIncompleteItems)
			{
				stream.WriteLine(td.ToString());
			}

			stream.WriteLine("====================================VCS");
			foreach (HistoryItem hi in _hHistoryItems)
			{
				stream.Write(hi.ToString());
			}
			stream.Close();
			_currentOpenFile = path;
			Title = WindowTitle;
			_isChanged = false;
			_currentOpenFile = path;
		}

		private void Load(string path)
		{
			SortRecentFiles(path);
			SaveSettings();

			StreamReader stream = new StreamReader(File.Open(path, FileMode.Open));

			_tIncompleteItems.Clear();
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

			RefreshTodo();
			RefreshHistory();
			_currentOpenFile = path;
			Title = WindowTitle;

			_isChanged = false;
			_currentOpenFile = path;
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Settings //
		private void LoadSettings()
		{
			_recentFiles = new ObservableCollection<string>();
			StreamReader stream = new StreamReader(File.Open(basePath + "TDHistory.settings", FileMode.Open));
			string line = stream.ReadLine();
			_recentFiles.Clear();
			while (line != null)
			{
				_recentFiles.Add(line);
				line = stream.ReadLine();
				if (line == "WINDOWPOSITION")
					break;
			}

			top = Convert.ToDouble(stream.ReadLine());
			left = Convert.ToDouble(stream.ReadLine());
			height = Convert.ToDouble(stream.ReadLine());
			width = Convert.ToDouble(stream.ReadLine());

			stream.Close();
		}

		private void SaveSettings()
		{
			StreamWriter stream = new StreamWriter(File.Open(basePath + "TDHistory.settings", FileMode.Create));
			foreach (string s in _recentFiles)
				stream.WriteLine(s);
			stream.WriteLine("WINDOWPOSITION");
			stream.WriteLine(Top);
			stream.WriteLine(Left);
			stream.WriteLine(Height);
			stream.WriteLine(Width);
			stream.Close();
		}

		private void SortRecentFiles(string recent)
		{
			if (_recentFiles.Contains(recent))
				_recentFiles.Remove(recent);

			_recentFiles.Insert(0, recent);

			while (_recentFiles.Count >= 10)
			{
				_recentFiles.RemoveAt(_recentFiles.Count - 1);
			}
		}
	}
}
