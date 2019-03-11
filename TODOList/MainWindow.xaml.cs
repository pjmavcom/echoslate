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
		public const string VERSION = "1.7a";

		// TO DO TAB ITEMS
		private List<TodoItem> _tIncompleteItems;
		private List<TodoItem> _tIncompleteBugItems;
		private List<TodoItem> _tIncompleteFeatureItems;
		private List<string> _tHashTags;
		private List<string> _bugHashTags;
		private List<string> _featureHashTags;
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


		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		// TODO: Fix this for more Tabs
		public List<string> HashTags => _tHashTags;
		public List<string> BugHashTags => _bugHashTags;
		public List<string> FeatureHashTags => _featureHashTags;
		public List<TodoItem> IncompleteItems => _tIncompleteItems;
		public List<TodoItem> IncompleteBugItems => _tIncompleteBugItems;
		public List<TodoItem> IncompleteFeatureItems => _tIncompleteFeatureItems;
		 
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

			_tIncompleteItems = new List<TodoItem>();
			_tIncompleteBugItems = new List<TodoItem>();
			_tIncompleteFeatureItems = new List<TodoItem>();
			
			_hHistoryItems = new List<HistoryItem>();
			_tHashTags = new List<string>();
			_bugHashTags = new List<string>();
			_featureHashTags = new List<string>();
			_hCurrentHistoryItem = new HistoryItem("", "");

			if (_recentFiles.Count > 0)
				Load(_recentFiles[0]);

			lbHistory.SelectedIndex = 0;
			lbCompletedTodos.SelectedIndex = 0;
			lbIncompleteItems.SelectedIndex = 0;
			lbIncompleteBugItems.SelectedIndex = 0;
			lbIncompleteFeatureItems.SelectedIndex = 0;
			
			lblPomoWork.Content = _pomoWorkTime.ToString();
			lblPomoBreak.Content = _pomoBreakTime.ToString();
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
								
								// TODO: Fix this for more Tabs
								if(tabTodo.IsSelected)
									Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate { tbNewTodo.Focus(); }));
								else if(tabBug.IsSelected)
									Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate { tbBugNewTodo.Focus(); }));
								else if(tabFeature.IsSelected)
									Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate { tbFeatureNewTodo.Focus(); }));
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
			foreach (TodoItem td in _tIncompleteBugItems)
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
			cbBugSeverity.SelectedIndex = index;
			cbFeatureSeverity.SelectedIndex = index;
			cbSeverity.Items.Refresh();
			cbBugSeverity.Items.Refresh();
			cbFeatureSeverity.Items.Refresh();
		}
		
		// TODO: Fix this for more Tabs
		private void hkComplete(object sender, ExecutedRoutedEventArgs e)
		{
			if (tabHistory.IsSelected)
				return;

			if (tbNewTodo.IsFocused || tbBugNewTodo.IsFocused || tbFeatureNewTodo.IsFocused)
			{
				QuickComplete();
				return;
			}
			
			ListBox lb = null;
			List<TodoItem> list = new List<TodoItem>();
			
			if(tabTodo.IsSelected)
			{
				lb = lbIncompleteItems;
				list = _tIncompleteItems;
			}
			else if(tabBug.IsSelected)
			{
				lb = lbIncompleteBugItems;
				list = _tIncompleteBugItems;
			}
			else if(tabFeature.IsSelected)
			{
				lb = lbIncompleteFeatureItems;
				list = _tIncompleteFeatureItems;
			}

			TodoItem td = (TodoItem) lb.SelectedItem;
			if (td != null)
			{
				TodoItemComplete tdc = new TodoItemComplete(td);
				tdc.ShowDialog();
				if (tdc.isOk)
				{
					list.RemoveAt(lb.SelectedIndex);
					list.Add(tdc.Result);
				}
			}
			RefreshTodo();
		}

		// TODO: Fix this for more Tabs
		private void hkAdd(object sender, ExecutedRoutedEventArgs e)
		{
			if (tabHistory.IsSelected)
				return;
			btnTAdd_Click(sender, e);
		}

		// TODO: Fix this for more Tabs
		private void hkEdit(object sender, EventArgs e)
		{
			if (tbNewTodo.IsFocused || tbBugNewTodo.IsFocused || tbFeatureNewTodo.IsFocused)
			{
				btnTAdd_Click(sender, e);
				return;
			}
			
			ListBox lb = null;
			List<TodoItem> list = new List<TodoItem>();
			
			if(tabTodo.IsSelected)
			{
				lb = lbIncompleteItems;
				list = _tIncompleteItems;
			}
			else if(tabBug.IsSelected)
			{
				lb = lbIncompleteBugItems;
				list = _tIncompleteBugItems;
			}
			else if (tabHistory.IsSelected)
			{
				lb = lbCompletedTodos;
				list = _hCurrentHistoryItem.CompletedTodos;
			}
			else if(tabFeature.IsSelected)
			{
				lb = lbIncompleteFeatureItems;
				list = _tIncompleteFeatureItems;
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

		// TODO: Fix this for more Tabs
		private void hkStartStopTimer(object sender, ExecutedRoutedEventArgs e)
		{
			if (!tabTodo.IsSelected)
				return;
			
			ListBox lb = null;
			List<TodoItem> list = new List<TodoItem>();
			
			if(tabTodo.IsSelected)
			{
				lb = lbIncompleteItems;
				list = _tIncompleteItems;
			}
			else if(tabBug.IsSelected)
			{
				lb = lbIncompleteBugItems;
				list = _tIncompleteBugItems;
			}
			else if(tabFeature.IsSelected)
			{
				lb = lbIncompleteFeatureItems;
				list = _tIncompleteFeatureItems;
			}
			
			int index = lb.SelectedIndex;
			list[index].IsTimerOn = !list[index].IsTimerOn;
			lb.Items.Refresh();
		}

		private void QuickComplete()
		{
			TodoItem newtd = new TodoItem
			{
				Todo = tbNewTodo.Text,
				Severity = _tCurrentSeverity,
				Rank = _tIncompleteItems.Count, 
				IsComplete = true
			};

			_tIncompleteItems.Add(newtd);
			AutoSave();
			RefreshTodo();
			tbNewTodo.Clear();
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

		// TODO: Fix this for more Tabs
		private void mnuResetTimer_Click(object sender, EventArgs e)
		{
			ListBox lb = null;
			List<TodoItem> list = new List<TodoItem>();
			
			if(tabTodo.IsSelected)
			{
				lb = lbIncompleteItems;
				list = _tIncompleteItems;
			}
			else if(tabBug.IsSelected)
			{
				lb = lbIncompleteBugItems;
				list = _tIncompleteBugItems;
			}
			else if(tabFeature.IsSelected)
			{
				lb = lbIncompleteFeatureItems;
				list = _tIncompleteFeatureItems;
			}
			
			int index = lb.SelectedIndex;
			if (index < 0)
				return;
			TodoItem td = list[index];
			td.TimeTaken = new DateTime();
			td.IsTimerOn = false;
			lb.Items.Refresh();
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
		
		// TODO: Fix this for more Tabs
		private void mnuTDelete_Click(object sender, EventArgs e)
		{
			ListBox lb = null;
			List<TodoItem> list = new List<TodoItem>();
			
			if(tabTodo.IsSelected)
			{
				lb = lbIncompleteItems;
				list = _tIncompleteItems;
			}
			else if(tabBug.IsSelected)
			{
				lb = lbIncompleteBugItems;
				list = _tIncompleteBugItems;
			}
			else if(tabFeature.IsSelected)
			{
				lb = lbIncompleteFeatureItems;
				list = _tIncompleteFeatureItems;
			}
			
			list.RemoveAt(lb.SelectedIndex);
			
			AutoSave();
			RefreshTodo();
		}

		// TODO: Fix this for more Tabs
		private void mnuTEdit_Click(object sender, EventArgs e)
		{
			ListBox lb = null;
			List<TodoItem> list = new List<TodoItem>();
			
			if(tabTodo.IsSelected)
			{
				lb = lbIncompleteItems;
				list = _tIncompleteItems;
			}
			else if(tabBug.IsSelected)
			{
				lb = lbIncompleteBugItems;
				list = _tIncompleteBugItems;
			}
			else if(tabFeature.IsSelected)
			{
				lb = lbIncompleteFeatureItems;
				list = _tIncompleteFeatureItems;
			}
			
			if(lb.SelectedItems.Count > 1)
				MultiEditItems(lb, list);
			else
				EditItem(lb, list);
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
				_tIncompleteItems.Add(td);
				td.Rank = _tIncompleteItems.Count;
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
		
		// TODO: Fix this for more Tabs
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
		// TODO: Fix this for more Tabs
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
		// TODO: Fix this for more Tabs
		private void btnTAdd_Click(object sender, EventArgs e)
		{
			TextBox tb = null;
			
			if(tabTodo.IsSelected)
				tb = tbNewTodo;
			else if(tabBug.IsSelected)
				tb = tbBugNewTodo;
			else if(tabFeature.IsSelected)
				tb = tbFeatureNewTodo;
			if (tb == null)
				return;
			
			TodoItem td = new TodoItem() {Todo = tb.Text, Severity = _tCurrentSeverity};

			_tIncompleteItems.Add(td);
			td.Rank = int.MaxValue;
			if (td.Severity == 3)
				td.Rank = 0;
			AutoSave();
			RefreshTodo();
			tb.Clear();
		}

		// TODO: Fix this for more Tabs
		private void btnTComplete_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			object item = b?.DataContext;
			if (item != null)
			{
				ListBox lb = null;
				List<TodoItem> list = new List<TodoItem>();
				
				if(tabTodo.IsSelected)
				{
					lb = lbIncompleteItems;
					list = _tIncompleteItems;
				}
				else if(tabBug.IsSelected)
				{
					lb = lbIncompleteBugItems;
					list = _tIncompleteBugItems;
				}
				else if(tabFeature.IsSelected)
				{
					lb = lbIncompleteFeatureItems;
					list = _tIncompleteFeatureItems;
				}
				
				int index = lb.Items.IndexOf(item);
				TodoItem td = list[index];

				TodoItemComplete tdc = new TodoItemComplete(td);
				tdc.ShowDialog();
				if (tdc.isOk)
				{
					list.RemoveAt(index);
					list.Add(tdc.Result);
					AutoSave();
				}
			}
			RefreshTodo();
		}

		// TODO: Fix this for more Tabs
		private void btnRank_Click(object sender, EventArgs e)
		{
			if (sender is Button b)
			{
				TodoItem td = b.DataContext as TodoItem;
				
				List<TodoItem> list = new List<TodoItem>();
				if (tabTodo.IsSelected)
					list = _tIncompleteItems;
				else if (tabBug.IsSelected)
					list = _tIncompleteBugItems;
				else if(tabFeature.IsSelected)
					list = _tIncompleteFeatureItems;

				if (list.Count == 0)
					return;
				
				var index = list.IndexOf(td);
				if ((string) b.CommandParameter == "up")
				{
					if (index == 0)
						return;
					int newRank = list[index - 1].Rank;
					if (td != null)
					{
						list[index - 1].Rank = td.Rank;
						td.Rank = newRank;
						AutoSave();
					}
				}
				else if ((string) b.CommandParameter == "down")
				{
					if (index >= list.Count)
						return;
					int newRank = list[index + 1].Rank;
					if (td != null)
					{
						list[index + 1].Rank = td.Rank;
						td.Rank = newRank;
						AutoSave();
					}
				}
			}
			RefreshTodo();
		}
		
		// TODO: Fix this for more Tabs
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
			lbIncompleteBugItems.Items.Refresh();
			lbIncompleteFeatureItems.Items.Refresh();
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
				_tIncompleteItems.Add(tdie.Result);
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

		private void NewTodo_OnTextChanged(object sender, EventArgs e)
		{
			tbFeatureNewTodo.Text = tbNewTodo.Text;
			tbBugNewTodo.Text = tbNewTodo.Text;
		}
		private void NewTodoBug_OnTextChanged(object sender, EventArgs e)
		{
			tbFeatureNewTodo.Text = tbBugNewTodo.Text;
			tbNewTodo.Text = tbBugNewTodo.Text;
		}
		private void NewTodoFeature_OnTextChanged(object sender, EventArgs e)
		{
			tbBugNewTodo.Text = tbFeatureNewTodo.Text;
			tbNewTodo.Text = tbFeatureNewTodo.Text;
		}
			
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Sorting //
		private void cbTHashtags_SelectionChanged(object sender, EventArgs e)
		{
			ComboBox cb = sender as ComboBox;
			if (cb == null)
				return;

			// TODO: Fix this for more Tabs
			List<string> hashTags = new List<string>();
			if (tabTodo.IsSelected)
				hashTags = _tHashTags;
			else if (tabBug.IsSelected)
				hashTags = _bugHashTags;
			else if(tabFeature.IsSelected)
				hashTags = _featureHashTags;
			if (hashTags.Count == 0)
				return;
			
			_hashToSortBy = hashTags[0];
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
			
			// TODO: Fix this for more Tabs
			List<string> hashTags = new List<string>();
			if (tabTodo.IsSelected)
				hashTags = _tHashTags;
			else if (tabBug.IsSelected)
				hashTags = _bugHashTags;
			else if(tabFeature.IsSelected)
				hashTags = _featureHashTags;
			if (hashTags.Count == 0)
				return;
			
			if ((string) b?.CommandParameter == "hash")
			{
				_tCurrentHashTagSortIndex++;
				if (_tCurrentHashTagSortIndex >= hashTags.Count)
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
			
			// TODO: Fix this for more Tabs
			List<string> hashTags = new List<string>();
			if (tabTodo.IsSelected)
				hashTags = _tHashTags;
			else if (tabBug.IsSelected)
				hashTags = _bugHashTags;
			else if(tabFeature.IsSelected)
				hashTags = _featureHashTags;
			if (hashTags.Count == 0)
				return list;
			
			if (_hashSortSelected)
			{
				_tCurrentHashTagSortIndex = 0;
				foreach (string s in hashTags)
				{
					if (s.Equals(_hashToSortBy))
						break;
					_tCurrentHashTagSortIndex++;
				}
			}

			for (int i = 0 + _tCurrentHashTagSortIndex; i < hashTags.Count; i++)
				sortedHashTags.Add(hashTags[i]);
			for (int i = 0; i < _tCurrentHashTagSortIndex; i++)
				sortedHashTags.Add(hashTags[i]);

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
		
		// TODO: Fix this for more Tabs
		private void FixRankings()
		{
			_tIncompleteItems = _tIncompleteItems.OrderBy(o => o.Rank).ToList();
			_tIncompleteBugItems = _tIncompleteBugItems.OrderBy(o => o.Rank).ToList();
			_tIncompleteFeatureItems = _tIncompleteFeatureItems.OrderBy(o => o.Rank).ToList();
			
			for (int i = 0; i < _tIncompleteItems.Count; i++)
				_tIncompleteItems[i].Rank = i + 1;
			for (int i = 0; i < _tIncompleteBugItems.Count; i++)
				_tIncompleteBugItems[i].Rank = i + 1;
			for (int i = 0; i < _tIncompleteFeatureItems.Count; i++)
				_tIncompleteFeatureItems[i].Rank = i + 1;
		}
		
		private void RefreshTodo()
		{
			// TODO: Fix this for more Tabs
			List<TodoItem> incompleteItems = new List<TodoItem>();
			List<TodoItem> incompleteBugItems = new List<TodoItem>();
			List<TodoItem> incompleteFeatureItems = new List<TodoItem>();
			List<string> hashTagList = new List<string>();
			List<string> bugHashTagList = new List<string>();
			List<string> featureHashTagList = new List<string>();
			_tIncompleteItems.AddRange(_tIncompleteBugItems);
			_tIncompleteItems.AddRange(_tIncompleteFeatureItems);
			
			foreach (TodoItem td in _tIncompleteItems)
			{
				if (td.IsComplete)
				{
					td.Rank = 0;
					AddTodoToHistory(td);
				}
				else if (td.Tags.Contains("#BUG"))
				{
					incompleteBugItems.Add(td);
					foreach (string s in td.Tags)
						if(!bugHashTagList.Contains(s))
							bugHashTagList.Add(s);
				}
				else if (td.Tags.Contains("#FEATURE"))
				{
					incompleteFeatureItems.Add(td);
					foreach (string s in td.Tags)
						if(!featureHashTagList.Contains(s))
							featureHashTagList.Add(s);
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
			_tIncompleteBugItems = incompleteBugItems;
			_tIncompleteFeatureItems = incompleteFeatureItems;
			hashTagList = hashTagList.OrderBy(o => o).ToList();
			bugHashTagList = bugHashTagList.OrderBy(o => o).ToList();
			featureHashTagList = featureHashTagList.OrderBy(o => o).ToList();
			
			_tHashTags = _tHashTags.OrderBy(o => o).ToList();
			_tDidHashChange = false;
			if (_tHashTags.Count != hashTagList.Count)
				_tDidHashChange = true;
			else
				for (int i = 0; i < _tHashTags.Count; i++)
					if (_tHashTags[i] != hashTagList[i])
						_tDidHashChange = true;
			
			_tHashTags = hashTagList;
			_bugHashTags = bugHashTagList;
			_featureHashTags = featureHashTagList;

			FixRankings();

			switch (_tCurrentSort)
			{
				case "sev":
					_tIncompleteItems = _tReverseSort
						? _tIncompleteItems.OrderByDescending(o => o.Severity).ToList()
						: _tIncompleteItems.OrderBy(o => o.Severity).ToList();
					_tIncompleteBugItems = _tReverseSort
						? _tIncompleteBugItems.OrderByDescending(o => o.Severity).ToList()
						: _tIncompleteBugItems.OrderBy(o => o.Severity).ToList();
					_tIncompleteFeatureItems = _tReverseSort
						? _tIncompleteFeatureItems.OrderByDescending(o => o.Severity).ToList()
						: _tIncompleteFeatureItems.OrderBy(o => o.Severity).ToList();
					break;
				case "date":
					_tIncompleteItems = _tReverseSort
						? _tIncompleteItems.OrderByDescending(o => o.TimeStarted).ToList()
						: _tIncompleteItems.OrderBy(o => o.TimeStarted).ToList();
					_tIncompleteItems = _tReverseSort
						? _tIncompleteItems.OrderByDescending(o => o.DateStarted).ToList()
						: _tIncompleteItems.OrderBy(o => o.DateStarted).ToList();
					
					_tIncompleteBugItems = _tReverseSort
						? _tIncompleteBugItems.OrderByDescending(o => o.TimeStarted).ToList()
						: _tIncompleteBugItems.OrderBy(o => o.TimeStarted).ToList();
					_tIncompleteBugItems = _tReverseSort
						? _tIncompleteBugItems.OrderByDescending(o => o.DateStarted).ToList()
						: _tIncompleteBugItems.OrderBy(o => o.DateStarted).ToList();
					
					_tIncompleteFeatureItems = _tReverseSort
						? _tIncompleteFeatureItems.OrderByDescending(o => o.TimeStarted).ToList()
						: _tIncompleteFeatureItems.OrderBy(o => o.TimeStarted).ToList();
					_tIncompleteFeatureItems = _tReverseSort
						? _tIncompleteFeatureItems.OrderByDescending(o => o.DateStarted).ToList()
						: _tIncompleteFeatureItems.OrderBy(o => o.DateStarted).ToList();
					break;
				case "hash":
					_tIncompleteItems = SortByHashTag(_tIncompleteItems);
					_tIncompleteBugItems = SortByHashTag(_tIncompleteBugItems);
					_tIncompleteFeatureItems = SortByHashTag(_tIncompleteFeatureItems);
					break;
				case "rank":
					_tIncompleteItems = _tReverseSort
						? _tIncompleteItems.OrderByDescending(o => o.Rank).ToList()
						: _tIncompleteItems.OrderBy(o => o.Rank).ToList();
					_tIncompleteBugItems = _tReverseSort
						? _tIncompleteBugItems.OrderByDescending(o => o.Rank).ToList()
						: _tIncompleteBugItems.OrderBy(o => o.Rank).ToList();
					_tIncompleteFeatureItems = _tReverseSort
						? _tIncompleteFeatureItems.OrderByDescending(o => o.Rank).ToList()
						: _tIncompleteFeatureItems.OrderBy(o => o.Rank).ToList();
					break;
				case "active":
					_tIncompleteItems = _tReverseSort
						? _tIncompleteItems.OrderByDescending(o => o.IsTimerOn).ToList()
						: _tIncompleteItems.OrderBy(o => o.IsTimerOn).ToList();
					_tIncompleteBugItems = _tReverseSort
						? _tIncompleteBugItems.OrderByDescending(o => o.IsTimerOn).ToList()
						: _tIncompleteBugItems.OrderBy(o => o.IsTimerOn).ToList();
					_tIncompleteFeatureItems = _tReverseSort
						? _tIncompleteFeatureItems.OrderByDescending(o => o.IsTimerOn).ToList()
						: _tIncompleteFeatureItems.OrderBy(o => o.IsTimerOn).ToList();
					break;
			}

			lbIncompleteItems.ItemsSource = _tIncompleteItems;
			lbIncompleteItems.Items.Refresh();
			lbIncompleteBugItems.ItemsSource = _tIncompleteBugItems;
			lbIncompleteBugItems.Items.Refresh();
			lbIncompleteFeatureItems.ItemsSource = _tIncompleteFeatureItems;
			lbIncompleteFeatureItems.Items.Refresh();
			cbBugHashtags.ItemsSource = _bugHashTags;
			cbBugHashtags.Items.Refresh();
			cbFeatureHashtags.ItemsSource = _featureHashTags;
			cbFeatureHashtags.Items.Refresh();
			cbHashtags.ItemsSource = _tHashTags;
			cbHashtags.Items.Refresh();
//			cbBugHashtags.ItemsSource = _bugHashTags;
//			cbBugHashtags.Items.Refresh();

//			foreach (ListBoxItem lbi in lbTIncompleteItems.ItemContainerGenerator.Items)
//			{
//				int test = 0;
//			}
//			foreach (TodoItem td in _tIncompleteItems)
//			{
//				VirtualizingStackPanel.SetIsVirtualizing(lbTIncompleteItems, false);
//				int index = lbTIncompleteItems.Items.IndexOf(td);
//				ListBoxItem lbi22 = lbTIncompleteItems.Items.GetItemAt(index) as ListBoxItem;
//				ListBoxItem lbi = lbTIncompleteItems.Items[index] as ListBoxItem;
//			int index = 0;
//			var lbi3 = (ListBoxItem) lbTIncompleteItems.SelectedItem;
//				var lbi = lbTIncompleteItems.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
//				var lbi2 = lbTIncompleteItems.ItemContainerGenerator.ContainerFromItem(_tIncompleteItems[0]) as ListBoxItem;
//				if (_tCurrentSort == "hash" && td.Tags.Contains(_hashToSortBy))
//					lbi.Background = Brushes.Red;
//				else
//					lbi.Background=Brushes.Transparent;

//			}
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
			incompleteItems.AddRange(_tIncompleteItems);
			incompleteItems.AddRange(_tIncompleteBugItems);
			incompleteItems.AddRange(_tIncompleteFeatureItems);
			
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

			_tIncompleteItems.Clear();
			_tIncompleteBugItems.Clear();
			_tIncompleteFeatureItems.Clear();
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
