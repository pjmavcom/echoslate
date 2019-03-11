using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using TextBox = System.Windows.Controls.TextBox;


namespace TODOList
{
	public partial class MainWindow
	{
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
		public const string DATE = "yyyMMdd";
		public const string TIME = "HHmmss";
		public const string VERSION = "2.0";

		private List<TabItem> tabItemList;

		// TO DO TAB ITEMS
		private List<TodoItem> _currentList;
		private List<string> _currentHashTags;

		private List<List<TodoItem>> _incompleteItems;
		private List<List<string>> _hashTags;
		private List<string> _tabHash;
		List<string> _prevHashTagList = new List<string>();
		
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
		private bool _autoSave;

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
		
		// POMOTIMER
		private DateTime _pomoTimer;
		private bool _isPomoTimerOn;
		private bool _isPomoWorkTimerOn;
		private int _pomoWorkTime = 25;
		private int _pomoBreakTime = 5;
		private string _pomoTimerString;
		
		// CONTROLS
		private ComboBox cbHashTags;
		private ListBox lbIncompleteItems;
//		private TextBox tbNewTodo;


		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		// TODO: Fix this for more Tabs
		public List<TodoItem> IncompleteItems => _incompleteItems[tabTest.SelectedIndex];
		public List<string> HashTags => _hashTags[tabTest.SelectedIndex];
		public string TabHash => _tabHash[tabTest.SelectedIndex];
		
		public List<HistoryItem> HistoryItems => _hHistoryItems;
		
		public string WindowTitle => "TodoList v" + VERSION + " " + _currentOpenFile;
		public ObservableCollection<string> RecentFiles
		{
			get => _recentFiles;
			set => _recentFiles = value;
		}
		

		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		// TODO: Fix this for more Tabs
		public  MainWindow()
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

			tabItemList = new List<TabItem>();

			_incompleteItems = new List<List<TodoItem>>();
			_hashTags = new List<List<string>>();
			_tabHash = new List<string>();
			
			_hHistoryItems = new List<HistoryItem>();
			_hCurrentHistoryItem = new HistoryItem("", "");

			CreateNewTabs();
			
			tabTest.ItemsSource = tabItemList;
			tabTest.Items.Refresh();

			if (_recentFiles.Count > 0)
				Load(_recentFiles[0]);

			lbHistory.SelectedIndex = 0;
			lbCompletedTodos.SelectedIndex = 0;
			
			lblPomoWork.Content = _pomoWorkTime.ToString();
			lblPomoBreak.Content = _pomoBreakTime.ToString();
		}

		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS //
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// MainWindow Methods //
		private void CreateNewTabs()
		{
			AddNewTodoTab("Other");
			AddNewTodoTab("Bug");
			AddNewTodoTab("Feature");
		}
		private void AddNewTodoTab(string name)
		{
			TabItem ti = new TabItem();
			string hash = "#" + name.ToUpper();
			name = UpperFirstLetter(name);
			ti.Header = name;
			ti.Name = name;
			_incompleteItems.Add(new List<TodoItem>());
			_hashTags.Add(new List<string>());
			_tabHash.Add(hash);
			tabItemList.Add(ti);
			RefreshTodo();
			tabTest.Items.Refresh();
		}
		private void RemoveTab(int index)
		{
			if (index < 1 || index >= tabItemList.Count)
				return;

			_incompleteItems[0].AddRange(_incompleteItems[index]);
			_incompleteItems.RemoveAt(index);
			_hashTags.RemoveAt(index);
			_tabHash.RemoveAt(index);
			tabItemList.RemoveAt(index);
			tabTest.SelectedIndex = 0;
			tabTest.Items.Refresh();
			RefreshTodo();
		}
		private string UpperFirstLetter(string s)
		{
			string result = "";
			for (int i = 0; i < s.Length; i++)
			{
				if(i == 0)
					result += s[i].ToString().ToUpper();
				else
					result += s[i];
			}
			return result;
		}
		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			IntPtr handle = new WindowInteropHelper(this).Handle;
			source = HwndSource.FromHwnd(handle);
			if (source != null) 
				source.AddHook(HwndHook);
			RegisterHotKey(handle, HOTKEY_ID, MOD_WIN, 0x73);

#if DEBUG
			ckAutoSave.IsChecked = false;
#else
			ckAutoSave.IsChecked = true;
#endif
			
			_autoSave = (bool) ckAutoSave.IsChecked;
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
								tbNewTodo.Focus();
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
			for(int i = 0; i < _incompleteItems.Count;i++)
				foreach (TodoItem td in _incompleteItems[i])
					if (td.IsTimerOn)
						td.TimeTaken = td.TimeTaken.AddSeconds(1);
			
			if (_isPomoTimerOn)
			{
				_pomoTimer = _pomoTimer.AddSeconds(1);
				lblPomo.Content = String.Format("{0:00}:{1:00}", _pomoTimer.Ticks / TimeSpan.TicksPerMinute, _pomoTimer.Second);
				
				if(_isPomoWorkTimerOn)
				{
					lblPomo.Background = Brushes.Lime;
					if (_pomoTimer.Ticks / TimeSpan.TicksPerMinute >= _pomoWorkTime)
					{
						_isPomoWorkTimerOn = false;
						_pomoTimer=DateTime.MinValue;
					}
				}
				else
				{
					lblPomo.Background = Brushes.Maroon;
					if (_pomoTimer.Ticks / TimeSpan.TicksPerMinute >= _pomoBreakTime)
					{
						_isPomoWorkTimerOn = true;
						_pomoTimer=DateTime.MinValue;
					}
				}
			}
			else
				lblPomo.Background=Brushes.Transparent;
		}
		
		private void Window_Closed(object sender, CancelEventArgs e)
		{
			SaveSettings();
			if (!_isChanged)
				return;

			if (MessageBox.Show("Maybe save first?", "Close", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
				Save(_currentOpenFile);
		}
		
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		
		private void tabSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Dispatcher.BeginInvoke(new Action(() => updateHandler()));
		}
		void updateHandler()
		{
			TabItem test = tabTest.SelectedItem as TabItem;
			ContentPresenter myContentPresenter = tabTest.Template.FindName("PART_SelectedContentHost", tabTest) as ContentPresenter;
			{
				myContentPresenter.ApplyTemplate();
				lbIncompleteItems = myContentPresenter.ContentTemplate.FindName("lbIncompleteItems", myContentPresenter) as ListBox;
				lbIncompleteItems.ItemsSource = IncompleteItems;
				lbIncompleteItems.Items.Refresh();
				cbHashTags = myContentPresenter.ContentTemplate.FindName("cbHashTags", myContentPresenter) as ComboBox;
				cbHashTags.ItemsSource = HashTags;
				cbHashTags.Items.Refresh();
			}
			
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Hotkeys //
		private void hkSwitchTab(object sender, ExecutedRoutedEventArgs e)
		{
			int index = tabTest.SelectedIndex;
			if ((string) e.Parameter == "right")
			{
				if (TabControl.SelectedIndex == 0)
				{
					TabControl.SelectedIndex = 1;
					return;
				}
				index++;
				if (index >= tabTest.Items.Count)
					index = tabTest.Items.Count - 1;
			}
			else if ((string) e.Parameter == "left")
			{
				index--;
				if (index < 0)
				{
					index = 0;
					TabControl.SelectedIndex = 0;
				}
			}
			tabTest.SelectedIndex = index;
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

			if (tbNewTodo.IsFocused)
			{
				QuickComplete();
				return;
			}

			TodoItem td = (TodoItem) lbIncompleteItems.SelectedItem;
			if (td != null)
			{
				TodoItemComplete tdc = new TodoItemComplete(td);
				tdc.ShowDialog();
				if (tdc.isOk)
				{
					IncompleteItems.RemoveAt(lbIncompleteItems.SelectedIndex);
					IncompleteItems.Add(tdc.Result);
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
			if (tbNewTodo.IsFocused)
			{
				btnTAdd_Click(sender, e);
				return;
			}
			
			ListBox lb = null;
			List<TodoItem> list = new List<TodoItem>();
			
			if(tabTodos.IsSelected)
			{
				lb = lbIncompleteItems;
				list = IncompleteItems;
			}
			else if (tabHistory.IsSelected)
			{
				lb = lbCompletedTodos;
				list = _hCurrentHistoryItem.CompletedTodos;
			}
		
			EditItem(lb, list);
		}

		private void hkQuickSave(object sender, ExecutedRoutedEventArgs e)
		{
			Save(_recentFiles[0]);
		}

		private void hkQuickLoadPrevious(object sender, ExecutedRoutedEventArgs e)
		{
			if(_recentFiles.Count >= 2)
				Load(_recentFiles[1]);
		}

		private void hkStartStopTimer(object sender, ExecutedRoutedEventArgs e)
		{
			if (!tabTodos.IsSelected)
				return;
			int index = lbIncompleteItems.SelectedIndex;
			IncompleteItems[index].IsTimerOn = !IncompleteItems[index].IsTimerOn;
			lbIncompleteItems.Items.Refresh();
		}

		private void QuickComplete()
		{
			TodoItem newtd = new TodoItem
			{
				Todo = tbNewTodo.Text,
				Severity = _tCurrentSeverity,
				Rank = IncompleteItems.Count,
				IsComplete = true
			};

			IncompleteItems.Add(newtd);
			AutoSave();
			RefreshTodo();
			tbNewTodo.Clear();
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// MenuCommands //
		private void AddNewTab_OnClick(object sender, EventArgs e)
		{
			AddTab at = new AddTab();
			at.ShowDialog();
			if (at.Result)
				AddNewTodoTab(at.Name);
		}
		private void RemoveTab_OnClick(object sender, EventArgs e)
		{
			List<TabItem> list = tabItemList.ToList();
			RemoveTab rt = new RemoveTab(list);
			rt.ShowDialog();
			if (!rt.Result)
				return;
			RemoveTab(rt.Index);
		}
			
		private void mnuNew_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Are you sure?", "New File", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
				return;
			_hHistoryItems.Clear();
			_incompleteItems.Clear();
			CreateNewTabs();

			_hCurrentHistoryItem = new HistoryItem("", "");
			RefreshHistory();
			RefreshTodo();

			_currentOpenFile = "";
			Title = WindowTitle;
			AutoSave();
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
			if (mi == null)
				return;
			string path = (string) mi.DataContext;
			_recentFilesIndex = mnuRecentSaves.Items.IndexOf(path);
		}
		
		private void mnuRecentLoadsRMB(object sender, MouseButtonEventArgs e)
		{
			_recentFilesIndex = -1;
			var mi = e.OriginalSource as TextBlock;
			if (mi == null)
				return;
			string path = (string) mi.DataContext;
			_recentFilesIndex = mnuRecentLoads.Items.IndexOf(path);
		}

		private void mnuResetTimer_Click(object sender, EventArgs e)
		{
			int index = lbIncompleteItems.SelectedIndex;
			if (index < 0)
				return;
			TodoItem td = IncompleteItems[index];
			td.TimeTaken = new DateTime();
			td.IsTimerOn = false;
			lbIncompleteItems.Items.Refresh();
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
			
			if(_isChanged)
				if (MessageBox.Show("Maybe save first?", "Close", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
					Save(_currentOpenFile);
			
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
			
			if(_isChanged)
				if (MessageBox.Show("Maybe save first?", "Close", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
					Save(_currentOpenFile);

			Load(ofd.FileName);
		}
		
		private void mnuTDelete_Click(object sender, EventArgs e)
		{
			IncompleteItems.RemoveAt(lbIncompleteItems.SelectedIndex);
			
			AutoSave();
			RefreshTodo();
		}

		private void mnuTEdit_Click(object sender, EventArgs e)
		{
			if(lbIncompleteItems.SelectedItems.Count > 1)
				MultiEditItems(lbIncompleteItems, IncompleteItems);
			else
				EditItem(lbIncompleteItems, IncompleteItems);
		}
		
		private void mnuHEdit_Click(object sender, EventArgs e)
		{
			if(lbCompletedTodos.IsMouseOver)
				EditItem(lbCompletedTodos, _hCurrentHistoryItem.CompletedTodos);
			else if(lbCompletedTodosFeatures.IsMouseOver)
				EditItem(lbCompletedTodosFeatures, _hCurrentHistoryItem.CompletedTodosFeatures);
			else if(lbCompletedTodosBugs.IsMouseOver)
				EditItem(lbCompletedTodosBugs, _hCurrentHistoryItem.CompletedTodosBugs);
			
			RefreshHistory();
		}

		private void AutoSave_OnClick(object sender, EventArgs e)
		{
			_autoSave = (bool) ckAutoSave.IsChecked;
		}
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// HISTORY TAB //
		private void tbHTitle_TextChanged(object sender, EventArgs e)
		{
			_hCurrentHistoryItem.Title = tbHTitle.Text;
			lbHistory.Items.Refresh();
		}
		
		private void tbHNotes_TextChanged(object sender, EventArgs e)
		{
			_hCurrentHistoryItem.Notes = tbHNotes.Text;
			lbHistory.Items.Refresh();
		}

		private void lbHHistory_SelectionChanged(object sender, EventArgs e)
		{
			int index = lbHistory.SelectedIndex;
			if (_hHistoryItems.Count > 0 && index >= _hHistoryItems.Count)
				index = _hHistoryItems.Count - 1;
			if (_hHistoryItems.Count == 0)
			{
				_hCurrentHistoryItem = new HistoryItem("", "");
				return;
			}
			if (index < 0)
				index = 0;

			_hCurrentHistoryItem = lbHistory.Items[index] as HistoryItem;
			lblHTotalTime.Content = _hCurrentHistoryItem.TotalTime;
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
				IncompleteItems.Add(td);
				td.Rank = IncompleteItems.Count;
				RefreshTodo();
				if(_hCurrentHistoryItem.CompletedTodos.Contains(td))
					_hCurrentHistoryItem.CompletedTodos.Remove(td);
				else if(_hCurrentHistoryItem.CompletedTodosBugs.Contains(td))
					_hCurrentHistoryItem.CompletedTodosBugs.Remove(td);
				else if(_hCurrentHistoryItem.CompletedTodosFeatures.Contains(td))
					_hCurrentHistoryItem.CompletedTodosFeatures.Remove(td);
				AutoSave();
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
			AutoSave();
			RefreshHistory();
		}

		private void btnHNewHistory_Click(object sender, EventArgs e)
		{
			AddNewHistoryItem();
		}
		
		private void btnHCopyHistory_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			HistoryItem hi = (HistoryItem) b?.DataContext;
			if (hi != null)
			{
				int totalTime = 0;
				foreach (HistoryItem hist in _hHistoryItems)
				{
					totalTime += Convert.ToInt32(hist.TotalTime);
				}
				string time = String.Format("{0:00} : {1:00}", totalTime / 60, totalTime % 60);
				Clipboard.SetText(hi.ToClipboard(time));
				if (lbHistory.Items.IndexOf(hi) == 0)
					AddNewHistoryItem();
			}
		}
		
		private void AddTodoToHistory(TodoItem td)
		{
			if (_hCurrentHistoryItem.DateAdded == "")
				AddNewHistoryItem();
			RefreshHistory();
			_hCurrentHistoryItem = _hHistoryItems[0];
			_hCurrentHistoryItem.AddCompletedTodo(td);
			RefreshHistory();
			AutoSave();
		}
		
		private void AddNewHistoryItem()
		{
			_hCurrentHistoryItem = new HistoryItem(DateTime.Now);
			_hHistoryItems.Add(_hCurrentHistoryItem);
			AutoSave();
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
				lbHistory.SelectedIndex = 0;

			SortCompletedItems(_hCurrentHistoryItem); 
			
			tbHNotes.Text = _hCurrentHistoryItem.Notes;
			tbHTitle.Text = _hCurrentHistoryItem.Title;
			lbCompletedTodos.ItemsSource = _hCurrentHistoryItem.CompletedTodos;
			lbCompletedTodos.Items.Refresh();
			lbCompletedTodosBugs.ItemsSource = _hCurrentHistoryItem.CompletedTodosBugs;
			lbCompletedTodosBugs.Items.Refresh();
			lbCompletedTodosFeatures.ItemsSource = _hCurrentHistoryItem.CompletedTodosFeatures;
			lbCompletedTodosFeatures.Items.Refresh();
			lblHTotalTime.Content = _hCurrentHistoryItem.TotalTime;

			lbHistory.Items.Refresh();
		}
		
		private void SortCompletedItems(HistoryItem hi)
		{
			List<TodoItem> result = new List<TodoItem>();
			List<TodoItem> tempBug = new List<TodoItem>();
			List<TodoItem> tempFeature = new List<TodoItem>();
			List<TodoItem> tempOther = new List<TodoItem>();
			List<TodoItem> temp = new List<TodoItem>();

			temp.AddRange(hi.CompletedTodos);
			temp.AddRange(hi.CompletedTodosBugs);
			temp.AddRange(hi.CompletedTodosFeatures);
			
			foreach (TodoItem td in temp)
			{
				if (td.Tags.Contains("#BUG"))
					tempBug.Add(td);
				else if (td.Tags.Contains("#FEATURE"))
					tempFeature.Add(td);
				else
					tempOther.Add(td);
			}
			hi.CompletedTodos = tempOther;
			hi.CompletedTodosBugs = tempBug;
			hi.CompletedTodosFeatures = tempFeature;
		}
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// TO DO TAB //
		private void cbSeverity_SelectionChanged(object sender, EventArgs e)
		{
			if (sender is ComboBox cb)
			{
				int index = cb.SelectedIndex;
				_tCurrentSeverity = index;
			}
		}
		private void cb_IsLoaded(object sender, EventArgs e)
		{
			if (sender is ComboBox cb)
			{
				cb.SelectedIndex = _tCurrentSeverity;
			}
		}
		private void btnTAdd_Click(object sender, EventArgs e)
		{
			TodoItem td = new TodoItem() {Todo = tbNewTodo.Text, Severity = _tCurrentSeverity};

			_incompleteItems[0].Add(td);
			td.Rank = int.MaxValue;
			if (td.Severity == 3)
				td.Rank = 0;
			AutoSave();
			RefreshTodo();
			tbNewTodo.Clear();
		}

		private void btnTComplete_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			object item = b?.DataContext;
			if (item != null)
			{
				int index = lbIncompleteItems.Items.IndexOf(item);
				TodoItem td = IncompleteItems[index];

				TodoItemComplete tdc = new TodoItemComplete(td);
				tdc.ShowDialog();
				if (tdc.isOk)
				{
					IncompleteItems.RemoveAt(index);
					IncompleteItems.Add(tdc.Result);
					AutoSave();
				}
			}
			RefreshTodo();
		}

		private void btnRank_Click(object sender, EventArgs e)
		{
			if (sender is Button b)
			{
				TodoItem td = b.DataContext as TodoItem;

				if (IncompleteItems.Count == 0)
					return;
				
				var index = IncompleteItems.IndexOf(td);
				if ((string) b.CommandParameter == "up")
				{
					if (index == 0)
						return;
					int newRank = IncompleteItems[index - 1].Rank;
					if (td != null)
					{
						IncompleteItems[index - 1].Rank = td.Rank;
						td.Rank = newRank;
						AutoSave();
					}
				}
				else if ((string) b.CommandParameter == "down")
				{
					if (index >= IncompleteItems.Count)
						return;
					int newRank = IncompleteItems[index + 1].Rank;
					if (td != null)
					{
						IncompleteItems[index + 1].Rank = td.Rank;
						td.Rank = newRank;
						AutoSave();
					}
				}
			}
			RefreshTodo();
		}
		
		private void btnTimeTaken_Click(object sender, EventArgs e)
		{
			if (sender is Button b)
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
				
				AutoSave();
			}

			lbIncompleteItems.Items.Refresh();
		}

		private void btnPomoTimerStart_OnClick(object sender, EventArgs e)
		{
			_isPomoTimerOn = true;
			_isPomoWorkTimerOn = true;
			_pomoTimer = DateTime.MinValue;
		}
		private void btnPomoTimerStop_OnClick(object sender, EventArgs e)
		{
			_isPomoTimerOn = false;
		}
		private void btnPomoWorkInc_OnClick(object sender, EventArgs e)
		{
			_pomoWorkTime += 5;
			lblPomoWork.Content = _pomoWorkTime.ToString();
		}
		private void btnPomoWorkDec_OnClick(object sender, EventArgs e)
		{
			_pomoWorkTime -= 5;
			if (_pomoWorkTime <= 0)
				_pomoWorkTime = 5;
			lblPomoWork.Content = _pomoWorkTime.ToString();
		}
		private void btnPomoBreakInc_OnClick(object sender, EventArgs e)
		{
			_pomoBreakTime += 5;
			lblPomoBreak.Content = _pomoBreakTime.ToString();
		}
		private void btnPomoBreakDec_OnClick(object sender, EventArgs e)
		{
			_pomoBreakTime -= 5;
			if (_pomoBreakTime <= 0)
				_pomoBreakTime = 5;
			lblPomoBreak.Content = _pomoBreakTime.ToString();
		}

		private void EditItem(ListBox lb, List<TodoItem> list)
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
				_incompleteItems[0].Add(tdie.Result);
				AutoSave();
			}

			RefreshTodo();
			RefreshHistory();
		}
		
		private void MultiEditItems(ListBox lb, List<TodoItem> list)
		{
			TodoItem firstTd = lb.SelectedItems[0] as TodoItem;
			TodoMultiItemEditor tmie = new TodoMultiItemEditor(firstTd);
			tmie.ShowDialog();
			if (tmie.isOk)
			{
				foreach (TodoItem td in lb.SelectedItems)
				{
					if(tmie.ChangeRank)
						td.Rank = tmie.Result.Rank;
					if(tmie.ChangeSev)
						td.Severity = tmie.Result.Severity;
					if (tmie.isComplete && tmie.ChangeComplete)
						td.IsComplete = true;
					if (tmie.ChangeTodo)
					{
						td.Todo += Environment.NewLine + tmie.Result.Todo;
						td.ParseTags();
					}
				}
				RefreshTodo();
			}
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Sorting //
		private void cbHashtags_SelectionChanged(object sender, EventArgs e)
		{
			if (HashTags.Count == 0)
				return;
			_hashToSortBy = HashTags[0];
			if (cbHashTags.SelectedItem != null)
				_hashToSortBy = cbHashTags.SelectedItem.ToString();
			_hashSortSelected = true;
			_tCurrentSort = "hash";
			RefreshTodo();
		}
		
		private void btnSort_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			if (_tCurrentSort != (string) b?.CommandParameter)
			{
				_tReverseSort = false;
				_tCurrentSort = (string) b?.CommandParameter;
			}
			
			if ((string) b?.CommandParameter == "hash")
			{
				if (HashTags.Count == 0)
					return;
				_tCurrentHashTagSortIndex++;
				if (_tCurrentHashTagSortIndex >= HashTags.Count)
					_tCurrentHashTagSortIndex = 0;
			}

			_tReverseSort = !_tReverseSort;
			RefreshTodo();
		}
		
		private List<TodoItem> SortByHashTag(List<TodoItem> list)
		{
			if (_tDidHashChange)
				_tCurrentHashTagSortIndex = 0;

			List<TodoItem> incompleteItems = new List<TodoItem>();
			List<string> sortedHashTags = new List<string>();
			
			if (HashTags.Count == 0)
				return list;
			
			if (_hashSortSelected)
			{
				_tCurrentHashTagSortIndex = 0;
				foreach (string s in HashTags)
				{
					if (s.Equals(_hashToSortBy))
						break;
					_tCurrentHashTagSortIndex++;
				}
			}

			for (int i = 0 + _tCurrentHashTagSortIndex; i < HashTags.Count; i++)
				sortedHashTags.Add(HashTags[i]);
			for (int i = 0; i < _tCurrentHashTagSortIndex; i++)
				sortedHashTags.Add(HashTags[i]);

			foreach (string s in sortedHashTags)
			{
				List<TodoItem> temp = list.ToList();
				foreach (TodoItem td in temp)
				{
					List<string> sortedTags = new List<string>();
					List<string> unsortedTags = td.Tags.ToList();
					foreach (string u in td.Tags)
					{
						if (u != s)
							continue;
						sortedTags.Add(u);
						unsortedTags.Remove(u);
					}
					sortedTags.AddRange(unsortedTags);
					td.Tags = sortedTags;
					
					foreach (string t in td.Tags)
					{
						if (!s.Equals(t))
							continue;
						incompleteItems.Add(td);
						list.Remove(td);
					}
				}
			}
			foreach (TodoItem td in list)
				incompleteItems.Add(td);
			
			return incompleteItems;
		}
		
		private void FixRankings()
		{
			for (int i = 0; i < _incompleteItems.Count; i++)
			{
				_incompleteItems[i] = _incompleteItems[i].OrderBy(o => o.Rank).ToList();
				for (int rank = 0; rank < _incompleteItems[i].Count; rank++)
					_incompleteItems[i][rank].Rank = rank + 1;
			}
		}
		
		private void RefreshTodo()
		{
			List<TodoItem> incompleteItems = new List<TodoItem>();
			
			for (int i = 0; i < _incompleteItems.Count; i++)
			{
				foreach (TodoItem td in _incompleteItems[i])
					if (!incompleteItems.Contains(td))
						incompleteItems.Add(td);
				_incompleteItems[i].Clear();
			}
			
			foreach (TodoItem td in incompleteItems)
			{
				if (td.IsComplete)
				{
					td.Rank = 0;
					AddTodoToHistory(td);
					continue;
				}
				bool sortedToTab = false;
				int index = 0;
				foreach (string hash in _tabHash)
				{
					if (td.Tags.Contains(hash))
					{
						index = _tabHash.IndexOf(hash);
						_incompleteItems[index].Add(td);
						foreach(string s in td.Tags)
							if (!_hashTags[index].Contains(s))
								_hashTags[index].Add(s);
						sortedToTab = true;
						break;
					}
				}

				if (sortedToTab)
					continue;
				_incompleteItems[0].Add(td);
				foreach(string s in td.Tags)
					if (!_hashTags[0].Contains(s))
					{
						_hashTags[0].Add(s);
					}
			}

			for (int i = 0; i < _hashTags.Count; i++)
			{
				_hashTags[i] = _hashTags[i].OrderBy(o => o).ToList();
			}
			
			_tDidHashChange = false;
			if (_hashTags[0].Count != _prevHashTagList.Count)
				_tDidHashChange = true;
			else
				for (int i = 0; i < _hashTags[0].Count; i++)
					if (_hashTags[0][i] != _prevHashTagList[i])
						_tDidHashChange = true;
			_prevHashTagList = _hashTags[0].ToList();
			
			FixRankings();

			int tabIndex = tabTest.SelectedIndex;
			if (tabIndex < 0 || tabIndex >= tabItemList.Count)
				return;
			switch (_tCurrentSort)
			{
				case "sev":
					_incompleteItems[tabIndex] = _tReverseSort
						? _incompleteItems[tabIndex].OrderByDescending(o => o.Severity).ToList()
						: _incompleteItems[tabIndex].OrderBy(o => o.Severity).ToList();
					break;
				case "date":
					_incompleteItems[tabIndex] = _tReverseSort
						? _incompleteItems[tabIndex].OrderByDescending(o => o.TimeStarted).ToList()
						: _incompleteItems[tabIndex].OrderBy(o => o.TimeStarted).ToList();
					_incompleteItems[tabIndex] = _tReverseSort
						? _incompleteItems[tabIndex].OrderByDescending(o => o.DateStarted).ToList()
						: _incompleteItems[tabIndex].OrderBy(o => o.DateStarted).ToList();
					break;
				case "hash":
					_incompleteItems[tabIndex] = SortByHashTag(_incompleteItems[tabIndex]);
					break;
				case "rank":
					_incompleteItems[tabIndex] = _tReverseSort
						? _incompleteItems[tabIndex].OrderByDescending(o => o.Rank).ToList()
						: _incompleteItems[tabIndex].OrderBy(o => o.Rank).ToList();
					break;
				case "active":
					_incompleteItems[tabIndex] = _tReverseSort
						? _incompleteItems[tabIndex].OrderByDescending(o => o.IsTimerOn).ToList()
						: _incompleteItems[tabIndex].OrderBy(o => o.IsTimerOn).ToList();
					break;
			}

			lbIncompleteItems.ItemsSource = IncompleteItems;
			lbIncompleteItems.Items.Refresh();
			cbHashTags.ItemsSource = HashTags;
			cbHashTags.Items.Refresh();
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
			if (_recentFiles.Count == 0)
				return "";

			string[] sa = _recentFiles[0].Split('\\');
			return sa[sa.Length - 1];
		}
		private void AutoSave()
		{
			_isChanged = true;
			if (_currentOpenFile == "")
			{
				SaveAs();
				return;
			}
			if (_autoSave)
				Save(_recentFiles[0]);
		}
		
		private void SaveAs()
		{
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

			// TODO: Fix this for more Tabs
			List<TodoItem> incompleteItems = new List<TodoItem>();
			for (int i = 0; i < _incompleteItems.Count; i++)
				incompleteItems.AddRange(_incompleteItems[i]);
			foreach (TabItem ti in tabItemList)
			{
				stream.WriteLine(ti.Name);
			}
			stream.WriteLine("====================================TODO");
			foreach (TodoItem td in incompleteItems)
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

			_incompleteItems.Clear();
			_hashTags.Clear();
			_tabHash.Clear();
			tabItemList.Clear();
			_hHistoryItems.Clear();

			string line = stream.ReadLine();

			while (line != null)
			{
				if (line == "====================================")
				{
					line = stream.ReadLine();
					continue;
				}
				if (line == "====================================TODO")
				{
					line = stream.ReadLine();
					break;
				}
				AddNewTodoTab(line);
				line = stream.ReadLine();
			}

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
				_incompleteItems[0].Add(td);
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
			if (HistoryItems.Count > 0)
			{
				lbHistory.SelectedIndex = 0;
				_hCurrentHistoryItem = HistoryItems[0];
			}
			else
				_hCurrentHistoryItem = new HistoryItem("", "");
			
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
