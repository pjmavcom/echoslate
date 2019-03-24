using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListBox = System.Windows.Controls.ListBox;
using MenuItem = System.Windows.Controls.MenuItem;


namespace TODOList
{
	public partial class MainWindow : INotifyPropertyChanged
	{
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
		public const string DATE = "yyyMMdd";
		public const string TIME = "HHmmss";
		public const string VERSION = "3.03";
		private const float VERSIONINCREMENT = 0.01f;

		private readonly List<TabItem> _tabList;
		private readonly List<TodoItem> _masterList;
		private readonly List<List<TodoItemHolder>> _incompleteItems;
		private readonly List<List<string>> _hashTags;
		private readonly List<string> _tabHash;
		private static Dictionary<string, string> _hashShortcuts;
		List<string> _prevHashTagList = new List<string>();
		

		// Sorting
		private bool _reverseSort;
		private string _currentSort = "rank";
		private int _currentHashTagSortIndex = -1;
		private bool _didHashChange;
		private string _hashToSortBy = "";
		private bool _hashSortSelected;
		
		private int _currentSeverity;

		// HISTORY TAB ITEMS
		private readonly List<HistoryItem> _historyItems;
		private HistoryItem _currentHistoryItem;
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
		private bool _doBackup;
		private bool _autoBackup;
		private TimeSpan _backupTime;
		private TimeSpan _timeUntilBackup;
		private int _backupIncrement;
		private string _historyLogPath;
		private float _currentProjectVersion;
		private float _projectVersionIncrement;

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
		private int _pomoTimeLeft;
		
		// CONTROLS
		private ComboBox cbHashTags;
		private ListBox lbIncompleteItems;


		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		public int PomoTimeLeft
		{
			get => _pomoTimeLeft;
			set
			{
				_pomoTimeLeft = value;
				OnPropertyChanged();
			}
		}
		public ObservableCollection<string> RecentFiles
		{
			get => _recentFiles;
			set => _recentFiles = value;
		}
		private List<TodoItemHolder> IncompleteItems => _incompleteItems[_todoTabs.SelectedIndex];
		private List<string> HashTags => _hashTags[_todoTabs.SelectedIndex];
		private string TabNames => _todoTabs.SelectedIndex == -1
			? _tabList[0].Name
			: _tabList[_todoTabs.SelectedIndex].Name;
		public List<HistoryItem> HistoryItems => _historyItems;
		private string WindowTitle => "EthereaListVCSNotes v" + VERSION + " " + _currentOpenFile;

		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public  MainWindow()
		{
			left = 0;
			InitializeComponent();
			Closing += Window_Closed;
			DataContext = this;
#if DEBUG
			mnuMain.Background = Brushes.Red;
#endif
			
			LoadSettings();

			Top = top;
			Left = left;
			Height = height;
			Width = width;
			_backupTime = new TimeSpan(0, 5, 0);
			_backupIncrement = 0;
 		
			var timer = new DispatcherTimer();
			timer.Tick += Timer_Tick;
			timer.Interval = new TimeSpan(TimeSpan.TicksPerSecond);
			timer.Start();

			_tabList = new List<TabItem>();
			_masterList = new List<TodoItem>();
			_incompleteItems = new List<List<TodoItemHolder>>();
			_hashTags = new List<List<string>>();
			_tabHash = new List<string>();
			_hashShortcuts = new Dictionary<string, string>();
			_historyItems = new List<HistoryItem>();
			_currentHistoryItem = new HistoryItem("", "");

			_todoTabs.ItemsSource = _tabList;
			_todoTabs.Items.Refresh();

			lbHistory.SelectedIndex = 0;
			_currentHistoryItemIndex = 0;
			lbCompletedTodos.SelectedIndex = 0;
			
			lblPomoWork.Content = _pomoWorkTime.ToString();
			lblPomoBreak.Content = _pomoBreakTime.ToString();
			
			if (_recentFiles.Count > 0)
				Load(_recentFiles[0]);
			else
				CreateNewTabs();
			_timeUntilBackup = _backupTime;
			LoadHistory();
		}

		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS //
		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			_handle = new WindowInteropHelper(this).Handle;
			source = HwndSource.FromHwnd(_handle);
			source?.AddHook(HwndHook);

#if !DEBUG
			_autoSave = false;
			_autoBackup = false;
#else
			_autoSave = true;
			_autoBackup = true;
#endif

			GlobalHotkeysToggle();
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
		private void Window_Closed(object sender, CancelEventArgs e)
		{
			UnregisterHotKey(_handle, HOTKEY_ID);
			SaveSettings();
			if (!_isChanged)
				return;

			DlgYesNo dlg = new DlgYesNo("Close", "Maybe save first?");
			dlg.ShowDialog();
			if(dlg.Result)
				Save(_currentOpenFile);
		}
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		private void GlobalHotkeysToggle()
		{
			if (_globalHotkeys)
				RegisterHotKey(_handle, HOTKEY_ID, MOD_WIN, 0x73);
			else
				UnregisterHotKey(_handle, HOTKEY_ID);
		}
		private void tabSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Dispatcher.BeginInvoke(new Action(updateHandler));
		}
		private void updateHandler()
		{
			ContentPresenter myContentPresenter = _todoTabs.Template.FindName("PART_SelectedContentHost", _todoTabs) as ContentPresenter;
			{
				if (myContentPresenter != null)
				{
					myContentPresenter.ApplyTemplate();
					lbIncompleteItems = myContentPresenter.ContentTemplate.FindName("lbIncompleteItems", myContentPresenter) as ListBox;
					if (lbIncompleteItems != null)
					{
						lbIncompleteItems.ItemsSource = IncompleteItems;
						lbIncompleteItems.Items.Refresh();
					}
					cbHashTags = myContentPresenter.ContentTemplate.FindName("cbHashTags", myContentPresenter) as ComboBox;
				}
				if (cbHashTags == null)
					return;
				cbHashTags.ItemsSource = HashTags;
				cbHashTags.Items.Refresh();
			}
		}
		private void CreateNewTabs()
		{
			AddNewTodoTab("Other");
			AddNewTodoTab("Bug");
			AddNewTodoTab("Feature");
		}
		private void AddNewTodoTab(string name, bool doSave = true)
		{
			TabItem ti = new TabItem();
			name = UpperFirstLetter(name);
			ti.Header = name;
			ti.Name = name;
			foreach (TabItem existingTabItem in _tabList)
			{
				if (existingTabItem.Name != name)
					continue;
				DlgYesNo dlg = new DlgYesNo("Already have a tab called " + name);
				dlg.ShowDialog();
				return;
			}

			if (name != "All")
			{
				string hash = "#" + name.ToUpper();
				string hashShortcut = "";
				hashShortcut = GetHashShortcut(name, hashShortcut);
				_hashShortcuts.Add(hashShortcut, name);
				_tabHash.Add(hash);
			}
			
			_incompleteItems.Add(new List<TodoItemHolder>());
			_hashTags.Add(new List<string>());
			_tabList.Add(ti);
			RefreshTodo();
			_todoTabs.Items.Refresh();
			if (doSave)
				AutoSave();
		}
		private string GetHashShortcut(string name, string shortcut)
		{
			string hashShortcut = shortcut + name[0].ToString().ToLower();
			string leftover = "";
			for (int i = 1; i < name.Length; i++)
				leftover += name[i];

			if (_hashShortcuts.ContainsKey(hashShortcut))
				return GetHashShortcut(leftover, hashShortcut);
			return hashShortcut;
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
		private void Timer_Tick(object sender, EventArgs e)
		{
			_timeUntilBackup = _timeUntilBackup.Subtract(new TimeSpan(0, 0, 1));
			if (_timeUntilBackup <= TimeSpan.Zero)
			{
				_timeUntilBackup = _backupTime;
				BackupSave();
			}

			foreach (TodoItem td in _masterList)
				if (td.IsTimerOn)
					td.TimeTaken = td.TimeTaken.AddSeconds(1);
			foreach (var list in _incompleteItems)
				foreach (TodoItemHolder tlh in list)
					if (tlh.TD.IsTimerOn)
						tlh.TimeTaken = tlh.TD.TimeTaken;

			lblPomo.Content = $"{_pomoTimer.Ticks / TimeSpan.TicksPerMinute:00}:{_pomoTimer.Second:00}";
			if (_isPomoTimerOn)
			{
				pbPomo.Background = Brushes.Maroon;
				_pomoTimer = _pomoTimer.AddSeconds(1);
				
				if(_isPomoWorkTimerOn)
				{
					long ticks = _pomoWorkTime * TimeSpan.TicksPerMinute;
					PomoTimeLeft = (int)((float) _pomoTimer.Ticks / ticks * 100);
					pbPomo.Background = Brushes.DarkGreen;
					if (_pomoTimer.Ticks < ticks)
						return;
					_isPomoWorkTimerOn = false;
					_pomoTimer=DateTime.MinValue;
				}
				else
				{
					long ticks = _pomoBreakTime * TimeSpan.TicksPerMinute;
					PomoTimeLeft = (int)((float) (ticks - _pomoTimer.Ticks) / ticks * 100);
					if (_pomoTimer.Ticks < ticks)
						return;
					_isPomoWorkTimerOn = true;
					_pomoTimer=DateTime.MinValue;
				}
			}
			else
			{
				pbPomo.Background = Brushes.Transparent;
				lblPomo.Background = Brushes.Transparent;
			}
		}
		private void RefreshLog_OnClick(object sender, EventArgs e)
		{
			LoadHistory();
		}
		private void LoadHistory()
		{
			string[] pathPieces = _currentOpenFile.Split('\\');
			string path = "";
			for (int i = 0; i < pathPieces.Length - 1; i++)
				path += pathPieces[i] + "\\";
			string gitPath = FindGitDirectory(path);
			_historyLogPath = path + "log.txt";
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				CreateNoWindow = false,
				UseShellExecute = false,
				FileName = "cmd.exe",
				WindowStyle = ProcessWindowStyle.Hidden
			};
			string args = "/c git --git-dir " + gitPath + "\\.git log > \"" + _historyLogPath + "\"";
			startInfo.Arguments = args;
			Process p = new Process();
			p.StartInfo = startInfo;
			p.Start();
			p.WaitForExit();
			
			List<string> log = new List<string>();
			StreamReader stream = new StreamReader(File.Open(_historyLogPath, FileMode.OpenOrCreate));
			string line = stream.ReadLine();
			while (line != null)
			{
				log.Add(line);
				line = stream.ReadLine();
			}
			lbHistoryLog.ItemsSource = log;
			lbHistoryLog.Items.Refresh();
		}
		private string FindGitDirectory(string dir)
		{
			if (dir == Directory.GetDirectoryRoot(dir))
				return null;
			List<string> dirs = Directory.GetDirectories(dir).ToList();
			foreach(string s in dirs)
				if (s.Contains(".git"))
					return dir;
			
			return FindGitDirectory(Directory.GetParent(dir).FullName);
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Hotkeys //
		private void hkSwitchTab(object sender, ExecutedRoutedEventArgs e)
		{
			int index = _todoTabs.SelectedIndex;
			if ((string) e.Parameter == "right")
			{
				if (TabControl.SelectedIndex == 0)
				{
					TabControl.SelectedIndex = 1;
					return;
				}
				index++;
				if (index >= _todoTabs.Items.Count)
					index = _todoTabs.Items.Count - 1;
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
			_todoTabs.SelectedIndex = index;
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
				DlgTodoItemEditor tdc = new DlgTodoItemEditor(tlh.TD, TabNames);
				tdc.ShowDialog();
				if (tdc.Result)
					AddItemToMasterList(tdc.ResultTD);
			}
			RefreshTodo();
		}
		private void hkAdd(object sender, ExecutedRoutedEventArgs e)
		{
			if (tabHistory.IsSelected)
				return;
			Add_OnClick(sender, e);
		}
		private void hkEdit(object sender, EventArgs e)
		{
			if (tbNewTodo.IsFocused)
			{
				Add_OnClick(sender, e);
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
				list = _currentHistoryItem.CompletedTodos;
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
				Severity = _currentSeverity,
				IsComplete = true,
				Rank = {[TabNames] = IncompleteItems.Count}
			};

			AddItemToMasterList(newtd);
			AutoSave();
			RefreshTodo();
			tbNewTodo.Clear();
		}
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// MenuCommands //
		private void mnuEditTabs_OnClick(object sender, EventArgs e)
		{
			List<TabItem> list = _tabList.ToList();
			DlgEditTabs rt = new DlgEditTabs(list);
			rt.ShowDialog();
			if (!rt.Result)
				return;
			_tabList.Clear();
			_hashShortcuts.Clear();
			_tabHash.Clear();
			_hashTags.Clear();
			_incompleteItems.Clear();
			foreach (string s in rt.ResultList)
				AddNewTodoTab(s);

			foreach (TodoItem td in _masterList)
				CleanTodoHashRanks(td);
		}
		private void mnuOptions_OnClick(object sender, EventArgs e)
		{
			DlgOptions options = new DlgOptions(_autoSave, _globalHotkeys, _autoBackup, _backupTime, _currentProjectVersion, _projectVersionIncrement);
			options.ShowDialog();
			if (!options.Result)
				return;
			_autoSave = options.AutoSave;
			_globalHotkeys = options.GlobalHotkeys;
			_autoBackup = options.AutoBackup;
			_backupTime = options.BackupTime;
			_currentProjectVersion = options.CurrentProjectVersion;
			_projectVersionIncrement = options.ProjectVersionIncrement;
			
			GlobalHotkeysToggle();
			AutoSave();
		}
		private void mnuNew_OnClick(object sender, EventArgs e)
		{
			DlgYesNo dlg = new DlgYesNo("New file", "Are you sure?");
			dlg.ShowDialog();
			if (!dlg.Result)
				return;
			_historyItems.Clear();
			_incompleteItems.Clear();
			CreateNewTabs();

			_currentHistoryItem = new HistoryItem("", "");
			RefreshHistory();
			RefreshTodo();

			_currentOpenFile = "";
			Title = WindowTitle;
			AutoSave();
			SaveAs();
		}
		private void mnuRemoveFile_OnClick(object sender, RoutedEventArgs e)
		{
			if (_recentFilesIndex < 0)
				return;
			_recentFiles.RemoveAt(_recentFilesIndex);
			mnuRecentLoads.Items.Refresh();
		}
		private void mnuRecentLoads_OnRMBUp(object sender, MouseButtonEventArgs e)
		{
			_recentFilesIndex = -1;
			var mi = e.OriginalSource as TextBlock;
			if (mi == null)
				return;
			string path = (string) mi.DataContext;
			_recentFilesIndex = mnuRecentLoads.Items.IndexOf(path);
		}
		private void mnuResetTimer_OnClick(object sender, EventArgs e)
		{
			int index = lbIncompleteItems.SelectedIndex;
			if (index < 0)
				return;
			TodoItemHolder tlh = IncompleteItems[index];
			tlh.TimeTaken = new DateTime();
			tlh.TD.IsTimerOn = false;
			lbIncompleteItems.Items.Refresh();
		}
		private void mnuResetHistoryCopied_OnClick(object sender, EventArgs e)
		{
			int index = lbHistory.SelectedIndex;
			_historyItems[index].ResetCopied();
		}
		private void mnuQuit_OnClick(object sender, EventArgs e)
		{
			Close();
		}
		private void mnuSaveAs_OnClick(object sender, EventArgs e)
		{
			SaveAs();
		}
		private void mnuSave_OnClick(object sender, EventArgs e)
		{
			if (_currentOpenFile == null)
			{
				DlgYesNo dlg = new DlgYesNo("No current file");
				dlg.ShowDialog();
				return;
			}
			Save(_currentOpenFile);
		}
		private void mnuLoadFiles_OnClick(object sender, RoutedEventArgs e)
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
					Save(_currentOpenFile);
			}
			
			Load(path);
		}
		private void mnuLoad_OnClick(object sender, EventArgs e)
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
					Save(_currentOpenFile);
			}

			Load(ofd.FileName);
		}
		private void mnuDelete_OnClick(object sender, EventArgs e)
		{
			TodoItem td = IncompleteItems[lbIncompleteItems.SelectedIndex].TD;
			RemoveItemFromMasterList(td);
			IncompleteItems.RemoveAt(lbIncompleteItems.SelectedIndex);
			
			AutoSave();
			RefreshTodo();
		}
		private void mnuEditTodo_OnClick(object sender, EventArgs e)
		{
			List<TodoItem> list = new List<TodoItem>();
			foreach (TodoItemHolder tlh in IncompleteItems)
					list.Add(tlh.TD);
			if(lbIncompleteItems.SelectedItems.Count > 1)
				MultiEditItems(lbIncompleteItems);
			else
				EditItem(lbIncompleteItems, list);
		}
		private void mnuEditHistoryTodo_OnClick(object sender, EventArgs e)
		{
			if(lbCompletedTodos.IsMouseOver)
				EditItem(lbCompletedTodos, _currentHistoryItem.CompletedTodos);
			else if(lbCompletedTodosFeatures.IsMouseOver)
				EditItem(lbCompletedTodosFeatures, _currentHistoryItem.CompletedTodosFeatures);
			else if(lbCompletedTodosBugs.IsMouseOver)
				EditItem(lbCompletedTodosBugs, _currentHistoryItem.CompletedTodosBugs);
			
			RefreshHistory();
		}
		private void mnuHelp_OnClick(object sender, EventArgs e)
		{
			DlgHelp dlgH = new DlgHelp();
			dlgH.ShowDialog();
		}
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// HISTORY TAB //
		private void Title_OnTextChange(object sender, EventArgs e)
		{
			_currentHistoryItem.Title = tbHTitle.Text;
			lbHistory.Items.Refresh();
		}
		private void Notes_OnTextChange(object sender, EventArgs e)
		{
			_currentHistoryItem.Notes = tbHNotes.Text;
			lbHistory.Items.Refresh();
		}
		private void HistoryListBox_OnKeyDown(object sender, KeyEventArgs e)
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
			}
			
			if (_currentHistoryItemIndex >= _historyItems.Count)
				_currentHistoryItemIndex = 0;
			if (_currentHistoryItemIndex < 0)
				_currentHistoryItemIndex = _historyItems.Count - 1;
			if (_historyItems.Count == 0)
			{
				_currentHistoryItem = new HistoryItem("", "");
				return;
			}
			if (prevIndex == _currentHistoryItemIndex)
				return;

			_currentHistoryItem = lbHistory.Items[_currentHistoryItemIndex] as HistoryItem;
			if (_currentHistoryItem != null)
				lblHTotalTime.Content = _currentHistoryItem.TotalTime;
			lbHistory.SelectedIndex = _currentHistoryItemIndex;
			RefreshHistory();
		}
		private void HistoryListBox_OnMouseDown(object sender, EventArgs e)
		{
			_didMouseSelect = true;
		}
		private void HistoryListBox_OnSelectionChange(object sender, SelectionChangedEventArgs e)
		{
			if(_didMouseSelect)
				_currentHistoryItemIndex = lbHistory.SelectedIndex;
			
			_currentHistoryItem = lbHistory.Items[_currentHistoryItemIndex] as HistoryItem;
			if (_currentHistoryItem != null)
				lblHTotalTime.Content = _currentHistoryItem.TotalTime;
			
			RefreshHistory();
			_didMouseSelect = false;
		}
		private void DeleteTodo_OnClick(object sender, EventArgs e)
		{
			Button b = sender as Button;
			TodoItem td = b?.DataContext as TodoItem;
			DlgYesNo dlgYN = new DlgYesNo("Delete", "Are you sure you want to delete this Todo?");
			dlgYN.ShowDialog();
			if (!dlgYN.Result)
				return;

			if (td != null)
			{
				td.IsComplete = false;
				AddItemToMasterList(td);
				RefreshTodo();
				if(_currentHistoryItem.CompletedTodos.Contains(td))
					_currentHistoryItem.CompletedTodos.Remove(td);
				else if(_currentHistoryItem.CompletedTodosBugs.Contains(td))
					_currentHistoryItem.CompletedTodosBugs.Remove(td);
				else if(_currentHistoryItem.CompletedTodosFeatures.Contains(td))
					_currentHistoryItem.CompletedTodosFeatures.Remove(td);
				AutoSave();
			}
			RefreshHistory();
		}
		private void DeleteHistory_OnClick(object sender, EventArgs e)
		{
			if (_historyItems.Count == 0)
				return;
			DlgYesNo dlgYN = new DlgYesNo("Delete", "Are you sure you want to delete this History Item?");
			dlgYN.ShowDialog();
			if (!dlgYN.Result)
				return;
			
			_historyItems.Remove(_currentHistoryItem);

			_currentHistoryItem = _historyItems.Count > 0 ? _historyItems[0] : new HistoryItem("", "");
			AutoSave();
			RefreshHistory();
		}
		private void NewHistory_OnClick(object sender, EventArgs e)
		{
			AddNewHistoryItem();
		}
		private void CopyHistory_OnClick(object sender, EventArgs e)
		{
			Button b = sender as Button;
			HistoryItem hi = (HistoryItem) b?.DataContext;
			if (hi == null)
				return;
			int totalTime = 0;
			foreach (HistoryItem hist in _historyItems)
			{
				totalTime += Convert.ToInt32(hist.TotalTime);
			}
			string time = $"{totalTime / 60:00} : {totalTime % 60:00}";
			Clipboard.SetText(hi.ToClipboard(time));
			hi.SetCopied();
			if (lbHistory.Items.IndexOf(hi) != 0)
				return;
			DlgYesNo dlgYN = new DlgYesNo("New History", "Add a new History Item?");
			dlgYN.ShowDialog();
			if (dlgYN.Result)
				AddNewHistoryItem();
		}
		private void AddTodoToHistory(TodoItem td)
		{
			if (_currentHistoryItem.DateAdded == "")
				AddNewHistoryItem();
			RefreshHistory();
			_currentHistoryItem = _historyItems[0];
			_currentHistoryItem.AddCompletedTodo(td);
			RefreshHistory();
			AutoSave();
		}
		private void AddNewHistoryItem()
		{
			DlgAddNewHistory dlgANH = new DlgAddNewHistory(_currentProjectVersion, _projectVersionIncrement);
			dlgANH.ShowDialog();
			if (!dlgANH.Result)
				return;

			_currentProjectVersion += _projectVersionIncrement;

			_currentHistoryItem = new HistoryItem(DateTime.Now)
			{
				Title = dlgANH.ResultTitle
			};
			_historyItems.Add(_currentHistoryItem);
			AutoSave();
			RefreshHistory();
		}
		private void RefreshHistory()
		{
			List<HistoryItem> temp = _historyItems.OrderByDescending(o => o.DateTimeAdded).ToList();
			_historyItems.Clear();
			foreach (HistoryItem hi in temp)
				_historyItems.Add(hi);

			if (_historyItems.Count == 0)
				_currentHistoryItem = new HistoryItem("", "");

			if (_historyItems.Count > 0 && _currentHistoryItem.DateAdded == "")
				lbHistory.SelectedIndex = 0;

			SortCompletedItems(_currentHistoryItem); 
			
			tbHNotes.Text = _currentHistoryItem.Notes;
			tbHTitle.Text = _currentHistoryItem.Title;
			lbCompletedTodos.ItemsSource = _currentHistoryItem.CompletedTodos;
			lbCompletedTodos.Items.Refresh();
			lbCompletedTodosBugs.ItemsSource = _currentHistoryItem.CompletedTodosBugs;
			lbCompletedTodosBugs.Items.Refresh();
			lbCompletedTodosFeatures.ItemsSource = _currentHistoryItem.CompletedTodosFeatures;
			lbCompletedTodosFeatures.Items.Refresh();
			lblHTotalTime.Content = _currentHistoryItem.TotalTime;

			int index = lbHistory.SelectedIndex;
			lbHistory.Items.Refresh();
			lbHistory.SelectedIndex = index;
		}
		private void SortCompletedItems(HistoryItem hi)
		{
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
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// NEW TO DO //
		private void SeverityComboBox_OnSelectionChange(object sender, EventArgs e)
		{
			if (!(sender is ComboBox cb))
				return;
			int index = cb.SelectedIndex;
			_currentSeverity = index;
		}
		private void SeverityComboBox_OnIsLoaded(object sender, EventArgs e)
		{
			if (sender is ComboBox cb)
				cb.SelectedIndex = _currentSeverity;
		}
		private void Add_OnClick(object sender, EventArgs e)
		{
			TodoItem td = new TodoItem() {Todo = tbNewTodo.Text, Severity = _currentSeverity};
			ExpandHashTags(td);
			td.Rank[TabNames] = -1;
			if (td.Severity == 3)
				td.Rank[TabNames] = 0;

			AddItemToMasterList(td);
			AutoSave();
			RefreshTodo();
			tbNewTodo.Clear();
		}
		private void RankAdjust_OnClick(object sender, EventArgs e)
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
					// TODO: Fix this too
					int newRank = IncompleteItems[index - 1].TD.Rank[TabNames];
					if (tlh != null)
					{
						IncompleteItems[index - 1].TD.Rank[TabNames] = tlh.Rank;
						tlh.TD.Rank[TabNames] = newRank;
						AutoSave();
					}
				}
				else if ((string) b.CommandParameter == "down")
				{
					if (index >= IncompleteItems.Count - 1)
						return;
					// TODO: And this 
					int newRank = IncompleteItems[index + 1].TD.Rank[TabNames];
					if (tlh != null)
					{
						IncompleteItems[index + 1].TD.Rank[TabNames] = tlh.Rank;
						tlh.TD.Rank[TabNames] = newRank;
						AutoSave();
					}
				}
			}
			RefreshTodo();
		}
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// POMO STUFF //
		private void PomoTimerStart_OnClick(object sender, EventArgs e)
		{
			_isPomoTimerOn = true;
		}
		private void PomoTimerPause_OnClick(object sender, EventArgs e)
		{
			_isPomoTimerOn = false;
		}
		private void PomoTimerStop_OnClick(object sender, EventArgs e)
		{
			_isPomoTimerOn = false;
			_pomoTimer = DateTime.MinValue;
			PomoTimeLeft = 0;
		}
		private void PomoWorkInc_OnClick(object sender, EventArgs e)
		{
			int value = Convert.ToInt16((string) (sender as Button)?.CommandParameter);
			_pomoWorkTime += value;
			lblPomoWork.Content = _pomoWorkTime.ToString();
		}
		private void PomoWorkDec_OnClick(object sender, EventArgs e)
		{
			int value = Convert.ToInt16((string) (sender as Button)?.CommandParameter);
			_pomoWorkTime -= value;
			if (_pomoWorkTime <= 0)
				_pomoWorkTime = value;
			lblPomoWork.Content = _pomoWorkTime.ToString();
		}
		private void PomoBreakInc_OnClick(object sender, EventArgs e)
		{
			int value = Convert.ToInt16((string) (sender as Button)?.CommandParameter);
			_pomoBreakTime += value;
			lblPomoBreak.Content = _pomoBreakTime.ToString();
		}
		private void PomoBreakDec_OnClick(object sender, EventArgs e)
		{
			int value = Convert.ToInt16((string) (sender as Button)?.CommandParameter);
			_pomoBreakTime -= value;
			if (_pomoBreakTime <= 0)
				_pomoBreakTime = value;
			lblPomoBreak.Content = _pomoBreakTime.ToString();
		}
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// TO DOs //
		private void AddItemToMasterList(TodoItem td)
		{
			if (MasterListContains(td) >= 0)
				return;

			CleanTodoHashRanks(td);

			_masterList.Add(td);
		}
		private void RemoveItemFromMasterList(TodoItem td)
		{
			int index = MasterListContains(td);
			if (index == -1)
				return;
			
			_masterList.RemoveAt(index);
		}
		private int MasterListContains(TodoItem td)
		{
			if (_masterList.Contains(td))
				return _masterList.IndexOf(td);
			return -1;
		}
		private void CleanTodoHashRanks(TodoItem td)
		{
			List<string> tabNames = new List<string>();
			foreach (TabItem ti in _tabList)
				tabNames.Add(ti.Name);
			
			List<string> remove = new List<string>();
			foreach (KeyValuePair<string, int> kvp in td.Rank)
				if (!tabNames.Contains(kvp.Key))
					remove.Add(kvp.Key);
			foreach (string hash in remove)
				td.Rank.Remove(hash);
			
			foreach (string name in tabNames)
				if (!td.Rank.ContainsKey(name))
					td.Rank.Add(name, -1);
		}
		private void EditItem(ListBox lb, List<TodoItem> list)
		{
			int index = lb.SelectedIndex;
			if (index < 0)
				return;
			TodoItem td = list[index];
			DlgTodoItemEditor tdie = new DlgTodoItemEditor(td, TabNames);

			tdie.ShowDialog();
			if (tdie.Result)
			{
				RemoveItemFromMasterList(td);
				if(_currentHistoryItem.CompletedTodos.Contains(td))
					_currentHistoryItem.CompletedTodos.Remove(td);
				if(_currentHistoryItem.CompletedTodosBugs.Contains(td))
					_currentHistoryItem.CompletedTodosBugs.Remove(td);
				if(_currentHistoryItem.CompletedTodosFeatures.Contains(td))
					_currentHistoryItem.CompletedTodosFeatures.Remove(td);
				AddItemToMasterList(tdie.ResultTD);
				AutoSave();
			}

			RefreshTodo();
			RefreshHistory();
		}
		private void MultiEditItems(ListBox lb)
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
				foreach (string tag in commonTagsTemp)
					if (!tih.TD.Tags.Contains(tag))
						commonTags.Remove(tag);

			if (firstTd == null)
				return;
			
			DlgTodoMultiItemEditor tmie = new DlgTodoMultiItemEditor(firstTd.TD, TabNames, commonTags);
			tmie.ShowDialog();
			if (!tmie.Result)
				return;
			
			List<string> tagsToRemove = new List<string>();
		
			foreach (string tag in commonTags)
				if (!tmie.ResultTags.Contains(tag))
					tagsToRemove.Add(tag);

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
					tlh.TD.Rank = tmie.ResultTD.Rank;
				if(tmie.ChangeSev)
					tlh.TD.Severity = tmie.ResultTD.Severity;
				if (tmie.ResultIsComplete && tmie.ChangeComplete)
					tlh.TD.IsComplete = true;
				if (!tmie.ChangeTodo)
					continue;
				tlh.TD.Todo += Environment.NewLine + tmie.ResultTD.Todo;
				foreach (string tag in tmie.ResultTD.Tags)
					if(!tlh.TD.Tags.Contains(tag))
						tlh.TD.Tags.Add(tag);
			}
			RefreshTodo();
		}
		private static void ExpandHashTags(TodoItem td)
		{
			string tempTodo = ExpandHashTagsInString(td.Todo);
			string tempTags = ExpandHashTagsInList(td.Tags);
			td.Tags = new List<string>();
			
			td.Todo = tempTags.Trim() + " " + tempTodo.Trim();
		}
		public static string ExpandHashTagsInString(string todo)
		{
			string result = "";
			string[] pieces = todo.Split(' ');

			List<string> list = new List<string>();
			foreach (string piece in pieces)
			{
				string s = piece;
				if (s.Contains('#'))
				{
					string t = s.ToUpper();
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
					s = s.ToLower();
				}
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
		private static string ExpandHashTagsInList(List<string> tags)
		{
			string result = "";
			foreach (string s in tags)
				result += s + " ";

			result = ExpandHashTagsInString(result);
			return result;
		}
		private void TimeTakenTimer_OnClick(object sender, EventArgs e)
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

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Sorting //
		private void Hashtags_OnSelectionChange(object sender, EventArgs e)
		{
			if (HashTags.Count == 0)
				return;
			_hashToSortBy = HashTags[0];
			if (cbHashTags.SelectedItem != null)
				_hashToSortBy = cbHashTags.SelectedItem.ToString();
			_hashSortSelected = true;
			_currentSort = "hash";
			RefreshTodo();
		}
		private void Sort_OnClick(object sender, EventArgs e)
		{
			Button b = sender as Button;
			if (_currentSort != (string) b?.CommandParameter)
			{
				_reverseSort = false;
				_currentSort = (string) b?.CommandParameter;
			}
			
			if ((string) b?.CommandParameter == "hash")
			{
				if (HashTags.Count == 0)
					return;
				_currentHashTagSortIndex++;
				if (_currentHashTagSortIndex >= HashTags.Count)
					_currentHashTagSortIndex = 0;
			}

			_reverseSort = !_reverseSort;
			RefreshTodo();
		}
		private List<TodoItemHolder> SortByHashTag(List<TodoItemHolder> list)
		{
			if (_didHashChange)
				_currentHashTagSortIndex = 0;

			List<TodoItemHolder> incompleteItems = new List<TodoItemHolder>();
			List<string> sortedHashTags = new List<string>();
			
			if (HashTags.Count == 0)
				return list;
			
			if (_hashSortSelected)
			{
				_currentHashTagSortIndex = 0;
				foreach (string s in HashTags)
				{
					if (s.Equals(_hashToSortBy))
						break;
					_currentHashTagSortIndex++;
				}
			}

			for (int i = 0 + _currentHashTagSortIndex; i < HashTags.Count; i++)
				sortedHashTags.Add(HashTags[i]);
			for (int i = 0; i < _currentHashTagSortIndex; i++)
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
				if (_incompleteItems[i].Count <= 0)
					continue;
				
				string hash = _tabList[i].Name;

				if (!_incompleteItems[i][0].TD.Rank.ContainsKey(hash))
					_incompleteItems[i][0].TD.Rank.Add(hash, -1);
	
				
				_incompleteItems[i] = _incompleteItems[i].OrderBy(o => o.TD.Rank[hash]).ToList();
				for (int rank = 0; rank < _incompleteItems[i].Count; rank++)
				{
					_incompleteItems[i][rank].TD.Rank[hash] = rank + 1;
					_incompleteItems[i][rank].Rank = _incompleteItems[i][rank].TD.Rank[hash];
				}
			}
		}
		private void CheckForHashTagListChanges()
		{

			_didHashChange = false;
			if (_hashTags[0].Count != _prevHashTagList.Count)
				_didHashChange = true;
			else
				for (int i = 0; i < _hashTags[0].Count; i++)
					if (_hashTags[0][i] != _prevHashTagList[i])
						_didHashChange = true;
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
			for (int i = 1; i < _tabList.Count; i++)
			{
				_tabList[i].Header = _tabHash[i - 1] + " " + _incompleteItems[i].Count;
			}
			_tabList[0].Header = "All " + _incompleteItems[0].Count;
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
					AddTodoToHistory(td);
					RemoveItemFromMasterList(td);
					continue;
				}
				bool sortedToTab = false;

				_incompleteItems[0].Add(new TodoItemHolder(td));
				TodoItemHolder tlh = new TodoItemHolder(td);

				foreach (string hash in _tabHash)
				{
					if (!td.Tags.Contains(hash))
						continue;
					int index = _tabHash.IndexOf(hash) + 1;
					_incompleteItems[index].Add(tlh);
					sortedToTab = true;
				}
				if (sortedToTab)
					continue;
				if(_incompleteItems.Count > 1)
					_incompleteItems[1].Add(tlh);
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
		
			int tabIndex = _todoTabs.SelectedIndex;
			if (tabIndex < 0 || tabIndex >= _tabList.Count)
				return;
			
			switch (_currentSort)
			{
				case "sev":
					_incompleteItems[tabIndex] = _reverseSort
						? _incompleteItems[tabIndex].OrderByDescending(o => o.Severity).ToList()
						: _incompleteItems[tabIndex].OrderBy(o => o.Severity).ToList();
					break;
				case "date":
					_incompleteItems[tabIndex] = _reverseSort
						? _incompleteItems[tabIndex].OrderByDescending(o => o.TimeStarted).ToList()
						: _incompleteItems[tabIndex].OrderBy(o => o.TimeStarted).ToList();
					_incompleteItems[tabIndex] = _reverseSort
						? _incompleteItems[tabIndex].OrderByDescending(o => o.DateStarted).ToList()
						: _incompleteItems[tabIndex].OrderBy(o => o.DateStarted).ToList();
					break;
				case "hash":
					_incompleteItems[tabIndex] = SortByHashTag(_incompleteItems[tabIndex]);
					break;
				case "rank":
					_incompleteItems[tabIndex] = _reverseSort
						? _incompleteItems[tabIndex].OrderByDescending(o => o.TD.Rank[TabNames]).ToList()
						: _incompleteItems[tabIndex].OrderBy(o => o.TD.Rank[TabNames]).ToList();
					break;
				case "active":
					_incompleteItems[tabIndex] = _reverseSort
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
		private void Load(string path)
		{
			SortRecentFiles(path);
			SaveSettings();

			StreamReader stream = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

			_incompleteItems.Clear();
			_masterList.Clear();
			_hashTags.Clear();
			_tabHash.Clear();
			_tabList.Clear();
			_historyItems.Clear();
			_hashShortcuts.Clear();

			float version = 0.0f;

			string line = stream.ReadLine();
			if (line != null && line.Contains("=====VERSION"))
			{
				line = stream.ReadLine();
				if (line != null)
				{
					string[] versionPieces = line.Split(' ');
					version = Convert.ToSingle(versionPieces[0]);
				}
			}
			else
				version = 2.0f;

			// Heres where versions are loaded
			if (version <= 2.0f)
				Load2_0SaveFile(stream, line);
			else if (version > 2.0f)
				Load2_1SaveFile(stream, line);
			
			stream.Close();

			RefreshTodo();
			RefreshHistory();
			if (HistoryItems.Count > 0)
			{
				lbHistory.SelectedIndex = 0;
				_currentHistoryItem = HistoryItems[0];
			}
			else
				_currentHistoryItem = new HistoryItem("", "");
			
			_currentOpenFile = path;
			Title = WindowTitle;

			_isChanged = false;
			_currentOpenFile = path;
			
			if (!_currentHistoryItem.HasBeenCopied)
				return;
			DlgYesNo dlgYN = new DlgYesNo("New History", "Start a new History Item?");
			dlgYN.ShowDialog();
			if(dlgYN.Result)
				AddNewHistoryItem();
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
				AddNewTodoTab(line, false);
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
				AddItemToMasterList(td);
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
					_historyItems.Add(new HistoryItem(history));
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
				if (line != null && line.Contains("=====TABS"))
					continue;
				if (line != null && line.Contains("=====FILESETTINGS"))
					break;
			
				AddNewTodoTab(line, false);
			}
			
			stream.ReadLine();
			_backupIncrement = Convert.ToInt16(stream.ReadLine());
			stream.ReadLine();
			int backupMinutes = Convert.ToInt16(stream.ReadLine());
			_backupTime = new TimeSpan(0, backupMinutes, 0);
			stream.ReadLine();
			_autoBackup = Convert.ToBoolean(stream.ReadLine());
			stream.ReadLine();
			_autoSave = Convert.ToBoolean(stream.ReadLine());
			stream.ReadLine();
			_currentProjectVersion = Convert.ToSingle(stream.ReadLine());
			stream.ReadLine();
			_projectVersionIncrement = Convert.ToSingle(stream.ReadLine());
			
			while (line != null)
			{
				line = stream.ReadLine();
				if (line != null && line.Contains("=====VCS"))
					break;
				if (line != null && line.Contains("=====TODO"))
					continue;

				TodoItem td = new TodoItem(line);
				AddItemToMasterList(td);
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
					_historyItems.Add(new HistoryItem(history));
					continue;
				}
				history.Add(line);
			}
		}
		private void AutoSave()
		{
			_isChanged = true;
			_doBackup = true;
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
			_currentOpenFile = path;
			_isChanged = false;
		}
		private void SaveFile(string path)
		{
			StreamWriter stream = new StreamWriter(File.Open(path, FileMode.Create));
			
			stream.WriteLine("====================================VERSION");
			stream.WriteLine(VERSION);

			stream.WriteLine("====================================TABS");
			foreach (TabItem ti in _tabList)
				stream.WriteLine(ti.Name);
			
			stream.WriteLine("====================================FILESETTINGS");
			stream.WriteLine("BackupIncrement");
			stream.WriteLine(_backupIncrement);
			stream.WriteLine("BackupTime");
			stream.WriteLine(_backupTime.Minutes);
			stream.WriteLine("AutoBackup");
			stream.WriteLine(_autoBackup);
			stream.WriteLine("AutoSave");
			stream.WriteLine(_autoSave);
			stream.WriteLine("CurrentProjectVersion");
			stream.WriteLine(_currentProjectVersion);
			stream.WriteLine("ProjectVersionIncrement");
			stream.WriteLine(_projectVersionIncrement);
			
			stream.WriteLine("====================================TODO");
			foreach (TodoItem td in _masterList)
				stream.WriteLine(td.ToString());

			stream.WriteLine("====================================VCS");
			foreach (HistoryItem hi in _historyItems)
				stream.Write(hi.ToString());

			stream.Close();
		}
		private void BackupSave()
		{
			if (!_autoBackup || !_doBackup)
				return;
			
			string path = _recentFiles[0] + ".bak" + _backupIncrement;
			_backupIncrement++;
			_backupIncrement = _backupIncrement > 9 ? 0 : _backupIncrement;
			SaveFile(path);
			_doBackup = false;
		}
	
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Settings //
		private void LoadSettings()
		{
			_recentFiles = new ObservableCollection<string>();
			float version = 0.0f;
			
			string filePath = basePath + "TDHistory.settings";
			if (!File.Exists(filePath))
				SaveSettings();
			
			DlgYesNo dlg;
			
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
					dlgYN.ShowDialog();
					if(dlgYN.Result)
					{
						SaveSettings();
						dlg = new DlgYesNo("New settings file created");
						dlg.ShowDialog();
					}
					dlg = new DlgYesNo("New Todo file created");
					dlg.ShowDialog();
					return;
				}
			}
			else
			{
				line = stream.ReadLine();
				if (line != null)
				{
					string[] versionPieces = line.Split(' ');
					version = Convert.ToSingle(versionPieces[0]);
				}
			}

			// Heres where versions are loaded
			if (version <= 2.0)
				LoadV2_0Settings(stream, line);
			else if (version > 2.0)
				LoadV2_1Settings(stream, line);
			
			stream.Close();

			if (_recentFiles.Count != 0)
				return;
			dlg = new DlgYesNo("New file created");
			dlg.ShowDialog();
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
			stream.ReadLine();
			_pomoWorkTime = Convert.ToInt16(stream.ReadLine());
			_pomoBreakTime = Convert.ToInt16(stream.ReadLine());
			stream.ReadLine();
			_globalHotkeys = Convert.ToBoolean(stream.ReadLine());
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
			stream.WriteLine("GLOBALHOTKEYS");
			stream.WriteLine(_globalHotkeys);
			
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
