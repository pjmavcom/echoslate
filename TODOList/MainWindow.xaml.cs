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
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Button = System.Windows.Controls.Button;
using Clipboard = System.Windows.Forms.Clipboard;
using ComboBox = System.Windows.Controls.ComboBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Label = System.Windows.Controls.Label;
using ListBox = System.Windows.Controls.ListBox;
using MenuItem = System.Windows.Controls.MenuItem;
using TextBox = System.Windows.Controls.TextBox;


namespace TODOList
{
	public partial class MainWindow : INotifyPropertyChanged
	{
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
		public const string DATE_STRING_FORMAT = "yyyyMMdd";
		public const string TIME_STRING_FORMAT = "HHmmss";
		public const string PROGRAM_VERSION = "3.23";

		private readonly List<TabItem> _incompleteItemsTabsList;
		private readonly List<TabItem> _kanbanTabsList;
		private readonly List<TodoItem> _masterList;
		private readonly List<List<TodoItemHolder>> _incompleteItems;
		private readonly List<List<TodoItemHolder>> _kanbanItems;
		private readonly List<List<string>> _hashTags;
		private readonly List<string> _tabHash;
		private static Dictionary<string, string> _hashShortcuts;
		private List<string> _prevHashTagList = new List<string>();

		private string _errorMessage = string.Empty;
		// Sorting
		private bool _reverseSort;
		private string _currentSort = "rank";
		private int _currentHashTagSortIndex = -1;
		private bool _didHashChange;
		private string _hashToSortBy = "";
		private bool _hashSortSelected;
		
		private int _currentSeverity;
		private int _todoTabsPreviousIndex = -1;
		private int _kanbanTabsPreviousIndex = -1;

		// HISTORY TAB ITEMS
		private HistoryItem _currentHistoryItem;
		private int _currentHistoryItemIndex;
		private int _currentHistoryItemMouseIndex;
		private bool _didMouseSelect;

		// FILE IO
		private const string BASE_PATH = @"C:\MyBinaries\";
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
		private double _currentProjectVersion;
		private double _projectVersionIncrement;

		// WINDOW ITEMS
		private double _top;
		private double _left;
		private double _height = 1080;
		private double _width = 1920;

		// HOTKEY STUFF
		private bool _globalHotkeys;
		private const int HOTKEY_ID = 9000;
		private const uint MOD_WIN = 0x0008; //WINDOWS
		private HwndSource _source;
		private IntPtr _handle;
		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
		
		// Pomo Timer 
		private DateTime _pomoTimer;
		private bool _isPomoTimerOn;
		private bool _isPomoWorkTimerOn = true;
		private int _pomoWorkTime = 25;
		private int _pomoBreakTime = 5;
		private int _pomoTimeLeft;
		
		// CONTROLS
		private ComboBox _cbHashTags;
		private ListBox _lbIncompleteItems;
		private ListBox _lbKanbanItems;

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

		private ObservableCollection<string> RecentFiles { get; set; }

		private List<TodoItemHolder> IncompleteItems => _incompleteItems[todoTabs.SelectedIndex];
		private List<TodoItemHolder> KanbanItems => _kanbanItems[kanbanTabs.SelectedIndex];
	
		private List<string> HashTags => _hashTags[todoTabs.SelectedIndex];
		private string TabNames => todoTabs.SelectedIndex == -1
			? _incompleteItemsTabsList[0].Name
			: _incompleteItemsTabsList[todoTabs.SelectedIndex].Name;
		// private string TabKanbanNames => kanbanTabs.SelectedIndex == -1
		// 	? _tabKanbanList[0].Name
		// 	: _tabKanbanList[kanbanTabs.SelectedIndex].Name;
		private string WindowTitle => "EtherealListVCSNotes v" + PROGRAM_VERSION + " " + _currentOpenFile;
		private List<HistoryItem> HistoryItems { get; }

		public int PomoWorkTime
		{
			get => _pomoWorkTime;
			set
			{
				_pomoWorkTime = value;
				OnPropertyChanged();
			}
		}
		public int PomoBreakTime
		{
			get => _pomoBreakTime;
			set
			{
				_pomoBreakTime = value;
				OnPropertyChanged();
			}
		}
		// private string CurrentNotes => "Testing!";

		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //

		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Windows METHODS //
		public  MainWindow()
		{
			_left = 0;
			InitializeComponent();
			Closing += Window_Closed;
			DataContext = this;
#if DEBUG
			mnuMain.Background = Brushes.Red;
#endif
			
			LoadSettings();

			Top = _top;
			Left = _left;
			Height = _height;
			Width = _width;
			_backupTime = new TimeSpan(0, 5, 0);
			_backupIncrement = 0;
 		
			var timer = new DispatcherTimer();
			timer.Tick += Timer_Tick;
			timer.Interval = new TimeSpan(TimeSpan.TicksPerSecond);
			timer.Start();

			_incompleteItemsTabsList = new List<TabItem>();
			_kanbanTabsList = new List<TabItem>();
			_masterList = new List<TodoItem>();
			_incompleteItems = new List<List<TodoItemHolder>>();
			_kanbanItems = new List<List<TodoItemHolder>>();
			_hashTags = new List<List<string>>();
			_tabHash = new List<string>();
			_hashShortcuts = new Dictionary<string, string>();
			HistoryItems = new List<HistoryItem>();
			_currentHistoryItem = new HistoryItem("", "");

			todoTabs.ItemsSource = _incompleteItemsTabsList;
			todoTabs.Items.Refresh();
			kanbanTabs.ItemsSource = _kanbanTabsList;
			kanbanTabs.Items.Refresh();
			mnuRecentLoads.ItemsSource = RecentFiles;
			lbHistory.ItemsSource = HistoryItems;

			lbHistory.SelectedIndex = 0;
			_currentHistoryItemIndex = 0;
			lbCompletedTodos.SelectedIndex = 0;


			//			lblPomoWork.Content = _pomoWorkTime.ToString();
			//			lblPomoBreak.Content = _pomoBreakTime.ToString();

			KanbanCreateTabs();
			
			int noGoodRecentFilesCount = 0;
			for (int i = 0; i < RecentFiles.Count; i++)
			{
				if (File.Exists(RecentFiles[i]))
				{
					Load(RecentFiles[i]);
					break;
				}
				else
				{
					noGoodRecentFilesCount = i + 1;
				}
			}

			if (noGoodRecentFilesCount == RecentFiles.Count)
			{
				new DlgErrorMessage("No recent file found").ShowDialog();
				IncompleteItemsCreateTabs();
			}
			else
			{
				LoadHistory();
			}

			// IncompleteItemsInitialize();
			// KanbanInitialize();
			// RefreshTodo();

			_timeUntilBackup = _backupTime;
		}
		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			_handle = new WindowInteropHelper(this).Handle;
			_source = HwndSource.FromHwnd(_handle);
			_source?.AddHook(HwndHook);

#if DEBUG
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
			const int wmHotkey = 0x0312;
			switch (msg)
			{
				case wmHotkey:
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
			{
				return;
			}

			DlgYesNo dlg = new DlgYesNo("Close", "Maybe save first?");
			dlg.ShowDialog();
			if (dlg.Result)
			{
				Save(_currentOpenFile);
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		private void GlobalHotkeysToggle()
		{
			if (_globalHotkeys)
			{
				RegisterHotKey(_handle, HOTKEY_ID, MOD_WIN, 0x73);
			}
			else
			{
				UnregisterHotKey(_handle, HOTKEY_ID);
			}
		}

		private static string UpperFirstLetter(string s)
		{
			string result = "";
			for (int i = 0; i < s.Length; i++)
			{
				if (i == 0)
				{
					result += s[i].ToString().ToUpper();
				}
				else
				{
					result += s[i];
				}
			}
			return result;
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Tabs //
		private void TabControl_OnSelectionChanged(object sender, EventArgs e)
		{
			int tabIndex = tabControl.SelectedIndex;
			switch (tabIndex)
			{
				case 0:
					// History Tab
					break;
				case 1:
					
					// IncompleteItems (TO DO) Tab
					break;
				case 2:
					// Kanban Tab
					// KanbanSort();
					// RefreshTodo();
					// KanbanRefresh();
					break;
				case 3:
					// Log Tab
					break;
				default:
					// No tab selected
					break;
			}
		}
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Timers //
		private void Timer_Tick(object sender, EventArgs e)
		{
			_timeUntilBackup = _timeUntilBackup.Subtract(new TimeSpan(0, 0, 1));
			if (_timeUntilBackup <= TimeSpan.Zero)
			{
				_timeUntilBackup = _backupTime;
				BackupSave();
			}

			foreach (var td in _masterList.Where(td => td.IsTimerOn))
			{
				td.TimeTaken = td.TimeTaken.AddSeconds(1);
			}

			foreach (TodoItemHolder itemHolder in from list in _incompleteItems from itemHolder in list where itemHolder.TD.IsTimerOn select itemHolder)
			{
				itemHolder.TimeTaken = itemHolder.TD.TimeTaken;
			}

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
					{
						return;
					}
					_isPomoWorkTimerOn = false;
					_pomoTimer=DateTime.MinValue;
				}
				else
				{
					long ticks = _pomoBreakTime * TimeSpan.TicksPerMinute;
					PomoTimeLeft = (int)((float) (ticks - _pomoTimer.Ticks) / ticks * 100);
					if (_pomoTimer.Ticks < ticks)
					{
						return;
					}
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
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// History //
		private void LoadHistory()
		{
			string[] pathPieces = _currentOpenFile.Split('\\');
			string path = "";
			for (int i = 0; i < pathPieces.Length - 1; i++)
			{
				path += pathPieces[i] + "\\";
			}
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
			Process p = new Process
			{
				StartInfo = startInfo
			};
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

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Git //
		private void RefreshLog_OnClick(object sender, EventArgs e)
		{
			LoadHistory();
		}
		private static string FindGitDirectory(string dir)
		{
			while (true)
			{
				if (dir == Directory.GetDirectoryRoot(dir))
				{
					return null;
				}

				List<string> dirs = Directory.GetDirectories(dir).ToList();
				if (dirs.Any(s => s.Contains(".git")))
				{
					return dir;
				}
				dir = Directory.GetParent(dir)?.FullName;
			}
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// HashTags //
		private static string GetHashShortcut(string name, string shortcut)
		{
			while (true)
			{
				string hashShortcut = shortcut + name[0].ToString().ToLower();
				string leftover = "";
				for (int i = 1; i < name.Length; i++)
				{
					leftover += name[i];
				}
				if (!_hashShortcuts.ContainsKey(hashShortcut))
				{
					return hashShortcut;
				}
				name = leftover;
				shortcut = hashShortcut;
			}
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
			string[] pieces = todo.Split(' ');

			List<string> list = new List<string>();
			foreach (string piece in pieces)
			{
				string s = piece;
				if (s.Contains('#'))
				{
					string t = s.ToUpper();
					if (t.Equals("#FEATURES"))
					{
						t = "#FEATURE";
					}

					if (t.Equals("#BUGS"))
					{
						t = "#BUG";
					}

					foreach (string hash in from pair in _hashShortcuts where t.Equals("#" + pair.Key.ToUpper()) select "#" + pair.Value)
					{
						s = hash;
					}
					s = s.ToLower();
				}
				list.Add(s);
			}

			return list.Where(s => s != "").Aggregate("", (current, s) => current + (s + " "));
		}
		private static string ExpandHashTagsInList(List<string> tags)
		{
			string result = tags.Aggregate("", (current, s) => current + (s + " "));

			result = ExpandHashTagsInString(result);
			return result;
		}
		private void HashTagsInitialize()
		{
			if (!(todoTabs.Template.FindName("PART_SelectedContentHost", todoTabs) is ContentPresenter hashTagsContentPresenter))
         	{
         		return;
         	}
         	_cbHashTags = hashTagsContentPresenter.ContentTemplate.FindName("cbHashTags", hashTagsContentPresenter) as ComboBox;
         	if (_cbHashTags == null)
         	{
         		return;
         	}
			_cbHashTags.ItemsSource = HashTags; 
			_cbHashTags.Items.Refresh();
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// IncompleteItems //
		private void IncompleteItemsInitialize()
		{
			if (!(todoTabs.Template.FindName("PART_SelectedContentHost", todoTabs) is ContentPresenter incompleteItemsContentPresenter))
			{
				return;
			}
			incompleteItemsContentPresenter.ApplyTemplate();
			_lbIncompleteItems = incompleteItemsContentPresenter.ContentTemplate.FindName("lbIncompleteItems", incompleteItemsContentPresenter) as ListBox;
			if (_lbIncompleteItems == null)
			{
				return;
			}
			_lbIncompleteItems.ItemsSource = IncompleteItems;
			_lbIncompleteItems.MouseDoubleClick += EditTodo_OnDoubleClick;
			_lbIncompleteItems.SelectionChanged += IncompleteItems_OnSelectionChanged;
		}
		private void IncompleteItemsUpdateHandler()
		{
			if (_todoTabsPreviousIndex == todoTabs.SelectedIndex)
			{
				return;
			}
			_todoTabsPreviousIndex = todoTabs.SelectedIndex;
			
			if (todoTabs.Items.Count <= 0)
			{
				return;
			}
			if (todoTabs.SelectedIndex < 0)
			{
				todoTabs.SelectedIndex = 0;
			}

			if (_lbIncompleteItems == null)
			{
				IncompleteItemsInitialize();
			}

			if (_cbHashTags == null)
			{
				HashTagsInitialize();
			}
			
			RefreshTodo();
		}
		private void IncompleteItemsAddNewTab(string name, bool doSave = true)
		{
			TabItem ti = new TabItem();
			name = UpperFirstLetter(name);
			ti.Header = name;
			ti.Name = name;
			ti.MinWidth = 100;
			ti.Padding = new Thickness(10, 5, 10, 5);
			if (_incompleteItemsTabsList.Any(existingTabItem => existingTabItem.Name == name))
			{
				DlgYesNo dlgYesNo = new DlgYesNo("Already have a tab called " + name);
				dlgYesNo.ShowDialog();
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
			
			foreach (var td in _masterList.Where(td => !td.Rank.ContainsKey(name)))
				td.Rank.Add(name, -1);
			
			_incompleteItems.Add(new List<TodoItemHolder>());
			_hashTags.Add(new List<string>());
			_incompleteItemsTabsList.Add(ti);
			if (!doSave)
			{
				return;
			}
			RefreshTodo();
			todoTabs.Items.Refresh();
			AutoSave();
		}
		private void IncompleteItemsSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Dispatcher.BeginInvoke(new Action(IncompleteItemsUpdateHandler));
		}
		private void IncompleteItemsCreateTabs()
		{
			IncompleteItemsAddNewTab("All", false);
			IncompleteItemsAddNewTab("Other", false);
			IncompleteItemsAddNewTab("Bug", false);
			IncompleteItemsAddNewTab("InProgress", false);
			IncompleteItemsAddNewTab("Feature");
		}
		private void IncompleteItems_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			List<TodoItem> list = IncompleteItems.Select(itemHolder => itemHolder.TD).ToList();
			if (_lbIncompleteItems.SelectedIndex < 0)
			{
				return;
			}
			tbIncompleteItemsNotes.Text = list[_lbIncompleteItems.SelectedIndex].Notes;
			tbTodo.Text = list[_lbIncompleteItems.SelectedIndex].Todo;
			e.Handled = true;
		}
		private void IncompleteItemsHashtags_OnSelectionChanged(object sender, EventArgs e)
		{
			if (HashTags.Count == 0)
			{
				return;
			}
			_hashToSortBy = HashTags[0];
			if (_cbHashTags.SelectedItem != null)
			{
				_hashToSortBy = _cbHashTags.SelectedItem.ToString();
			}
			_hashSortSelected = true;
			_currentSort = "hash";
			RefreshTodo();
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Kanban //
		private void KanbanInitialize()
		{
			if (!(kanbanTabs. Template.FindName("PART_SelectedContentHost", kanbanTabs) is ContentPresenter kanbanContentPresenter))
			{
				return;
			}
			kanbanContentPresenter.ApplyTemplate();
			_lbKanbanItems = kanbanContentPresenter.ContentTemplate.FindName("lbKanbanItems", kanbanContentPresenter) as ListBox;
			if (_lbKanbanItems == null)
			{
				return;
			}
			_lbKanbanItems.ItemsSource = KanbanItems;
			_lbKanbanItems.SelectionChanged += KanbanItems_OnSelectionChange;
			_lbKanbanItems.MouseDoubleClick += EditTodo_OnDoubleClick;
		}
		private void KanbanUpdateHandler()
		{
			if (_kanbanTabsPreviousIndex == kanbanTabs.SelectedIndex)
         	{
         		return;
         	}
         	_kanbanTabsPreviousIndex = kanbanTabs.SelectedIndex;

            if (kanbanTabs.Items.Count <= 0)
            {
	            return;
            }
            if (kanbanTabs.SelectedIndex < 0)
			{
				kanbanTabs.SelectedIndex = 0;
			}

			if (_lbKanbanItems == null)
			{
				KanbanInitialize();
			}

			KanbanRefresh();
			// RefreshTodo();
		}
		private void KanbanAddNewTab(string name)
		{
			TabItem ti = new TabItem
			{
				Header = name,
				Name = name,
				MinWidth = 100,
				Padding = new Thickness(10, 5, 10, 5)
			};

			_kanbanTabsList.Add(ti);
			_kanbanItems.Add(new List<TodoItemHolder>());
			kanbanTabs.Items.Refresh();
		}
		private void KanbanTabSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Dispatcher.BeginInvoke(new Action(KanbanUpdateHandler));
		}
		private void KanbanCreateTabs()
		{
			KanbanAddNewTab("None");
			KanbanAddNewTab("Backlog");
			KanbanAddNewTab("Next");
			KanbanAddNewTab("Current");
		}
		private void KanbanItems_OnSelectionChange(object sender, SelectionChangedEventArgs e)
		{
			List<TodoItem> list = KanbanItems.Select(itemHolder => itemHolder.TD).ToList();
			if (_lbKanbanItems.SelectedIndex < 0)
			{
				return;
			}
			tbKanbanNotes.Text = list[_lbKanbanItems.SelectedIndex].Notes;
			tbTodo2.Text = list[_lbKanbanItems.SelectedIndex].Todo;
			e.Handled = true;
		}
		private void KanbanHashtags_OnSelectionChange(object sender, EventArgs e)
		{
			// TODO: Empty hashtag sorting for kanban view
		}
		private void KanbanSort()
		{
			List<TodoItem> kanbanItems = _masterList.ToList();
			SortCompleteTodosToHistory(kanbanItems);

			foreach (TodoItem todoItem in kanbanItems)
			{
				bool sortedToTab = false;
				int kanbanIndex = todoItem.Kanban;
				_kanbanItems[kanbanIndex].Add(new TodoItemHolder(todoItem));
			}
		}
		private void KanbanRefresh()
		{
			for (int i = 0; i < _kanbanItems.Count; i++)
			{
				_kanbanItems[i].Clear();
			}

			KanbanSort();
			KanbanFixRankings();

			int tabIndex = kanbanTabs.SelectedIndex;
			if (tabIndex < 0 || tabIndex >= _kanbanTabsList.Count)
			{
				return;
			}

			/*
			switch (_currentSort)
			{
				case "severity":
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
						? _incompleteItems[tabIndex].OrderByDescending(o => o.TimeTaken).ToList()
						: _incompleteItems[tabIndex].OrderBy(o => o.TimeTaken).ToList();
					_incompleteItems[tabIndex] = _reverseSort
						? _incompleteItems[tabIndex].OrderByDescending(o => o.IsTimerOn).ToList()
						: _incompleteItems[tabIndex].OrderBy(o => o.IsTimerOn).ToList();
					break;
			}
			*/
			
			if (_lbKanbanItems != null)
			{
				_lbKanbanItems.ItemsSource = KanbanItems;
				_lbKanbanItems.Items.Refresh();
			}
		}

		private void SortCompleteTodosToHistory(IEnumerable<TodoItem> todoItemsList)
		{
			foreach (var todoItem in todoItemsList.Where(todoItem => todoItem.IsComplete))
			{
				AddTodoToHistory(todoItem);
				RemoveItemFromMasterList(todoItem);
			}
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Hotkeys //
		private void HkSwitchTab(object sender, ExecutedRoutedEventArgs e)
		{
			int index = todoTabs.SelectedIndex;
			switch ((string) e.Parameter)
			{
				case "right" when tabControl.SelectedIndex == 0:
					tabControl.SelectedIndex = 1;
					return;
				case "right":
				{
					index++;
					if (index >= todoTabs.Items.Count)
					{
						index = todoTabs.Items.Count - 1;
					}
					break;
				}
				case "left":
				{
					index--;
					if (index < 0)
					{
						index = 0;
						tabControl.SelectedIndex = 0;
					}

					break;
				}
			}
			todoTabs.SelectedIndex = index;
		}
		private void HkSwitchSeverity(object sender, ExecutedRoutedEventArgs e)
		{
			int index = cbSeverity.SelectedIndex;
			switch ((string) e.Parameter)
			{
				case "down":
				{
					index++;
					if (index >= cbSeverity.Items.Count)
					{
						index = cbSeverity.Items.Count - 1;
					}
					break;
				}
				case "up":
				{
					index--;
					if (index < 0)
					{
						index = 0;
					}
					break;
				}
			}
			cbSeverity.SelectedIndex = index;
			cbSeverity.Items.Refresh();
		}
		private void HkComplete(object sender, ExecutedRoutedEventArgs e)
		{
			if (tabHistory.IsSelected)
			{
				return;
			}
			if (tbNewTodo.IsFocused)
			{
				QuickComplete();
				return;
			}

			TodoItemHolder itemHolder = (TodoItemHolder) _lbIncompleteItems.SelectedItem;
			if (itemHolder != null)
			{
				DlgTodoItemEditor  dlgTodoItemEditor = new DlgTodoItemEditor(itemHolder.TD, TabNames);
				dlgTodoItemEditor.ShowDialog();
				if (dlgTodoItemEditor.Result)
				{
					AddItemToMasterList(dlgTodoItemEditor.ResultTD);
				}
			}
			RefreshTodo();
		}
		private void HkAdd(object sender, ExecutedRoutedEventArgs e)
		{
			if (tabHistory.IsSelected)
			{
				return;
			}
			Add_OnClick(sender, e);
		}
		private void HkEdit(object sender, EventArgs e)
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
				lb = _lbIncompleteItems;
				list.AddRange(IncompleteItems.Select(itemHolder => itemHolder.TD));
			}
			else if (tabHistory.IsSelected)
			{
				lb = lbCompletedTodos;
				list = _currentHistoryItem.CompletedTodos;
			}
		
			EditItem(lb, list);
		}
		private void HkQuickSave(object sender, ExecutedRoutedEventArgs e)
		{
			Save(RecentFiles[0]);
		}
		private void HkQuickLoadPrevious(object sender, ExecutedRoutedEventArgs e)
		{
			if (RecentFiles.Count >= 2)
			{
				Load(RecentFiles[1]);
			}
		}
		private void HkStartStopTimer(object sender, ExecutedRoutedEventArgs e)
		{
			if (!tabTodos.IsSelected)
			{
				return;
			}
			int index = _lbIncompleteItems.SelectedIndex;
			IncompleteItems[index].TD.IsTimerOn = !IncompleteItems[index].TD.IsTimerOn;
			_lbIncompleteItems.Items.Refresh();
		}
		private void QuickComplete()
		{
			TodoItem newTodo = new TodoItem
			{
				Todo = tbNewTodo.Text,
				Severity = _currentSeverity,
				IsComplete = true,
				Rank = {[TabNames] = IncompleteItems.Count}
			};

			AddItemToMasterList(newTodo);
			AutoSave();
			RefreshTodo();
			tbNewTodo.Clear();
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// MenuCommands //
		private void mnuEditTabs_OnClick(object sender, EventArgs e)
		{
			List<TabItem> list = _incompleteItemsTabsList.ToList();
			DlgEditTabs rt = new DlgEditTabs(list);
			rt.ShowDialog();
			if (!rt.Result)
			{
				return;
			}
			_incompleteItemsTabsList.Clear();
			_hashShortcuts.Clear();
			_tabHash.Clear();
			_hashTags.Clear();
			_incompleteItems.Clear();
			foreach (string s in rt.ResultList)
			{
				if (rt.ResultList.IndexOf(s) < rt.ResultList.Count - 1)
				{
					IncompleteItemsAddNewTab(s, false);
				}
				else
				{
					IncompleteItemsAddNewTab(s);
				}
			}

			foreach (TodoItem td in _masterList)
			{
				CleanTodoHashRanks(td);
			}
		}
		private void mnuOptions_OnClick(object sender, EventArgs e)
		{
			DlgOptions options = new DlgOptions(_autoSave, _globalHotkeys, _autoBackup, _backupTime, _currentProjectVersion, _projectVersionIncrement);
			options.ShowDialog();
			if (!options.Result)
			{
				return;
			}
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
			AutoSave();
			DlgYesNo dlg = new DlgYesNo("New file", "Are you sure?");
			dlg.ShowDialog();
			if (!dlg.Result)
			{
				return;
			}

			if (!NewFile())
			{
				return;
			}
			ClearLists();
			IncompleteItemsCreateTabs();
			_currentProjectVersion = 0.00d;
			_projectVersionIncrement = 0.01d;

			_currentHistoryItem = new HistoryItem("", "");
			AddNewHistoryItem();
			RefreshHistory();
			RefreshTodo();

			_currentOpenFile = "";
			Title = WindowTitle;
			Save(RecentFiles[0]);
		}
		private void mnuRemoveFile_OnClick(object sender, RoutedEventArgs e)
		{
			if (_recentFilesIndex < 0)
			{
				_recentFilesIndex = 0;
				return;
			}
			RecentFiles.RemoveAt(_recentFilesIndex);
			mnuRecentLoads.Items.Refresh();
		}
		private void mnuRecentLoads_OnRMBUp(object sender, MouseButtonEventArgs e)
		{
			_recentFilesIndex = -1;
			//var t = ((TextBlock) e.OriginalSource).Text;
			if (!(e.OriginalSource is TextBlock mi))
				return;

			string path = (string) mi.DataContext;
			_recentFilesIndex = mnuRecentLoads.Items.IndexOf(path);
		}
		private void mnuResetHistoryCopied_OnClick(object sender, EventArgs e)
		{
			int index = lbHistory.SelectedIndex;
			HistoryItems[index].ResetCopied();
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
			{
				return;
			}
			
			MenuItem mi = (MenuItem) e.OriginalSource;
			if (!(mi.DataContext is string path))
			{
				return;
			}
			
			if(_isChanged)
			{
				DlgYesNo dlgYesNo = new DlgYesNo("Close", "Maybe save first?");
				dlgYesNo.ShowDialog();
				if (dlgYesNo.Result)
				{
					Save(_currentOpenFile);
				}
			}
			
			Load(path);
		}
		private void mnuLoad_OnClick(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Title = @"Open file: ",
				InitialDirectory = GetFilePath(),
				Filter = @"txt files (*.txt)|*.txt|All files (*.*)|*.*"
			};

			DialogResult dr = openFileDialog.ShowDialog();

			if (dr != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}
			
			if(_isChanged)
			{
				DlgYesNo dlgYesNo = new DlgYesNo("Close", "Maybe save first?");
				dlgYesNo.ShowDialog();
				if (dlgYesNo.Result)
				{
					Save(_currentOpenFile);
				}
			}

			Load(openFileDialog.FileName);
		}
		private void mnuEditHistoryTodo_OnClick(object sender, EventArgs e)
		{
			if (lbCompletedTodos.IsMouseOver)
			{
				EditItem(lbCompletedTodos, _currentHistoryItem.CompletedTodos);
			}
			else if (lbCompletedTodosFeatures.IsMouseOver)
			{
				EditItem(lbCompletedTodosFeatures, _currentHistoryItem.CompletedTodosFeatures);
			}
			else if (lbCompletedTodosBugs.IsMouseOver)
			{
				EditItem(lbCompletedTodosBugs, _currentHistoryItem.CompletedTodosBugs);
			}
			
			RefreshHistory();
		}
		private void mnuHelp_OnClick(object sender, EventArgs e)
		{
			DlgHelp dlgH = new DlgHelp();
			dlgH.ShowDialog();
		}

		// List Context Menus
		private void ResetTimer()
		{
			TodoItemHolder todoItemHolder = GetSelectedTodoItemHolder();
			if (todoItemHolder == null)
			{
				_errorMessage = "Function: ResetTimer()\n" +
				                "\tGetSelectedTodoItemHolder() return null\n" +
				                _errorMessage;
				return;
			}

			todoItemHolder.TimeTaken = new DateTime();
			todoItemHolder.TD.IsTimerOn = false;
		}
		private void EditTodo_OnDoubleClick(object sender, EventArgs e)
		{
			MenuEditTodo_OnClick();
		}
		private void mnuDelete_OnClick()
		{
			TodoItem td = GetSelectedTodo();
			if (td == null)
			{
				_errorMessage = "Function: mnuDelete_OnClick()" +
				                "\n\ttd == null\n" +
				                _errorMessage;
				return;
			}
			RemoveItemFromMasterList(td);
			RefreshTodo();
		}
		private void mnuKanban_OnClick(int kanbanRank)
   		{
   			TodoItem td = GetSelectedTodo();
   			if (td == null)
   			{
   				_errorMessage = "\nFunction: mnuKanban_OnClick()" +
   				                "\n\n" + _errorMessage;
   				return;
   			}
   			td.Kanban = kanbanRank;
	    }
		private void MenuEditTodo_OnClick()
		{
			switch (tabControl.SelectedIndex)
			{
				case 1:
					EditTodo(IncompleteItems, _lbIncompleteItems);
					break;
				case 2:
					EditTodo(KanbanItems, _lbKanbanItems);
					break;
				default:
					_errorMessage = "Function: MenuEditTodo_OnClick()" +
					                "\n\tNot a valid tabControl.SelectedIndex";
					break;
			}
		}
		private void EditTodo(List<TodoItemHolder> list, ListBox listBox)
		{
			if (listBox.SelectedItems.Count > 1)
			{
				MultiEditItems(listBox);
			}
			else if (listBox.SelectedItems.Count == 1)
			{
				List<TodoItem> itemsList = list.Select(itemHolder => itemHolder.TD).ToList();
				EditItem(listBox, itemsList);
			}
			else
			{
				_errorMessage = "No selected items!" +
				                "\nFunction: EditTodo()";
			}
		}

		private void mnuContextMenu_OnClick(object sender, EventArgs e)
		{
			_errorMessage = string.Empty;
			
			MenuItem menuItem = sender as MenuItem;
			if (menuItem == null)
			{
				new DlgErrorMessage("Function: mnuContextMenu_OnClick()" +
				             "\n\tmenuItem == null").ShowDialog();
				return;
			}
			if (menuItem.CommandParameter == null)
			{
				new DlgErrorMessage("Function: mnuContextMenu_OnClick()" +
				             "\n\tNo CommandParameter").ShowDialog();
				return;
			}

			string command = menuItem?.CommandParameter.ToString();
			switch (command)
			{
				case "Edit":
					MenuEditTodo_OnClick();
					break;
				case "Delete":
					mnuDelete_OnClick();
					break;
				case "Kanban0":
					mnuKanban_OnClick(0);
					break;
				case "Kanban1":
					mnuKanban_OnClick(1);
					break;
				case "Kanban2":
					mnuKanban_OnClick(2);
					break;
				case "Kanban3":
					mnuKanban_OnClick(3);
					break;
				case "ResetTimer":
					ResetTimer();
					break;
				default:
					_errorMessage = "\n\tNo recognized command parameter";
					break;
			}

			if (_errorMessage != string.Empty)
			{
				new DlgErrorMessage("Function: mnuContextMenu_OnClick()\n" +
				                _errorMessage).ShowDialog();
				return;
			}
			AutoSave();
			RefreshTodo();
			KanbanRefresh();
		}

		private TodoItemHolder GetSelectedTodoItemHolder()
		{
			TodoItemHolder todoItemHolder = null;
			int tabIndex = tabControl.SelectedIndex;
			string tabName = "No valid tab selected";
			int selectedIndex = -1;
			switch (tabIndex)
			{
				case 1:
					tabName = "IncompleteItems";
					selectedIndex = _lbIncompleteItems.SelectedIndex;
					if (selectedIndex < 0)
					{
						_errorMessage = "\t_lbIncompleteItems.SelectedIndex == -1";
						break;
					}

					if (selectedIndex > _lbIncompleteItems.Items.Count)
					{
						_errorMessage = "\t_lbIncompleteItems.SelectedIndex out of range";
						break;
					}

					todoItemHolder = IncompleteItems[selectedIndex];
					break;
				case 2:
					tabName = "KanbanItems";
					selectedIndex = _lbKanbanItems.SelectedIndex;
					if (selectedIndex < 0)
					{
						_errorMessage = "\t_lbKanbanItems.SelectedIndex == -1";
						break;
					}
					if (selectedIndex > _lbKanbanItems.Items.Count)
					{
						_errorMessage = "\t_lbKanbanItems.SelectedIndex out of range";
						break;
					}

					todoItemHolder = KanbanItems[selectedIndex];
					break;
				default:
					_errorMessage = "\ttabIndex is not a valid index to a TodoList";
					break;
			}

			if (todoItemHolder == null)
			{
				_errorMessage = "\nFunction: GetTodoItemHolder()\n" +
				                "\tSelected Tab: " + tabName + "\n" +
				                "\tSelected Index: " + selectedIndex + "\n" +
				                "\ttodoItemHolder == null\n" +
				                _errorMessage;
			}

			return todoItemHolder;
		}
		private TodoItem GetSelectedTodo()
		{
			TodoItemHolder todoItemHolder = GetSelectedTodoItemHolder();
			if (todoItemHolder == null)
			{
				_errorMessage = "\nFunction: GetSelectedTodo()\n" +
				                "\tGetSelectedTodoItemHolder() returned null:\n" +
				                _errorMessage;
				return null;
			}

			TodoItem td = todoItemHolder.TD;
			if (td == null)
			{
				_errorMessage = "\nFunction: GetSelectedTodo()\n" +
				                "\ttodoItem == null\n" +
				                "\ttodoItemHolder did return a value though..." +
				                _errorMessage;
				return null;
			}

			return td;
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
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (_currentHistoryItemIndex >= HistoryItems.Count)
			{
				_currentHistoryItemIndex = 0;
			}

			if (_currentHistoryItemIndex < 0)
			{
				_currentHistoryItemIndex = HistoryItems.Count - 1;
			}
			if (HistoryItems.Count == 0)
			{
				_currentHistoryItem = new HistoryItem("", "");
				return;
			}

			if (prevIndex == _currentHistoryItemIndex)
			{
				return;
			}

			_currentHistoryItem = lbHistory.Items[_currentHistoryItemIndex] as HistoryItem;
			if (_currentHistoryItem != null)
			{
				lblHTotalTime.Content = _currentHistoryItem.TotalTime;
			}
			lbHistory.SelectedIndex = _currentHistoryItemIndex;
			RefreshHistory();
		}
		private void HistoryListBox_OnMouseDown(object sender, EventArgs e)
		{
			_didMouseSelect = true;
		}
		private void NewHistory_OnClick(object sender, EventArgs e)
		{
			AddNewHistoryItem();
		}
		private void HistoryListBox_OnSelectionChange(object sender, SelectionChangedEventArgs e)
		{
			if (_didMouseSelect)
			{
				_currentHistoryItemIndex = lbHistory.SelectedIndex;
			}

			if (lbHistory.Items.Count == 0)
			{
				return;
			}

			if (_currentHistoryItemIndex >= lbHistory.Items.Count)
			{
				_currentHistoryItemIndex = lbHistory.Items.Count - 1;
			}
			if (lbHistory.Items[_currentHistoryItemIndex] is HistoryItem hi)
			{
				_currentHistoryItem = hi;
				if (_currentHistoryItem != null)
				{
					lblHTotalTime.Content = _currentHistoryItem.TotalTime;
				}
			}
			
			RefreshHistory();
			_didMouseSelect = false;
		}
		private void AddTodoToHistory(TodoItem td)
		{
			if (_currentHistoryItem.DateAdded == "")
			{
				AddNewHistoryItem();
			}
			RefreshHistory();
			_currentHistoryItem = HistoryItems[0];
			_currentHistoryItem.AddCompletedTodo(td);
			RefreshHistory();
			AutoSave();
		}
		private void AddNewHistoryItem()
		{
			_currentProjectVersion = Math.Round(_currentProjectVersion, 2);
			_projectVersionIncrement = Math.Round(_projectVersionIncrement, 2);
			_currentProjectVersion += _projectVersionIncrement;
			_currentProjectVersion = Math.Round(_currentProjectVersion, 2);

			_currentHistoryItem = new HistoryItem(DateTime.Now)
			{
				Title = "v" + _currentProjectVersion
			};
			HistoryItems.Add(_currentHistoryItem);
			AutoSave();
			RefreshHistory();
		}
		private void RefreshHistory()
		{
			List<HistoryItem> temp = HistoryItems.OrderByDescending(o => o.DateTimeAdded).ToList();
			HistoryItems.Clear();
			foreach (HistoryItem hi in temp)
			{
				HistoryItems.Add(hi);
			}

			if (HistoryItems.Count == 0)
			{
				_currentHistoryItem = new HistoryItem("", "");
			}

			if (HistoryItems.Count > 0 && _currentHistoryItem.DateAdded == "")
			{
				lbHistory.SelectedIndex = 0;
			}

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
		private void DeleteHistory_OnClick(object sender, EventArgs e)
		{
			if (HistoryItems.Count == 0)
			{
				return;
			}
			DlgYesNo  dlgYesNo = new DlgYesNo("Delete", "Are you sure you want to delete this History Item?");
			dlgYesNo.ShowDialog();
			if (!dlgYesNo.Result)
			{
				return;
			}
			
			HistoryItems.Remove(_currentHistoryItem);

			_currentHistoryItem = HistoryItems.Count > 0 ? HistoryItems[0] : new HistoryItem("", "");
			AutoSave();
			RefreshHistory();
		}
		private void CopyHistory_OnClick(object sender, EventArgs e)
		{
			Button b = sender as Button;
			HistoryItem hi = (HistoryItem) b?.DataContext;
			if (hi == null)
			{
				return;
			}
			int totalTime = HistoryItems.Sum(hist => Convert.ToInt32(hist.TotalTime));
			string time = $"{totalTime / 60:00} : {totalTime % 60:00}";
			Clipboard.SetText(hi.ToClipboard(time));
			hi.SetCopied();
			if (lbHistory.Items.IndexOf(hi) != 0)
			{
				return;
			}
			DlgYesNo dlgYesNo = new DlgYesNo("New History", "Add a new History Item?");
			dlgYesNo.ShowDialog();
			if (dlgYesNo.Result)
			{
				AddNewHistoryItem();
			}
		}
		private static void SortCompletedItems(HistoryItem hi)
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
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// TODOS //
		private List<TodoItem> GetActiveItemList()
		{
			switch (tabControl.SelectedIndex)
			{
				case 1:
					return IncompleteItems.Select(itemHolder => itemHolder.TD).ToList();
				case 2:
					return KanbanItems.Select(itemHolder => itemHolder.TD).ToList();
				default:
					_errorMessage = "Function: GetActiveItemList()\n" +
					                "\tSelectedIndex: " + tabControl.SelectedIndex + "\n" +
					                "\tNot a valid tab with a list\n";
					return null;
			}
		}
		private ListBox GetActiveListBox()
		{
			switch (tabControl.SelectedIndex)
			{
				case 1:
					if (_lbIncompleteItems == null)
					{
						IncompleteItemsInitialize();
					}
					return _lbIncompleteItems;
				case 2:
					if (_lbKanbanItems == null)
					{
						KanbanInitialize();
					}
					return _lbKanbanItems;
				default:
					_errorMessage = "Function: GetActiveListBox()\n" +
					                "\tSelectedIndex: " + tabControl.SelectedIndex + "\n" +
					                "\tNot a valid tab with a ListBox\n" +
					                _errorMessage;
					return null;
			}
		}
		private TextBox GetActiveTextBox()
		{
			switch (tabControl.SelectedIndex)
			{
				case 1:
					return tbIncompleteItemsNotes;
				case 2:
					return tbKanbanNotes;
				default:
					_errorMessage = "Function: GetActiveTextBox()\n" +
					                "\tSelectedIndex: " + tabControl.SelectedIndex + "\n" +
					                "\tNot a valid tab with a Notes textbox\n" +
					                _errorMessage;
					return null;
			}
		}
		private void Notes_OnSelectionChanged(object sender, RoutedEventArgs e)
		{
			List<TodoItem> todoItemList = GetActiveItemList();
			ListBox listBox = GetActiveListBox();
			TextBox textBox = GetActiveTextBox();
			if (textBox == null || listBox == null || todoItemList == null)
            {
            	_errorMessage = "Function: Notes_OnSelectionChanged()\n" +
            	                "\ttodoItemList == " + todoItemList + "\n" +
	                            "\tlistBox == " + listBox + "\n" +
	                            "\ttextBox == " + textBox + "\n" +
	                            _errorMessage;
            	new DlgErrorMessage(_errorMessage).ShowDialog();
	            _errorMessage = string.Empty;
            	return;
            }
			if (listBox.SelectedIndex >= 0)
			{
				todoItemList[listBox.SelectedIndex].Notes = textBox.Text;
			}
		}
		
		private void DeleteTodo_OnClick(object sender, EventArgs e)
		{
			Button b = sender as Button;
			TodoItem td = b?.DataContext as TodoItem;
			DlgYesNo dlgYesNo = new DlgYesNo("Delete", "Are you sure you want to delete this Todo?");
			dlgYesNo.ShowDialog();
			if (!dlgYesNo.Result)
			{
				return;
			}

			if (td != null)
			{
				td.IsComplete = false;
				AddItemToMasterList(td);
				RefreshTodo();
				KanbanRefresh();
				if (_currentHistoryItem.CompletedTodos.Contains(td))
				{
					_currentHistoryItem.CompletedTodos.Remove(td);
				}
				else if (_currentHistoryItem.CompletedTodosBugs.Contains(td))
				{
					_currentHistoryItem.CompletedTodosBugs.Remove(td);
				}
				else if (_currentHistoryItem.CompletedTodosFeatures.Contains(td))
				{
					_currentHistoryItem.CompletedTodosFeatures.Remove(td);
				}
				AutoSave();
			}
			RefreshHistory();
		}
		private void Severity_OnClick(object sender, EventArgs e)
		{
			if (!(sender is Button b))
			{
				return;
			}
			if (b.DataContext is TodoItemHolder itemHolder)
			{
				int value = itemHolder.Severity;
				if (value == 3) { value = 0; }
				else { value++; }
				itemHolder.Severity = value;
			}

			RefreshTodo();
			KanbanRefresh();
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Sorting //
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
				{
					return;
				}
				_currentHashTagSortIndex++;
				if (_currentHashTagSortIndex >= HashTags.Count)
				{
					_currentHashTagSortIndex = 0;
				}
			}

			_reverseSort = !_reverseSort;
			RefreshTodo();
			KanbanRefresh();
		}
		private void KanbanSort_OnClick(object sender, EventArgs e)
		{
			// TODO: Kanban sorting
		}
		private List<TodoItemHolder> SortByHashTag(List<TodoItemHolder> list)
		{
			if (_didHashChange)
			{
				_currentHashTagSortIndex = 0;
			}

			List<TodoItemHolder> incompleteItems = new List<TodoItemHolder>();
			List<string> sortedHashTags = new List<string>();

			if (HashTags.Count == 0)
			{
				return list;
			}
			
			if (_hashSortSelected)
			{
				_currentHashTagSortIndex = 0;
				foreach (string s in HashTags.TakeWhile(s => !s.Equals(_hashToSortBy)))
				{
					_currentHashTagSortIndex++;
				}
			}

			for (int i = 0 + _currentHashTagSortIndex; i < HashTags.Count; i++)
			{
				sortedHashTags.Add(HashTags[i]);
			}

			for (int i = 0; i < _currentHashTagSortIndex; i++)
			{
				sortedHashTags.Add(HashTags[i]);
			}

			List<TodoItemHolder> sortSingleTags = list.ToList();
			
			foreach (var itemHolder in sortSingleTags.Where(itemHolder => itemHolder.TD.Tags.Contains(sortedHashTags[0]) && itemHolder.TD.Tags.Count == 1))
			{
				incompleteItems.Add(itemHolder);
				list.Remove(itemHolder);
			}
			foreach (string s in sortedHashTags)
			{
				List<TodoItemHolder> temp = list.ToList();
				foreach (TodoItemHolder itemHolder in temp)
				{
					List<string> sortedTags = new List<string>();
					List<string> unsortedTags = itemHolder.TD.Tags.ToList();
					foreach (string u in itemHolder.TD.Tags.Where(u => u == s))
					{
						sortedTags.Add(u);
						unsortedTags.Remove(u);
					}
					sortedTags.AddRange(unsortedTags);
					itemHolder.TD.Tags = sortedTags;
					
					foreach (string t in itemHolder.TD.Tags.Where(t => s.Equals(t)))
					{
						incompleteItems.Add(itemHolder);
						list.Remove(itemHolder);
					}
				}
			}

			incompleteItems.AddRange(list);

			return incompleteItems;
		}
		private void FixRankings()
		{
			if (todoTabs.Items.Count == 0 ||
			    todoTabs.SelectedIndex < 0 ||
			    todoTabs.SelectedIndex >= todoTabs.Items.Count)
			{
				return;
			}
			string currentHash = _incompleteItemsTabsList[todoTabs.SelectedIndex].Name;
			foreach (TodoItemHolder itemHolder in _incompleteItems[todoTabs.SelectedIndex].Where(itemHolder => !itemHolder.TD.Rank.ContainsKey(currentHash)))
			{
				itemHolder.TD.Rank.Add(currentHash, 99);
			}
			_incompleteItems[todoTabs.SelectedIndex] = _incompleteItems[todoTabs.SelectedIndex].OrderBy(o => o.TD.Rank[currentHash]).ToList();
			for (int rank = 0; rank < _incompleteItems[todoTabs.SelectedIndex].Count; rank++)
			{
				_incompleteItems[todoTabs.SelectedIndex][rank].TD.Rank[currentHash] = rank + 1;
				_incompleteItems[todoTabs.SelectedIndex][rank].Rank = _incompleteItems[todoTabs.SelectedIndex][rank].TD.Rank[currentHash];
			}
		}
		private void CheckForHashTagListChanges()
		{
			_didHashChange = false;
			if (_hashTags[0].Count != _prevHashTagList.Count)
			{
				_didHashChange = true;
			}
			else
			{
				for (int i = 0; i < _hashTags[0].Count; i++)
				{
					if (_hashTags[0][i] != _prevHashTagList[i])
					{
						_didHashChange = true;
					}
				}
			}
			_prevHashTagList = _hashTags[0].ToList();
		}
		private void SortHashTagLists()
		{
			for (int i = 0; i < _incompleteItems.Count; i++)
			{
				foreach (string tag in _incompleteItems[i].SelectMany(itemHolder => itemHolder.TD.Tags))
				{
					if (!_hashTags[i].Contains(tag))
					{
						_hashTags[i].Add(tag);
					}

					if (!_hashTags[0].Contains(tag))
					{
						_hashTags[0].Add(tag);
					}
				}

				_hashTags[i] = _hashTags[i].OrderBy(o => o).ToList();
			}
			for (int i = 1; i < _incompleteItemsTabsList.Count; i++)
			{
				_incompleteItemsTabsList[i].Header = _tabHash[i - 1] + " " + _incompleteItems[i].Count;
			}
			_incompleteItemsTabsList[0].Header = "All " + _incompleteItems[0].Count;
		}
		private void SortToLists()
		{
			List<TodoItem> incompleteItems = _masterList.ToList();
			SortCompleteTodosToHistory(incompleteItems);
			foreach (TodoItem td in incompleteItems)
			{
				bool sortedToTab = false;

				TodoItemHolder itemHolder = new TodoItemHolder(td);
				_incompleteItems[0].Add(itemHolder);

				// Add to do to all tabs the hash is from...
				foreach (int index in from hash in _tabHash where td.Tags.Contains(hash) select _tabHash.IndexOf(hash) + 1)
				{
					_incompleteItems[index].Add(itemHolder);
					sortedToTab = true;
				}

				// ...if sorted to any tab, skip the next step...
				if (sortedToTab)
				{
					continue;
				}

				// ...of adding to the unsorted tab
				if (_incompleteItems.Count > 1)
				{
					_incompleteItems[1].Add(itemHolder);
				}
			}
		}

		private void KanbanFixRankings()
		{
			
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
		
			int tabIndex = todoTabs.SelectedIndex;
			if (tabIndex < 0 || tabIndex >= _incompleteItemsTabsList.Count)
			{
				return;
			}
			
			switch (_currentSort)
			{
				case "severity":
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
						? _incompleteItems[tabIndex].OrderByDescending(o => o.TimeTaken).ToList()
						: _incompleteItems[tabIndex].OrderBy(o => o.TimeTaken).ToList();
					_incompleteItems[tabIndex] = _reverseSort
						? _incompleteItems[tabIndex].OrderByDescending(o => o.IsTimerOn).ToList()
						: _incompleteItems[tabIndex].OrderBy(o => o.IsTimerOn).ToList();
					break;
			}

			if (_lbIncompleteItems != null)
			{
				_lbIncompleteItems.ItemsSource = IncompleteItems;
				_lbIncompleteItems.Items.Refresh();
			}

			if (_cbHashTags != null)
			{
				_cbHashTags.ItemsSource = HashTags;
				_cbHashTags.Items.Refresh();
			}
		}
		private void SeverityComboBox_OnSelectionChange(object sender, EventArgs e)
		{
			if (!(sender is ComboBox cb))
			{
				return;
			}
			int index = cb.SelectedIndex;
			_currentSeverity = index;
		}
		private void SeverityComboBox_OnIsLoaded(object sender, EventArgs e)
		{
			if (sender is ComboBox cb)
			{
				cb.SelectedIndex = _currentSeverity;
			}
		}
		private void Add_OnClick(object sender, EventArgs e)
		{
			string name = TabNames;
			TodoItem td = new TodoItem() {Todo = tbNewTodo.Text, Severity = _currentSeverity};
			td.Tags.Add("#" + name);
			ExpandHashTags(td);
			td.Rank[TabNames] = -1;
			if (td.Severity == 3)
			{
				td.Rank[TabNames] = 0;
			}

			AddItemToMasterList(td);
			AutoSave();
			RefreshTodo();
			KanbanRefresh();
			tbNewTodo.Clear();
		}
		private void RankAdjust_OnClick(object sender, EventArgs e)
		{
			if (!(sender is Button b))
			{
				return;
			}
			TodoItemHolder itemHolder = b.DataContext as TodoItemHolder;

			if (IncompleteItems.Count == 0)
			{
				return;
			}
			int index = IncompleteItems.IndexOf(itemHolder);
			switch ((string) b.CommandParameter)
			{
				case "up" when index == 0:
					return;
				// TODO: Fix this too
				case "up":
				{
					int newRank = IncompleteItems[index - 1].TD.Rank[TabNames];
					if (itemHolder != null)
					{
						IncompleteItems[index - 1].TD.Rank[TabNames] = itemHolder.Rank;
						itemHolder.TD.Rank[TabNames] = newRank;
						AutoSave();
					}

					break;
				}
				case "down" when index >= IncompleteItems.Count - 1:
					return;
				// TODO: And this 
				case "down":
				{
					int newRank = IncompleteItems[index + 1].TD.Rank[TabNames];
					if (itemHolder != null)
					{
						IncompleteItems[index + 1].TD.Rank[TabNames] = itemHolder.Rank;
						itemHolder.TD.Rank[TabNames] = newRank;
						AutoSave();
					}

					break;
				}
			}
			RefreshTodo();
			KanbanRefresh();
//				Point p = b.PointToScreen(new Point(0d, 0d));;
//				SetCursorPosition((int) p.X + 18, (int) p.Y + 18);
		}
		private void AddItemToMasterList(TodoItem td)
		{
			if (MasterListContains(td) >= 0)
			{
				return;
			}

			CleanTodoHashRanks(td);

			_masterList.Add(td);
		}
		private void RemoveItemFromMasterList(TodoItem td)
		{
			int index = MasterListContains(td);
			if (index == -1)
			{
				return;
			}
			
			_masterList.RemoveAt(index);
		}
		private int MasterListContains(TodoItem td)
		{
			if (_masterList.Contains(td))
			{
				return _masterList.IndexOf(td);
			}
			return -1;
		}
		private void CleanTodoHashRanks(TodoItem td)
		{
			List<string> tabNames = _incompleteItemsTabsList.Select(ti => ti.Name).ToList();
			List<string> remove = (from pair in td.Rank where !tabNames.Contains(pair.Key) select pair.Key).ToList();
			foreach (string hash in remove)
			{
				td.Rank.Remove(hash);
			}

			foreach (string name in tabNames.Where(name => !td.Rank.ContainsKey(name)))
			{
				td.Rank.Add(name, -1);
			}
		}
		private void EditItem(Selector lb, IReadOnlyList<TodoItem> list)
		{
			int index = lb.SelectedIndex;
			if (index < 0)
			{
				return;
			}
			TodoItem td = list[index];
			DlgTodoItemEditor itemEditor = new DlgTodoItemEditor(td, TabNames);

			itemEditor.ShowDialog();
			if (itemEditor.Result)
			{
				RemoveItemFromMasterList(td);
				if (_currentHistoryItem.CompletedTodos.Contains(td))
				{
					_currentHistoryItem.CompletedTodos.Remove(td);
				}

				if (_currentHistoryItem.CompletedTodosBugs.Contains(td))
				{
					_currentHistoryItem.CompletedTodosBugs.Remove(td);
				}

				if (_currentHistoryItem.CompletedTodosFeatures.Contains(td))
				{
					_currentHistoryItem.CompletedTodosFeatures.Remove(td);
				}
				AddItemToMasterList(itemEditor.ResultTD);
				AutoSave();
			}

			RefreshTodo();
			KanbanRefresh();
			RefreshHistory();
		}
		private void MultiEditItems(ListBox lb)
		{
			TodoItemHolder firstTd = lb.SelectedItems[0] as TodoItemHolder;

			List<string> tags = new List<string>();
			List<string> commonTagsTemp = new List<string>();
			foreach (TodoItemHolder itemHolder in lb.SelectedItems)
			{
				foreach(string tag in itemHolder.TD.Tags)
				{
					if (!tags.Contains(tag))
					{
						tags.Add(tag);
					}
					else
					{
						if (!commonTagsTemp.Contains(tag))
						{
							commonTagsTemp.Add(tag);
						}
					}
				}
			}
			List<string> commonTags = commonTagsTemp.ToList();
			foreach (TodoItemHolder itemHolder in lb.SelectedItems)
			{
				foreach (string tag in commonTagsTemp.Where(tag => !itemHolder.TD.Tags.Contains(tag)))
				{
					commonTags.Remove(tag);
				}
			}

			if (firstTd == null)
			{
				return;
			}
			
			DlgTodoMultiItemEditor dlgTodoMultiItemEditor = new DlgTodoMultiItemEditor(firstTd.TD, TabNames, commonTags);
			dlgTodoMultiItemEditor.ShowDialog();
			if (!dlgTodoMultiItemEditor.Result)
			{
				return;
			}
			
			List<string> tagsToRemove = commonTags.Where(tag => !dlgTodoMultiItemEditor.ResultTags.Contains(tag)).ToList();

			foreach (TodoItemHolder itemHolder in lb.SelectedItems)
			{
				if(dlgTodoMultiItemEditor.ChangeTag)
				{
					foreach (string tag in tagsToRemove)
					{
						itemHolder.TD.Tags.Remove(tag);
					}

					foreach (string tag in dlgTodoMultiItemEditor.ResultTags.Where(tag => !itemHolder.TD.Tags.Contains(tag.ToUpper())))
					{
						itemHolder.TD.Tags.Add(tag.ToUpper());
					}
				}

				if (dlgTodoMultiItemEditor.ChangeRank)
				{
					itemHolder.TD.Rank = dlgTodoMultiItemEditor.ResultTD.Rank;
				}

				if (dlgTodoMultiItemEditor.ChangeSev)
				{
					itemHolder.TD.Severity = dlgTodoMultiItemEditor.ResultTD.Severity;
				}

				if (dlgTodoMultiItemEditor.ResultIsComplete && dlgTodoMultiItemEditor.ChangeComplete)
				{
					itemHolder.TD.IsComplete = true;
				}

				if (!dlgTodoMultiItemEditor.ChangeTodo)
				{
					continue;
				}
				itemHolder.TD.Todo += Environment.NewLine + dlgTodoMultiItemEditor.ResultTD.Todo;
				foreach (string tag in dlgTodoMultiItemEditor.ResultTD.Tags.Where(tag => !itemHolder.TD.Tags.Contains(tag)))
				{
					itemHolder.TD.Tags.Add(tag);
				}
			}
			RefreshTodo();
			KanbanRefresh();
		}
		private void TimeTakenTimer_OnClick(object sender, EventArgs e)
		{
			if (sender is Label l)
			{
				if (l.DataContext is TodoItemHolder itemHolder)
				{
					itemHolder.TD.IsTimerOn = !itemHolder.TD.IsTimerOn;
				}
				AutoSave();
			}

			_lbIncompleteItems.Items.Refresh();
		}
		
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// POMO STUFF //
		private void PomoTimerToggle_OnClick(object sender, EventArgs e)
		{
			_isPomoTimerOn = !_isPomoTimerOn;
		}
		// private void PomoTimerPause_OnClick(object sender, EventArgs e)
		// {
		// 	_isPomoTimerOn = false;
		// }
		private void PomoTimerReset_OnClick(object sender, EventArgs e)
		{
			_isPomoTimerOn = true;
			_pomoTimer = DateTime.MinValue;
			PomoTimeLeft = 0;
		}
//		private void PomoWorkInc_OnClick(object sender, EventArgs e)
//		{
//			int value = Convert.ToInt16((string) (sender as Button)?.CommandParameter);
//			PomoWorkTime += value;
//			lblPomoWork.Content = _pomoWorkTime.ToString();
//		}
		private void PomoWork_OnValueChanged(object sender, EventArgs e)
		{
			if (iudPomoWork.Value != null)
			{
				PomoWorkTime = (int)iudPomoWork.Value;
			}
		}
//		private void PomoWorkDec_OnClick(object sender, EventArgs e)
//		{
//			int value = Convert.ToInt16((string) (sender as Button)?.CommandParameter);
//			PomoWorkTime -= value;
//			if (PomoWorkTime <= 0)
//				PomoWorkTime = value;
//			lblPomoWork.Content = _pomoWorkTime.ToString();
//		}
//		private void PomoBreakInc_OnClick(object sender, EventArgs e)
//		{
//			int value = Convert.ToInt16((string) (sender as Button)?.CommandParameter);
//			PomoBreakTime += value;
//			lblPomoBreak.Content = _pomoBreakTime.ToString();
//		}
//		private void PomoBreakDec_OnClick(object sender, EventArgs e)
//		{
//			int value = Convert.ToInt16((string) (sender as Button)?.CommandParameter);
//			PomoBreakTime -= value;
//			if (PomoBreakTime <= 0)
//				PomoBreakTime = value;
//			lblPomoBreak.Content = _pomoBreakTime.ToString();
//		}
		private void PomoBreak_OnValueChanged(object sender, EventArgs e)
		{
			if (iudPomoBreak.Value != null)
			{
				PomoBreakTime = (int)iudPomoBreak.Value;
			}
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// TO DOs //

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Sorting //

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// FileIO //
		private string GetFilePath()
		{
			string result = "";
			if (RecentFiles.Count == 0)
			{
				return BASE_PATH;
			}

			string[] sa = RecentFiles[0].Split('\\');
			for (int i = 0; i < sa.Length - 1; i++)
			{
				result += sa[i] + "\\";
			}
			return result;
		}
		private string GetFileName()
		{
			if (RecentFiles.Count == 0)
			{
				return "";
			}

			string[] sa = RecentFiles[0].Split('\\');
			return sa[sa.Length - 1];
		}
		private void Load(string path)
		{
			SortRecentFiles(path);
			SaveSettings();

			StreamReader stream = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

			ClearLists();

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
			{
				version = 2.0f;
			}

			// Here's where versions are loaded
			if (version <= 2.0f)
			{
				Load2_0SaveFile(stream, line);
			}
			else if (version > 2.0f)
			{
				Load2_1SaveFile(stream, line);
			}
			
			stream.Close();

			RefreshTodo();
			KanbanRefresh();
			RefreshHistory();
			todoTabs.Items.Refresh();
			kanbanTabs.Items.Refresh();
			if (HistoryItems.Count > 0)
			{
				lbHistory.SelectedIndex = 0;
				_currentHistoryItem = HistoryItems[0];
			}
			else
			{
				_currentHistoryItem = new HistoryItem("", "");
			}
			
			_currentOpenFile = path;
			Title = WindowTitle;

			_isChanged = false;
			_currentOpenFile = path;

			if (!_currentHistoryItem.HasBeenCopied)
			{
				return;
			}
			DlgYesNo dlgYesNo = new DlgYesNo("New History", "Start a new History Item?");
			dlgYesNo.ShowDialog();
			if (dlgYesNo.Result)
			{
				AddNewHistoryItem();
			}
		}
		private void ClearLists()
		{
			_incompleteItems.Clear();
			_masterList.Clear();
			_hashTags.Clear();
			_tabHash.Clear();
			_incompleteItemsTabsList.Clear();
			HistoryItems.Clear();
			_hashShortcuts.Clear();
		}
		private void Load2_0SaveFile(TextReader stream, string line)
		{
			IncompleteItemsAddNewTab("All");
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
				IncompleteItemsAddNewTab(line, false);
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
				switch (line)
				{
					case "NewVCS":
						history = new List<string>();
						line = stream.ReadLine();
						continue;
					case "EndVCS":
						HistoryItems.Add(new HistoryItem(history));
						line = stream.ReadLine();
						continue;
					default:
						history.Add(line);
						line = stream.ReadLine();
						break;
				}
			}
		}
		private void Load2_1SaveFile(TextReader stream, string line)
		{
			while (line != null)
			{
				line = stream.ReadLine();
				if (line != null && line.Contains("=====TABS"))
					continue;
				if (line != null && line.Contains("=====FILESETTINGS"))
					break;
			
				IncompleteItemsAddNewTab(line, false);
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
				{
					break;
				}

				if (line != null && line.Contains("=====TODO"))
				{
					continue;
				}

				TodoItem td = new TodoItem(line);
				AddItemToMasterList(td);
			}

			List<string> history = new List<string>();
			while (line != null)
			{
				line = stream.ReadLine();
				switch (line)
				{
					case "NewVCS":
						history = new List<string>();
						continue;
					case "EndVCS":
						HistoryItems.Add(new HistoryItem(history));
						continue;
					default:
						history.Add(line);
						break;
				}
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
			{
				Save(RecentFiles[0]);
			}
		}
		private bool NewFile()
		{
			const string newFileName = "EToDo.txt";
			if (newFileName == null) throw new ArgumentNullException(nameof(newFileName));
			SaveFileDialog sfd = new SaveFileDialog
			{
				Title = @"Select folder to save file in.",
				FileName = newFileName,
				InitialDirectory = GetFilePath(),
				Filter = @"txt files (*.txt)|*.txt|All files (*.*)|*.*"
			};

			DialogResult dr = sfd.ShowDialog();

			if (dr != System.Windows.Forms.DialogResult.OK)
			{
				return false;
			}
			
			SortRecentFiles(sfd.FileName);
			SaveSettings();
			return true;
		}
		private void SaveAs()
		{
			SaveFileDialog sfd = new SaveFileDialog
			{
				Title = @"Select folder to save file in.",
				FileName = GetFileName(),
				InitialDirectory = GetFilePath(),
				Filter = @"txt files (*.txt)|*.txt|All files (*.*)|*.*"
			};

			DialogResult dr = sfd.ShowDialog();

			if (dr != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}
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
			stream.WriteLine(PROGRAM_VERSION);

			stream.WriteLine("====================================TABS");
			foreach (TabItem ti in _incompleteItemsTabsList)
			{
				stream.WriteLine(ti.Name);
			}
			
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
			{
				stream.WriteLine(td.ToString());
			}

			stream.WriteLine("====================================VCS");
			foreach (HistoryItem hi in HistoryItems)
			{
				stream.Write(hi.ToString());
			}

			stream.Close();
		}
		private void BackupSave()
		{
			if (!_autoBackup || !_doBackup)
			{
				return;
			}
			
			string path = RecentFiles[0] + ".bak" + _backupIncrement;
			_backupIncrement++;
			_backupIncrement = _backupIncrement > 9 ? 0 : _backupIncrement;
			SaveFile(path);
			_doBackup = false;
		}
	
		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Settings //
		private void LoadSettings()
		{
			RecentFiles = new ObservableCollection<string>();
			float version = 0.0f;
			
			const string filePath = BASE_PATH + "TDHistory.settings";
			if (!File.Exists(filePath))
			{
				SaveSettings();
			}
			
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
					RecentFiles = new ObservableCollection<string>();
					
					DlgYesNo  dlgYesNo = new DlgYesNo("Corrupted file", "Error with the settings file, create a new one?");
					dlgYesNo.ShowDialog();
					if(dlgYesNo.Result)
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

			// Here's where versions are loaded
			if (version <= 2.0)
			{
				LoadV2_0Settings(stream, line);
			}
			else if (version > 2.0)
			{
				LoadV2_1Settings(stream, line);
			}
			
			stream.Close();

			if (RecentFiles.Count != 0)
			{
				return;
			}
			dlg = new DlgYesNo("New file created");
			dlg.ShowDialog();
		}
		private void LoadV2_0Settings(TextReader stream, string line)
		{
			while (line != null)
			{
				if (line == "RECENTFILES" || line == "")
				{
					continue;
				}

				if (line == "WINDOWPOSITION")
				{
					break;
				}
					
				RecentFiles.Add(line);
				line = stream.ReadLine();
			}
			
			_top = Convert.ToDouble(stream.ReadLine());
			_left = Convert.ToDouble(stream.ReadLine());
			_height = Convert.ToDouble(stream.ReadLine());
			_width = Convert.ToDouble(stream.ReadLine());
		}
		private void LoadV2_1Settings(TextReader stream, string line)
		{
			while (line != null)
			{
				line = stream.ReadLine();
				if (line == "RECENTFILES" || line == "")
				{
					continue;
				}

				if (line == "WINDOWPOSITION")
				{
					break;
				}
					
				RecentFiles.Add(line);
			}
			
			_top = Convert.ToDouble(stream.ReadLine());
			_left = Convert.ToDouble(stream.ReadLine());
			_height = Convert.ToDouble(stream.ReadLine());
			_width = Convert.ToDouble(stream.ReadLine());
			stream.ReadLine();
			_pomoWorkTime = Convert.ToInt16(stream.ReadLine());
			_pomoBreakTime = Convert.ToInt16(stream.ReadLine());
			stream.ReadLine();
			_globalHotkeys = Convert.ToBoolean(stream.ReadLine());
		}
		private void SaveSettings()
		{
			const string filePath = BASE_PATH + "TDHistory.settings";
			StreamWriter stream = new StreamWriter(File.Open(filePath, FileMode.Create));

			stream.WriteLine("VERSION");
			stream.WriteLine(PROGRAM_VERSION);
			
			stream.WriteLine("RECENTFILES");
			foreach (string s in RecentFiles)
			{
				if (s == "")
				{
					continue;
				}
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
			if (RecentFiles.Contains(recent))
			{
				RecentFiles.Remove(recent);
			}

			RecentFiles.Insert(0, recent);

			while (RecentFiles.Count >= 10)
			{
				RecentFiles.RemoveAt(RecentFiles.Count - 1);
			}
		}
	}
}
