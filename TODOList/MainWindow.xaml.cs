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
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Button = System.Windows.Controls.Button;
using Clipboard = System.Windows.Forms.Clipboard;
using ComboBox = System.Windows.Controls.ComboBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListBox = System.Windows.Controls.ListBox;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.Forms.MessageBox;
using TextBox = System.Windows.Controls.TextBox;


namespace TODOList
{
	public partial class MainWindow : INotifyPropertyChanged
	{
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
		public const string DATE = "yyyMMdd";
		public const string TIME = "HHmmss";
		public const string VERSION = "2.04";
		public const float VERSIONINCREMENT = 0.01f;

		private List<TabItem> tabItemList;

		// TO DO TAB ITEMS
		private List<TodoItem> _currentList;
		private List<string> _currentHashTags;

		private List<TodoItem> _masterList;
		private List<List<TodoItemHolder>> _incompleteItems;
		private List<List<string>> _hashTags;
		private List<string> _tabHash;
		private static Dictionary<string, string> _hashShortcuts;
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
		private int _currentHistoryItemIndex;
		private int _currentHistoryItemMouseIndex;
		private bool _didMouseSelect;

		// FILE IO
		private const string basePath = @"C:\MyBinaries\";
		private ObservableCollection<string> _recentFiles;
		private string _currentOpenFile;
		private bool _isChanged;
		private int _recentFilesIndex;
		private bool _autoSave;
		private TimeSpan _backupTime;
		private TimeSpan _timeUntilBackup;
		private int _backupIncrement;

		// WINDOW ITEMS
		private double top;
		private double left;
		private double height = 1080;
		private double width = 1920;

		// HOTKEY STUFF
		private bool _globalHotkeys;
		private const int HOTKEY_ID = 9000;
		private const uint MOD_WIN = 0x0008; //WINDOWS
		private HwndSource source;
		private IntPtr _handle;
		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
		
		// POMOTIMER
		private DateTime _pomoTimer;
		private bool _isPomoTimerOn;
		private bool _isPomoWorkTimerOn = true;
		private int _pomoWorkTime = 25;
		private int _pomoBreakTime = 5;
		private string _pomoTimerString;
		private int _pomoTimeLeft;
		
		// CONTROLS
		private ComboBox cbHashTags;
		private ListBox lbIncompleteItems;
//		private TextBox tbNewTodo;


		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		// TODO: Fix this for more Tabs
		public int PomoTimeLeft
		{
			get => _pomoTimeLeft;
			set
			{
				_pomoTimeLeft = value;
				OnPropertyChanged();
			}
		}
		public List<TodoItemHolder> IncompleteItems => _incompleteItems[tabTest.SelectedIndex];
		public List<string> HashTags => _hashTags[tabTest.SelectedIndex];
		public string TabHash => _tabHash[tabTest.SelectedIndex];
		
		public List<HistoryItem> HistoryItems => _hHistoryItems;
		
		public string WindowTitle => "EthereaListVCSNotes v" + VERSION + " " + _currentOpenFile;
		public ObservableCollection<string> RecentFiles
		{
			get => _recentFiles;
			set => _recentFiles = value;
		}
		public bool IsPomoTimerOn
		{
			get => _isPomoTimerOn;
			set
			{
				_isPomoTimerOn = value;
				OnPropertyChanged();
			}
		}
		

		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		// TODO: Fix this for more Tabs
		public  MainWindow()
		{
			left = 0;
			InitializeComponent();
			
#if DEBUG
			mnuMain.Background = Brushes.Red;
#endif
			
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
			_backupTime = new TimeSpan(0, 5, 0);
			_timeUntilBackup = _backupTime;
			_backupIncrement = 0;

			Closing += Window_Closed;

			tabItemList = new List<TabItem>();

			_masterList = new List<TodoItem>();
			_incompleteItems = new List<List<TodoItemHolder>>();
			_hashTags = new List<List<string>>();
			_tabHash = new List<string>();
			_hashShortcuts = new Dictionary<string, string>();
			
			_hHistoryItems = new List<HistoryItem>();
			_hCurrentHistoryItem = new HistoryItem("", "");

			tabTest.ItemsSource = tabItemList;
			tabTest.Items.Refresh();

			lbHistory.SelectedIndex = 0;
			_currentHistoryItemIndex = 0;
			lbCompletedTodos.SelectedIndex = 0;
			
			lblPomoWork.Content = _pomoWorkTime.ToString();
			lblPomoBreak.Content = _pomoBreakTime.ToString();
			
			if (_recentFiles.Count > 0)
				Load(_recentFiles[0]);
			else
				CreateNewTabs();
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
			name = UpperFirstLetter(name);
			ti.Header = name;
			ti.Name = name;
			foreach (TabItem existingTabItem in tabItemList)
			{
				if (existingTabItem.Name == name)
				{
					DlgYesNo dlg = new DlgYesNo("Already have a tab called " + name);
					dlg.ShowDialog();
//					MessageBox.Show("Already have a tab called " + name);
					return;
				}
			}

			if (name != "All")
			{
				string hash = "#" + name.ToUpper();
				string hashShortcut = null;
				hashShortcut = GetHashShortcut(name, hashShortcut);
				_hashShortcuts.Add(hashShortcut, name);
				_tabHash.Add(hash);
			}
			
			_incompleteItems.Add(new List<TodoItemHolder>());
			_hashTags.Add(new List<string>());
			tabItemList.Add(ti);
			RefreshTodo();
			tabTest.Items.Refresh();
			AutoSave();
		}
		public string GetHashShortcut(string name, string shortcut)
		{
			string hashShortcut = shortcut + name[0].ToString().ToLower();
			string leftover = "";
			for (int i = 1; i < name.Length; i++)
				leftover += name[i];

			if (_hashShortcuts.ContainsKey(hashShortcut))
				return hashShortcut = GetHashShortcut(leftover, hashShortcut);
			else
				return hashShortcut;
		}
		private void RemoveTodoTab(int index)
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
			AutoSave();
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

			_handle = new WindowInteropHelper(this).Handle;
			source = HwndSource.FromHwnd(_handle);
			if (source != null) 
				source.AddHook(HwndHook);
			if(_globalHotkeys)
				RegisterHotKey(_handle, HOTKEY_ID, MOD_WIN, 0x73);

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
			_timeUntilBackup = _timeUntilBackup.Subtract(new TimeSpan(0, 0, 1));
			if (_timeUntilBackup <= TimeSpan.Zero)
			{
				_timeUntilBackup = _backupTime;
				BackupSave();
			}
			foreach (TodoItem td in _masterList)
			{
				if (td.IsTimerOn)
					td.TimeTaken = td.TimeTaken.AddSeconds(1);
			}
			for(int i = 0; i < _incompleteItems.Count; i++)
				foreach (TodoItemHolder tlh in _incompleteItems[i])
					if (tlh.TD.IsTimerOn)
					{
//						tlh.TimeTakenInMinutes = tlh.TD.TimeTakenInMinutes;
						tlh.TimeTaken = tlh.TD.TimeTaken;
					}
			
			lblPomo.Content = String.Format("{0:00}:{1:00}", _pomoTimer.Ticks / TimeSpan.TicksPerMinute, _pomoTimer.Second);
//			lblPomo.Foreground = Brushes.DarkGray;
			
			if (_isPomoTimerOn)
			{
//				lblPomo.Foreground = Brushes.Black;
				pbPomo.Background = Brushes.Maroon;
				_pomoTimer = _pomoTimer.AddSeconds(1);
				
				if(_isPomoWorkTimerOn)
				{
					long ticks = _pomoWorkTime * TimeSpan.TicksPerMinute;
					PomoTimeLeft = (int)((float) _pomoTimer.Ticks / ticks * 100);
//					pbPomo.Background = Brushes.Lime;
					if (_pomoTimer.Ticks >= ticks)
					{
						_isPomoWorkTimerOn = false;
						_pomoTimer=DateTime.MinValue;
					}
				}
				else
				{
					long ticks = _pomoBreakTime * TimeSpan.TicksPerMinute;
					PomoTimeLeft = (int)((float) (ticks - _pomoTimer.Ticks) / ticks * 100);
//					lblPomo.Foreground = Brushes.DarkGray;
					if (_pomoTimer.Ticks >= ticks)
					{
						_isPomoWorkTimerOn = true;
						_pomoTimer=DateTime.MinValue;
					}
				}
			}
			else
			{
				pbPomo.Background = Brushes.Transparent;
				lblPomo.Background = Brushes.Transparent;
			}
		}
		private void Window_Closed(object sender, CancelEventArgs e)
		{
			UnregisterHotKey(_handle, HOTKEY_ID);
			SaveSettings();
			if (!_isChanged)
				return;

			DlgYesNo dlg = new DlgYesNo("Close", "Maybe save first?");
			dlg.ShowDialog();
			if(dlg.Result)
//			if (MessageBox.Show("Maybe save first?", "Close", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
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

			TodoItemHolder tlh = (TodoItemHolder) lbIncompleteItems.SelectedItem;
			if (tlh != null)
			{
				DlgTodoItemComplete tdc = new DlgTodoItemComplete(tlh.TD);
				tdc.ShowDialog();
				if (tdc.isOk)
				{
//					IncompleteItems.RemoveAt(lbIncompleteItems.SelectedIndex);
//					TodoListHolder tlh = new TodoListHolder(tdc.Result);
//					IncompleteItems.Add(tlh);
					_masterList.Add(tdc.Result);
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
				foreach (TodoItemHolder tlh in IncompleteItems)
					list.Add(tlh.TD);
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
			IncompleteItems[index].TD.IsTimerOn = !IncompleteItems[index].TD.IsTimerOn;
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

			_masterList.Add(newtd);
//			TodoListHolder tlh = new TodoListHolder(newtd);
//			IncompleteItems.Add(tlh);
			AutoSave();
			RefreshTodo();
			tbNewTodo.Clear();
		}
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// MenuCommands //
		private void EditTabs_OnClick(object sender, EventArgs e)
		{
			List<TabItem> list = tabItemList.ToList();
			DlgEditTabs rt = new DlgEditTabs(list);
			rt.ShowDialog();
			if (!rt.Result)
				return;
			tabItemList.Clear();
			_hashShortcuts.Clear();
			_tabHash.Clear();
			foreach (string s in rt.ResultList)
				AddNewTodoTab(s);
		}
		private void mnuNew_Click(object sender, EventArgs e)
		{
			DlgYesNo dlg = new DlgYesNo("New file", "Are you sure?");
			dlg.ShowDialog();
			if (!dlg.Result)
//			if (MessageBox.Show("Are you sure?", "New File", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
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
//			mnuRecentSaves.Items.Refresh();
			mnuRecentLoads.Items.Refresh();
		}
		private void mnuRecentSavesRMB(object sender, MouseButtonEventArgs e)
		{
			_recentFilesIndex = -1;
			var mi = e.OriginalSource as TextBlock;
			if (mi == null)
				return;
			string path = (string) mi.DataContext;
//			_recentFilesIndex = mnuRecentSaves.Items.IndexOf(path);
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
			TodoItemHolder tlh = IncompleteItems[index];
			tlh.TimeTaken = new DateTime();
			tlh.TD.IsTimerOn = false;
			lbIncompleteItems.Items.Refresh();
		}
		private void mnuResetHistoryCopied(object sender, EventArgs e)
		{
			int index = lbHistory.SelectedIndex;
			_hHistoryItems[index].ResetCopied();
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
			
			DlgYesNo dlg = new DlgYesNo("Saving over ", "Are you sure you want to save over " + path);
			dlg.ShowDialog();
			if (!dlg.Result)
//			if (MessageBox.Show("Save over " + path, "Are you sure you want to save?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
				return;
			Save(path);
		}
		private void mnuSaveAs_Click(object sender, EventArgs e)
		{
			SaveAs();
		}
		private void mnuSave_Click(object sender, EventArgs e)
		{
			if (_currentOpenFile == null)
			{
				DlgYesNo dlg = new DlgYesNo("No current file");
				dlg.ShowDialog();
//				if(dlg.Result)
//				MessageBox.Show("No current file", "Nope");
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
			{
				DlgYesNo dlg = new DlgYesNo("Close", "Maybe save first?");
				dlg.ShowDialog();
				if (dlg.Result)
//				if (MessageBox.Show("Maybe save first?", "Close", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
					Save(_currentOpenFile);
			}
			
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
			{
				DlgYesNo dlg = new DlgYesNo("Close", "Maybe save first?");
				dlg.ShowDialog();
				if (dlg.Result)
//				if (MessageBox.Show("Maybe save first?", "Close", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
					Save(_currentOpenFile);
			}

			Load(ofd.FileName);
		}
		private void mnuTDelete_Click(object sender, EventArgs e)
		{
			_masterList.Remove(IncompleteItems[lbIncompleteItems.SelectedIndex].TD);
			IncompleteItems.RemoveAt(lbIncompleteItems.SelectedIndex);
			
			AutoSave();
			RefreshTodo();
		}
		private void mnuTEdit_Click(object sender, EventArgs e)
		{
			List<TodoItem> list = new List<TodoItem>();
			foreach (TodoItemHolder tlh in IncompleteItems)
					list.Add(tlh.TD);
			if(lbIncompleteItems.SelectedItems.Count > 1)
				MultiEditItems(lbIncompleteItems, list);
			else
				EditItem(lbIncompleteItems, list);
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
		private void GlobalHotkeysToggle_OnClick(object sender, EventArgs e)
		{
			_globalHotkeys = (bool) ckGlobalHotkeys.IsChecked;
			if (_globalHotkeys)
				RegisterHotKey(_handle, HOTKEY_ID, MOD_WIN, 0x73);
			else
				UnregisterHotKey(_handle, HOTKEY_ID);

		}
		private void mnuHelp_Click(object sender, EventArgs e)
		{
			DlgHelp dlgH = new DlgHelp();
			dlgH.Show();
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
		private void lbHHistory_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (_currentHistoryItemMouseIndex != -1)
			{
				_currentHistoryItemIndex = _currentHistoryItemMouseIndex;
				_currentHistoryItemMouseIndex = -1;
			}
			int prevIndex = _currentHistoryItemIndex;
			switch (e.Key)
			{
				case Key.Down:
					_currentHistoryItemIndex++;
					break;
				case Key.Up:
					_currentHistoryItemIndex--;
					break;
				default:
					break;
			}
			
			if (_currentHistoryItemIndex >= _hHistoryItems.Count)
				_currentHistoryItemIndex = 0;
			if (_currentHistoryItemIndex < 0)
				_currentHistoryItemIndex = _hHistoryItems.Count - 1;
			if (_hHistoryItems.Count == 0)
			{
				_hCurrentHistoryItem = new HistoryItem("", "");
				return;
			}
			if (prevIndex == _currentHistoryItemIndex)
				return;

			_hCurrentHistoryItem = lbHistory.Items[_currentHistoryItemIndex] as HistoryItem;
			lblHTotalTime.Content = _hCurrentHistoryItem.TotalTime;
			lbHistory.SelectedIndex = _currentHistoryItemIndex;
			RefreshHistory();
		}
		private void LBHHistory_OnMouseDown(object sender, EventArgs e)
		{
			_didMouseSelect = true;
		}
		private void lbHHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(_didMouseSelect)
				_currentHistoryItemIndex = lbHistory.SelectedIndex;
			
			_hCurrentHistoryItem = lbHistory.Items[_currentHistoryItemIndex] as HistoryItem;
			lblHTotalTime.Content = _hCurrentHistoryItem.TotalTime;
//			lbHistory.SelectedIndex = _currentHistoryItemIndex;
			
			RefreshHistory();
			_didMouseSelect = false;
		}
		private void btnHDeleteTodo_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;
			TodoItem td = b?.DataContext as TodoItem;
//			if (MessageBox.Show("Delete", "Are you sure", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
			DlgYesNo dlgYN = new DlgYesNo("Delete", "Are you sure you want to delete this Todo?");
			dlgYN.ShowDialog();
			if (!dlgYN.Result)
				return;

			if (td != null)
			{
				td.IsComplete = false;
				
//				TodoListHolder tlh = new TodoListHolder(td);
//				IncompleteItems.Add(tlh);
//				td.Rank = IncompleteItems.Count;
				_masterList.Add(td);
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
//			if (MessageBox.Show("Delete", "Are you sure", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
			DlgYesNo dlgYN = new DlgYesNo("Delete", "Are you sure you want to delete this History Item?");
			dlgYN.ShowDialog();
			if (!dlgYN.Result)
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
				hi.SetCopied();
				if (lbHistory.Items.IndexOf(hi) == 0)
				{
					
					// TODO: Add new history dialog here
//					if(MessageBox.Show("Add a new History Item?","New History",MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
					DlgYesNo dlgYN = new DlgYesNo("New History", "Add a new History Item?");
					dlgYN.ShowDialog();
					if(dlgYN.Result)
						AddNewHistoryItem();
				}
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
			string[] versionPieces = VERSION.Split(' ');
			DlgAddNewHistory dlgANH = new DlgAddNewHistory(Convert.ToSingle(versionPieces[0]), VERSIONINCREMENT);
			dlgANH.ShowDialog();
			if (!dlgANH.Result)
				return;
			
			_hCurrentHistoryItem = new HistoryItem(DateTime.Now);
			_hCurrentHistoryItem.Title = dlgANH.ResultTitle;
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

			int index = lbHistory.SelectedIndex;
			lbHistory.Items.Refresh();
			lbHistory.SelectedIndex = index;
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
			td.Rank = int.MaxValue;
			if (td.Severity == 3)
				td.Rank = 0;

			_masterList.Add(td);

//			TodoListHolder tlh = new TodoListHolder(td);
//			_incompleteItems[0].Add(tlh);
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
				TodoItem td = IncompleteItems[index].TD;

				DlgTodoItemComplete tdc = new DlgTodoItemComplete(td);
				tdc.ShowDialog();
				if (tdc.isOk)
				{
					TodoItem completeTD = tdc.Result;
					_masterList.RemoveAt(index);
					_masterList.Add(completeTD);
//					TodoListHolder tlh = new TodoListHolder(tdc.Result);
//					IncompleteItems.RemoveAt(index);
//					IncompleteItems.Add(tlh);
					AutoSave();
				}
			}
			RefreshTodo();
		}
		private void btnRank_Click(object sender, EventArgs e)
		{
			if (sender is Button b)
			{
				TodoItemHolder tlh = b.DataContext as TodoItemHolder;

				if (IncompleteItems.Count == 0)
					return;
				var index = IncompleteItems.IndexOf(tlh);
				if ((string) b.CommandParameter == "up")
				{
					if (index == 0)
						return;
					int newRank = IncompleteItems[index - 1].TD.Rank;
					if (tlh != null)
					{
						IncompleteItems[index - 1].TD.Rank = tlh.Rank;
						tlh.Rank = newRank;
						AutoSave();
					}
				}
				else if ((string) b.CommandParameter == "down")
				{
					if (index >= IncompleteItems.Count)
						return;
					int newRank = IncompleteItems[index + 1].TD.Rank;
					if (tlh != null)
					{
						IncompleteItems[index + 1].TD.Rank = tlh.Rank;
						tlh.Rank = newRank;
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
				TodoItemHolder tlh = b.DataContext as TodoItemHolder;

				if ((string) b.CommandParameter == "start")
				{
					if (tlh != null)
						tlh.TD.IsTimerOn = true;
				}
				else if ((string) b.CommandParameter == "stop")
				{
					if (tlh != null)
						tlh.TD.IsTimerOn = false;
				}
				
				AutoSave();
			}

			lbIncompleteItems.Items.Refresh();
		}
		private void btnPomoTimerStart_OnClick(object sender, EventArgs e)
		{
			_isPomoTimerOn = true;
		}
		private void btnPomoTimerPause_OnClick(object sender, EventArgs e)
		{
			_isPomoTimerOn = false;
		}
		private void btnPomoTimerStop_OnClick(object sender, EventArgs e)
		{
			_isPomoTimerOn = false;
			_pomoTimer = DateTime.MinValue;
			PomoTimeLeft = 0;
		}
		private void btnPomoWorkInc_OnClick(object sender, EventArgs e)
		{
			int value = Convert.ToInt16((string) (sender as Button).CommandParameter);
			_pomoWorkTime += value;
			lblPomoWork.Content = _pomoWorkTime.ToString();
		}
		private void btnPomoWorkDec_OnClick(object sender, EventArgs e)
		{
			int value = Convert.ToInt16((string) (sender as Button).CommandParameter);
			_pomoWorkTime -= value;
			if (_pomoWorkTime <= 0)
				_pomoWorkTime = value;
			lblPomoWork.Content = _pomoWorkTime.ToString();
		}
		private void btnPomoBreakInc_OnClick(object sender, EventArgs e)
		{
			int value = Convert.ToInt16((string) (sender as Button).CommandParameter);
			_pomoBreakTime += value;
			lblPomoBreak.Content = _pomoBreakTime.ToString();
		}
		private void btnPomoBreakDec_OnClick(object sender, EventArgs e)
		{
			int value = Convert.ToInt16((string) (sender as Button).CommandParameter);
			_pomoBreakTime -= value;
			if (_pomoBreakTime <= 0)
				_pomoBreakTime = value;
			lblPomoBreak.Content = _pomoBreakTime.ToString();
		}
		private void EditItem(ListBox lb, List<TodoItem> list)
		{
			int index = lb.SelectedIndex;
			if (index < 0)
				return;
			TodoItem td = list[index];
			DlgTodoItemEditor tdie = new DlgTodoItemEditor(td);

			tdie.ShowDialog();
			if (tdie.isOk)
			{
//				list.Remove(td);
//				TodoListHolder tlh = new TodoListHolder(tdie.Result);
//				_incompleteItems[0].Add(tlh);
				if(_masterList.Contains(td))
					_masterList.Remove(td);
				if(_hCurrentHistoryItem.CompletedTodos.Contains(td))
					_hCurrentHistoryItem.CompletedTodos.Remove(td);
				if(_hCurrentHistoryItem.CompletedTodosBugs.Contains(td))
					_hCurrentHistoryItem.CompletedTodosBugs.Remove(td);
				if(_hCurrentHistoryItem.CompletedTodosFeatures.Contains(td))
					_hCurrentHistoryItem.CompletedTodosFeatures.Remove(td);
//				_hCurrentHistoryItem.CompletedTodos.Remove(td);
//				_hCurrentHistoryItem.CompletedTodosBugs.Remove(td);
//				_hCurrentHistoryItem.CompletedTodosFeatures.Remove(td);
//				ExpandHashTags(tdie.Result);
				_masterList.Add(tdie.Result);
				AutoSave();
			}

			RefreshTodo();
			RefreshHistory();
		}
		private void MultiEditItems(ListBox lb, List<TodoItem> list)
		{
			TodoItemHolder firstTd = lb.SelectedItems[0] as TodoItemHolder;

			List<string> tags = new List<string>();
			List<string> commonTagsTemp = new List<string>();
			foreach (TodoItemHolder tih in lb.SelectedItems)
			{
				foreach(string tag in tih.TD.Tags)
				{
					if (!tags.Contains(tag))
						tags.Add(tag);
					else
					{
						if(!commonTagsTemp.Contains(tag))
							commonTagsTemp.Add(tag);
					}
				}
			}
			List<string> commonTags = commonTagsTemp.ToList();
			foreach (TodoItemHolder tih in lb.SelectedItems)
			{
				foreach (string tag in commonTagsTemp)
				{
					if (!tih.TD.Tags.Contains(tag))
						commonTags.Remove(tag);
				}
			}
			
			DlgTodoMultiItemEditor tmie = new DlgTodoMultiItemEditor(firstTd.TD, commonTags);
			tmie.ShowDialog();
			if (tmie.isOk)
			{
				List<string> tagsToRemove = new List<string>();
				
				foreach (string tag in commonTags)
				{
					if (!tmie.ResultTags.Contains(tag))
						tagsToRemove.Add(tag);
				}
				foreach (TodoItemHolder tlh in lb.SelectedItems)
				{
					if(tmie.ChangeTag)
					{
						foreach (string tag in tagsToRemove)
							tlh.TD.Tags.Remove(tag);
						foreach (string tag in tmie.ResultTags)
							if(!tlh.TD.Tags.Contains(tag.ToUpper()))
								tlh.TD.Tags.Add(tag.ToUpper());
					}
					if(tmie.ChangeRank)
						tlh.TD.Rank = tmie.Result.Rank;
					if(tmie.ChangeSev)
						tlh.TD.Severity = tmie.Result.Severity;
					if (tmie.isComplete && tmie.ChangeComplete)
						tlh.TD.IsComplete = true;
					if (tmie.ChangeTodo)
					{
						tlh.TD.Todo += Environment.NewLine + tmie.Result.Todo;
						foreach (string tag in tmie.Result.Tags)
							if(!tlh.TD.Tags.Contains(tag))
								tlh.TD.Tags.Add(tag);
					}
				}
				RefreshTodo();
			}
		}
		public static void ExpandHashTags(TodoItem td)
		{
			string tempTodo = ExpandHashTagsInString(td.Todo);
			string tempTags = ExpandHashTagsInList(td.Tags);
			
			td.Todo = tempTags.Trim() + " " + tempTodo.Trim();
		}
		public static string ExpandHashTagsInString(string todo)
		{
			string result = "";
			string[] pieces = todo.Split(' ');
			bool isBeginningTag = false;

			List<string> list = new List<string>();
			for (int index = 0; index < pieces.Length; index++)
			{
				string s = pieces[index];
				if (s.Contains('#'))
				{
//					if (index == 0)
//						isBeginningTag = true;
					
					string t = "";
					t = s.ToUpper();
					if (t.Equals("#FEATURES"))
						t = "#FEATURE";
					if (t.Equals("#BUGS"))
						t = "#BUG";

					foreach (KeyValuePair<string, string> kvp in _hashShortcuts)
					{
						if (!t.Equals("#" + kvp.Key.ToUpper()))
							continue;
						string hash = "#" + kvp.Value;
						s = hash;
					}
					
//					s = s.Remove(0, 1);
					s = s.ToLower();
				}
//				else
//					isBeginningTag = false;
				
//				if (isBeginningTag)
//					continue;
//				if (index == 0 ||
//					index > 0 && pieces[index - 1].Contains(". ") ||
//					index > 0 && pieces[index - 1].Contains("? ") ||
//					list.Count == 0)
//				{
//					s = UpperFirstLetter(s);
//				}
				list.Add(s);
			}

			foreach (string s in list)
			{
				if (s == "")
					continue;
				result += s + " ";
			}

			return result;
		}
		public static string ExpandHashTagsInList(List<string> tags)
		{
			string result = "";
			foreach (string s in tags)
				result += s + " ";

			result = ExpandHashTagsInString(result);
			return result;
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
		private List<TodoItemHolder> SortByHashTag(List<TodoItemHolder> list)
		{
			if (_tDidHashChange)
				_tCurrentHashTagSortIndex = 0;

			List<TodoItemHolder> incompleteItems = new List<TodoItemHolder>();
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

			List<TodoItemHolder> sortSingleTags = list.ToList();
			
			foreach (TodoItemHolder tlh in sortSingleTags)
			{
				if (!tlh.TD.Tags.Contains(sortedHashTags[0]) || tlh.TD.Tags.Count != 1)
					continue;
				incompleteItems.Add(tlh);
				list.Remove(tlh);
			}
			foreach (string s in sortedHashTags)
			{
				List<TodoItemHolder> temp = list.ToList();
				foreach (TodoItemHolder tlh in temp)
				{
					List<string> sortedTags = new List<string>();
					List<string> unsortedTags = tlh.TD.Tags.ToList();
					foreach (string u in tlh.TD.Tags)
					{
						if (u != s)
							continue;
						sortedTags.Add(u);
						unsortedTags.Remove(u);
					}
					sortedTags.AddRange(unsortedTags);
					tlh.TD.Tags = sortedTags;
					
					foreach (string t in tlh.TD.Tags)
					{
						if (!s.Equals(t))
							continue;
						incompleteItems.Add(tlh);
						list.Remove(tlh);
					}
				}
			}
			foreach (TodoItemHolder tlh in list)
				incompleteItems.Add(tlh);
			
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
			for (int i = 0; i < _incompleteItems.Count; i++)
			{
				_incompleteItems[i].Clear();
				_hashTags[i].Clear();
			}
			
			SortToLists();
			SortHashTagLists();
			CheckForHashTagListChanges();
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
		private void CheckForHashTagListChanges()
		{

			_tDidHashChange = false;
			if (_hashTags[0].Count != _prevHashTagList.Count)
				_tDidHashChange = true;
			else
				for (int i = 0; i < _hashTags[0].Count; i++)
					if (_hashTags[0][i] != _prevHashTagList[i])
						_tDidHashChange = true;
			_prevHashTagList = _hashTags[0].ToList();
		}
		private void SortHashTagLists()
		{

			for (int i = 0; i < _incompleteItems.Count; i++)
			{
				foreach (TodoItemHolder tlh in _incompleteItems[i])
				foreach (string tag in tlh.TD.Tags)
				{
					if (!_hashTags[i].Contains(tag))
						_hashTags[i].Add(tag);
					if (!_hashTags[0].Contains(tag))
						_hashTags[0].Add(tag);
				}
				_hashTags[i] = _hashTags[i].OrderBy(o => o).ToList();
			}
			for (int i = 1; i < tabItemList.Count; i++)
			{
				tabItemList[i].Header = _tabHash[i - 1] + " " + _incompleteItems[i].Count;
			}
			tabItemList[0].Header = "All " + _incompleteItems[0].Count;
		}
		private void SortToLists()
		{
			List<TodoItem> incompleteItems = new List<TodoItem>();
			foreach (TodoItem td in _masterList)
				incompleteItems.Add(td);
			
			foreach (TodoItem td in incompleteItems)
			{
				if (td.IsComplete)
				{
					td.Rank = 0;
					AddTodoToHistory(td);
					_masterList.Remove(td);
					continue;
				}
				bool sortedToTab = false;
				int index = 0;

				_incompleteItems[0].Add(new TodoItemHolder(td));
				TodoItemHolder tlh = new TodoItemHolder(td);

				foreach (string hash in _tabHash)
				{
					if (td.Tags.Contains(hash))
					{
						index = _tabHash.IndexOf(hash) + 1;
						_incompleteItems[index].Add(tlh);
						sortedToTab = true;
					}
				}
				if (sortedToTab)
					continue;

				_incompleteItems[1].Add(tlh);
			}
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

			SaveFile(path);
			
			_currentOpenFile = path;
			Title = WindowTitle;
			_isChanged = false;
			_currentOpenFile = path;
		}
		private void SaveFile(string path)
		{
			StreamWriter stream = new StreamWriter(File.Open(path, FileMode.Create));

			// TODO: Fix this for more Tabs
//			List<TodoListHolder> incompleteHolders = new List<TodoListHolder>();
//			List<TodoItem> incompleteItems = new List<TodoItem>();
//			for (int i = 0; i < _incompleteItems.Count; i++)
//				incompleteHolders.AddRange(_incompleteItems[i]);
//			foreach (TodoItem td in _masterList)
//				incompleteItems.Add(td);
			
			
			stream.WriteLine("====================================VERSION");
			stream.WriteLine(VERSION);

			stream.WriteLine("====================================TABS");
			foreach (TabItem ti in tabItemList)
			{
				stream.WriteLine(ti.Name);
			}
			
			stream.WriteLine("====================================FILESETTINGS");
			stream.WriteLine(_backupIncrement);
			stream.WriteLine(tabTest.SelectedIndex);
			
			stream.WriteLine("====================================TODO");
			foreach (TodoItem td in _masterList)
			{
				stream.WriteLine(td.ToString());
			}

			stream.WriteLine("====================================VCS");
			foreach (HistoryItem hi in _hHistoryItems)
			{
				stream.Write(hi.ToString());
			}
			stream.Close();
		}
		private void BackupSave()
		{
			string path = _recentFiles[0] + ".bak" + _backupIncrement;
			_backupIncrement++;
			_backupIncrement = _backupIncrement > 9 ? 0 : _backupIncrement;
			SaveFile(path);
		}
		private void Load(string path)
		{
			SortRecentFiles(path);
			SaveSettings();

			StreamReader stream = new StreamReader(File.Open(path, FileMode.Open));

			_incompleteItems.Clear();
			_masterList.Clear();
			_hashTags.Clear();
			_tabHash.Clear();
			tabItemList.Clear();
			_hHistoryItems.Clear();
			_hashShortcuts.Clear();

			float version;

			string line = stream.ReadLine();
			if (line.Contains("=====VERSION"))
			{
				line = stream.ReadLine();
				string[] versionPieces = line.Split(' ');
				version = Convert.ToSingle(versionPieces[0]);
			}
			else
				version = 2.0f;

			// Heres where versions are loaded
			if (version <= 2.0f)
				Load2_0SaveFile(stream, line);
			else if (version > 2.0f)
				Load2_1SaveFile(stream, line);
			
			stream.Close();

//			if(tabTest.SelectedIndex > -1)
//				TabControl.SelectedIndex = 1;
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
			// TODO: Add new history dialog here
			if (_hCurrentHistoryItem.HasBeenCopied)
			{
				DlgYesNo dlgYN = new DlgYesNo("New History", "Start a new History Item?");
					dlgYN.ShowDialog();
					if(dlgYN.Result)
						AddNewHistoryItem();
			}
//				if(MessageBox.Show("Start a new History Item?", "New History", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
//					AddNewHistoryItem();
		}
		private void Load2_0SaveFile(StreamReader stream, string line)
		{
			AddNewTodoTab("All");
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
//				TodoListHolder tlh = new TodoListHolder(td);
//				_incompleteItems[0].Add(tlh);
				_masterList.Add(td);
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
		}
		private void Load2_1SaveFile(StreamReader stream, string line)
		{
			
			
			while (line != null)
			{
				line = stream.ReadLine();
				if (line.Contains("=====TABS"))
					continue;
				if (line.Contains("=====FILESETTINGS"))
					break;
			
				AddNewTodoTab(line);
			}
			
//			line = stream.ReadLine();
//			if (line.Contains("=====FILESETTINGS"))
//				line = stream.ReadLine();
			_backupIncrement = Convert.ToInt16(stream.ReadLine());
//			tabTest.SelectedIndex = Convert.ToInt16(stream.ReadLine());
			
			while (line != null)
			{
				line = stream.ReadLine();
				if (line.Contains("=====VCS"))
					break;
				if (line.Contains("=====TODO"))
					continue;

				TodoItem td = new TodoItem(line);
				_masterList.Add(td);
			}

			List<string> history = new List<string>();
			while (line != null)
			{
				line = stream.ReadLine();
				if (line == "NewVCS")
				{
					history = new List<string>();
					continue;
				}
				if (line == "EndVCS")
				{
					_hHistoryItems.Add(new HistoryItem(history));
					continue;
				}
				history.Add(line);
			}
		}
	
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Settings //
		private void LoadSettings()
		{
			_recentFiles = new ObservableCollection<string>();
			float version;
			
			string filePath = basePath + "TDHistory.settings";
			if (!File.Exists(filePath))
				SaveSettings();
			
			StreamReader stream = new StreamReader(File.Open(filePath, FileMode.Open));
			string line = stream.ReadLine();
			if (line != "VERSION")
			{
				if (File.Exists(line))
				{
					version = 2.0f;
				}
				else
				{
					Top = 0;
					Left = 0;
					Height = 1080;
					Width = 1920;
					_recentFiles = new ObservableCollection<string>();
					
					DlgYesNo dlgYN = new DlgYesNo("Corrupted file", "Error with the settins file, create a new one?");
					DlgYesNo dlg;
					dlgYN.ShowDialog();
					if(dlgYN.Result)
//					if (MessageBox.Show("Error with the settins file, create a new one?", "Corrupted file", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
					{
						SaveSettings();
						dlg = new DlgYesNo("New settings file created");
						dlg.ShowDialog();
//						MessageBox.Show("New settings file created");
					}
					dlg = new DlgYesNo("New Todo file created");
					dlg.ShowDialog();
					return;
				}
			}
			else
			{
				line = stream.ReadLine();
				string[] versionPieces = line.Split(' ');
				version = Convert.ToSingle(versionPieces[0]);
			}

			// Heres where versions are loaded
			if (version <= 2.0)
				LoadV2_0Settings(stream, line);
			else if (version > 2.0)
				LoadV2_1Settings(stream, line);
			
			stream.Close();

			if (_recentFiles.Count == 0)
			{
				DlgYesNo dlg = new DlgYesNo("New file created");
				dlg.ShowDialog();
			}
//				MessageBox.Show("New file created");
		}
		private void LoadV2_0Settings(StreamReader stream, string line)
		{
			while (line != null)
			{
				if (line == "RECENTFILES" || line == "")
					continue;
				if (line == "WINDOWPOSITION")
					break;
					
				_recentFiles.Add(line);
				line = stream.ReadLine();
			}
			
			top = Convert.ToDouble(stream.ReadLine());
			left = Convert.ToDouble(stream.ReadLine());
			height = Convert.ToDouble(stream.ReadLine());
			width = Convert.ToDouble(stream.ReadLine());
		}
		private void LoadV2_1Settings(StreamReader stream, string line)
		{
			while (line != null)
			{
				line = stream.ReadLine();
				if (line == "RECENTFILES" || line == "")
					continue;
				if (line == "WINDOWPOSITION")
					break;
					
				_recentFiles.Add(line);
			}
			
			top = Convert.ToDouble(stream.ReadLine());
			left = Convert.ToDouble(stream.ReadLine());
			height = Convert.ToDouble(stream.ReadLine());
			width = Convert.ToDouble(stream.ReadLine());
			line = stream.ReadLine();
			_pomoWorkTime = Convert.ToInt16(stream.ReadLine());
			_pomoBreakTime = Convert.ToInt16(stream.ReadLine());
		}
		private void SaveSettings()
		{
			string filePath = basePath + "TDHistory.settings";
			StreamWriter stream = new StreamWriter(File.Open(filePath, FileMode.Create));

			stream.WriteLine("VERSION");
			stream.WriteLine(VERSION);
			
			stream.WriteLine("RECENTFILES");
			foreach (string s in _recentFiles)
			{
				if (s == "")
					continue;
				stream.WriteLine(s);
			}
			
			stream.WriteLine("WINDOWPOSITION");
			stream.WriteLine(Top);
			stream.WriteLine(Left);
			stream.WriteLine(Height);
			stream.WriteLine(Width);
			stream.WriteLine("POMOTIMERSETTINGS");
			stream.WriteLine(_pomoWorkTime);
			stream.WriteLine(_pomoBreakTime);
			
			
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
