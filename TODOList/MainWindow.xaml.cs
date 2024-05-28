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
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using Clipboard = System.Windows.Forms.Clipboard;
using ComboBox = System.Windows.Controls.ComboBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Label = System.Windows.Controls.Label;
using ListBox = System.Windows.Controls.ListBox;
using MenuItem = System.Windows.Controls.MenuItem;
using TabControl = System.Windows.Controls.TabControl;
using TextBox = System.Windows.Controls.TextBox;


namespace TODOList
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        // FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
        private const string PROGRAM_VERSION = "3.40.16.0";
        public const string DATE_STRING_FORMAT = "yyyyMMdd";
        public const string TIME_STRING_FORMAT = "HHmmss";
        private const string GIT_EXE_PATH = "C:\\Program Files\\Git\\cmd\\";

        private const int MAIN_LIST_BOX_PANEL_WIDTH = 3;
        private const int NOTES_PANEL_WIDTH = 1;
        private const int NEW_TODO_PANEL_HEIGHT = 100;
        private const int TOP_OF_PANEL_STUFF_HEIGHT = 110;


        private bool _skipUpdate;

        private readonly List<TabItem> _incompleteItemsTabsList;

        private readonly List<TabItem> _kanbanTabsList;

        private TabControl _currentSelectedSubTab;
        private readonly List<TodoItem> _masterList;

        private readonly List<List<TodoItemHolder>> _incompleteItems;
        private readonly List<List<TodoItemHolder>> _kanbanItems;
        private List<List<TodoItemHolder>> _currentItems;

        private readonly List<List<string>> _incompleteItemsHashTags;

        private readonly List<List<string>> _kanbanHashTags;

        private TodoItem _currentTodoItemInNotesPanel;
        private int _currentTodoItemInNotesPanelIndex = -1;

        private readonly List<string> _kanbanTabHeaders;
        private readonly List<string> _tabHash;
        private static Dictionary<string, string> _hashShortcuts;
        private List<string> _prevHashTagList = new List<string>();

        private int _previousMainTabSelectedIndex;
        private int _previousTodoTabSelectedIndex;
        private int _previousKanbanTabSelectedIndex;

        private string _errorMessage = string.Empty;

        // Sorting
        private bool _reverseSort;
        private string _currentSort = "rank";
        private int _currentHashTagSortIndex = -1;
        private bool _didHashChange;
        private string _hashToSortBy = "";
        private bool _hashSortSelected;

        private int _currentSeverity;
        private readonly List<string> _notesPanelHashTags;
        private ListBox _lbNotesPanelHashTags;

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
        private int _previousSessionLastActiveTab;

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
        private ComboBox _cbIncompleteItemsHashTags;
        private ComboBox _cbKanbanHashTags;
        private ListBox _lbIncompleteItems;
        private ListBox _lbKanbanItems;
        private ListBox _lbCurrentItems;
        private TextBox _tbNewTodo;
        private ComboBox _cbSeverity;

        private int _versionA;
        private int _versionB;
        private int _versionC;
        private int _versionD;

        private double _windowWidth;
        private double _windowHeight;

        // PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
        private int VersionA
        {
            get => _versionA;
            set
            {
                _versionA = value;
                iudVersionA.Value = value;
            }
        }
        private int VersionB
        {
            get => _versionB;
            set
            {
                _versionB = value;
                iudVersionB.Value = value;
            }
        }
        private int VersionC
        {
            get => _versionC;
            set
            {
                _versionC = value;
                iudVersionC.Value = value;
            }
        }
        private int VersionD
        {
            get => _versionD;
            set
            {
                _versionD = value;
                iudVersionD.Value = value;
            }
        }

        private bool _isUpdatingCheckBoxes;
        private string _testIfChanged;

        private ObservableCollection<string> RecentFiles { get; set; }
        private List<TodoItemHolder> IncompleteItems => _incompleteItems[incompleteItemsTodoTabs.SelectedIndex];
        private List<TodoItemHolder> KanbanItems => _kanbanItems[kanbanTodoTabs.SelectedIndex];
        private List<string> KanbanHashTags => _kanbanHashTags[kanbanTodoTabs.SelectedIndex];
        private List<string> IncompleteItemsHashTags => _incompleteItemsHashTags[incompleteItemsTodoTabs.SelectedIndex];

        private string TabNames => incompleteItemsTodoTabs.SelectedIndex == -1
            ? _incompleteItemsTabsList[0].Name
            : _incompleteItemsTabsList[incompleteItemsTodoTabs.SelectedIndex].Name;

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
        public int PomoTimeLeft
        {
            get => _pomoTimeLeft;
            set
            {
                _pomoTimeLeft = value;
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

        // CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
        public MainWindow()
        {
            _left = 0;
            InitializeComponent();
            Closing += Window_Closed;
            this.SizeChanged += Window_OnWindowSizeChanged;

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
            _kanbanTabHeaders = new List<string>();
            _incompleteItemsHashTags = new List<List<string>>();
            _kanbanHashTags = new List<List<string>>();
            _tabHash = new List<string>();
            _hashShortcuts = new Dictionary<string, string>();
            HistoryItems = new List<HistoryItem>();
            _currentHistoryItem = new HistoryItem("", "");

            incompleteItemsTodoTabs.ItemsSource = _incompleteItemsTabsList;
            incompleteItemsTodoTabs.Items.Refresh();
            kanbanTodoTabs.ItemsSource = _kanbanTabsList;
            kanbanTodoTabs.Items.Refresh();
            mnuRecentLoads.ItemsSource = RecentFiles;
            lbHistory.ItemsSource = HistoryItems;

            lbHistory.SelectedIndex = 0;
            _currentHistoryItemIndex = 0;
            lbCompletedTodos.SelectedIndex = 0;
            _notesPanelHashTags = new List<string>();

            //			lblPomoWork.Content = _pomoWorkTime.ToString();
            //			lblPomoBreak.Content = _pomoBreakTime.ToString();

            KanbanCreateTabs();
            _timeUntilBackup = _backupTime;
        }

        // METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Windows METHODS //
        private void Window_OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowWidth = e.NewSize.Width;
            _windowHeight = e.NewSize.Height;

            int mainPanelDivisions = MAIN_LIST_BOX_PANEL_WIDTH + NOTES_PANEL_WIDTH;
            double mainGridWidth = Math.Floor(_windowWidth / mainPanelDivisions * MAIN_LIST_BOX_PANEL_WIDTH);
            double notesPanelWidth = _windowWidth - mainGridWidth;
            incompleteItemsMainGrid.Width = mainGridWidth > 0 ? mainGridWidth : 1;
            kanbanMainGrid.Width = mainGridWidth > 0 ? mainGridWidth : 1;
            incompleteItemsNotesPanel.Width = notesPanelWidth > 0 ? notesPanelWidth : 1;
            kanbanNotesPanel.Width = notesPanelWidth > 0 ? notesPanelWidth : 1;

            double todoTabsHeight = _windowHeight - NEW_TODO_PANEL_HEIGHT - TOP_OF_PANEL_STUFF_HEIGHT;
            incompleteItemsTodoTabs.Height = todoTabsHeight > 0 ? todoTabsHeight : 1;
            kanbanTodoTabs.Height = todoTabsHeight > 0 ? todoTabsHeight : 1;
            incompleteItemsNewTodoPanel.Height = NEW_TODO_PANEL_HEIGHT;
            kanbanNewTodoPanel.Height = NEW_TODO_PANEL_HEIGHT;
            if (_lbIncompleteItems != null)
                _lbIncompleteItems.Height = todoTabsHeight > 0 ? todoTabsHeight : 1;
            if (_lbKanbanItems != null)
                _lbKanbanItems.Height = todoTabsHeight > 0 ? todoTabsHeight : 1;

            cbIncompleteItemsSeverity.Width = 100;
            cbKanbanSeverity.Width = 100;
            tbIncompleteItemsNewTodo.Width = (mainGridWidth - 100) > 0 ? (mainGridWidth - 100) : 1;
            tbKanbanNewTodo.Width = (mainGridWidth - 100) > 0 ? (mainGridWidth - 100) : 1;

            double notesPanelHeight = _windowHeight - TOP_OF_PANEL_STUFF_HEIGHT;
            notesPanelWidth -= 30;
            int numLabels = 5;
            double labelsHeight = numLabels * 25;
            double notesPanelTitleHeight = 65;
            double notesPanelHashTagsHeight = 335;
            double notesPanelCompleteButtonHeight = 35;
            double notesPanelTextBoxSpaceTotal = notesPanelHeight - labelsHeight - notesPanelTitleHeight - notesPanelHashTagsHeight - notesPanelCompleteButtonHeight;
            double notesPanelTextBoxDivision = notesPanelTextBoxSpaceTotal / 12;
            double notesPanelNotesHeight = Math.Floor(notesPanelTextBoxDivision * 3);
            double notesPanelProblemHeight = Math.Floor(notesPanelTextBoxDivision * 2);
            double notesPanelSolutionHeight = notesPanelTextBoxSpaceTotal - notesPanelNotesHeight - notesPanelProblemHeight;
            tbIncompleteItemsTitle.Height = notesPanelTitleHeight > 0 ? notesPanelTitleHeight : 1;
            tbIncompleteItemsTitle.Width = notesPanelWidth > 0 ? notesPanelWidth : 1;
            tbKanbanTitle.Height = notesPanelTitleHeight > 0 ? notesPanelTitleHeight : 1;
            tbKanbanTitle.Width = notesPanelWidth > 0 ? notesPanelWidth : 1;
            tbIncompleteItemsNotes.Height = notesPanelNotesHeight > 0 ? notesPanelNotesHeight : 1;
            tbIncompleteItemsNotes.Width = notesPanelWidth > 0 ? notesPanelWidth : 1;
            tbKanbanNotes.Height = notesPanelNotesHeight > 0 ? notesPanelNotesHeight : 1;
            tbKanbanNotes.Width = notesPanelWidth > 0 ? notesPanelWidth : 1;
            tbIncompleteItemsProblem.Height = notesPanelProblemHeight > 0 ? notesPanelProblemHeight : 1;
            tbIncompleteItemsProblem.Width = notesPanelWidth > 0 ? notesPanelWidth : 1;
            tbKanbanProblem.Height = notesPanelProblemHeight > 0 ? notesPanelProblemHeight : 1;
            tbKanbanProblem.Width = notesPanelWidth > 0 ? notesPanelWidth : 1;
            tbIncompleteItemsSolution.Height = notesPanelSolutionHeight > 0 ? notesPanelSolutionHeight : 1;
            tbIncompleteItemsSolution.Width = notesPanelWidth > 0 ? notesPanelWidth : 1;
            tbKanbanSolution.Width = notesPanelWidth > 0 ? notesPanelWidth : 1;
            tbKanbanSolution.Height = notesPanelSolutionHeight > 0 ? notesPanelSolutionHeight : 1;
            lbKanbanHashTags.Width = notesPanelWidth > 0 ? notesPanelWidth : 1;
            lbIncompleteItemsHashTags.Width = notesPanelWidth > 0 ? notesPanelWidth : 1;
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
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == 0x73)
                            {
                                Activate();
                                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(tbHNotes), tbHNotes);
                                _tbNewTodo.Focus();
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
            if (dlg.Result)
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
        private static string UpperFirstLetter(string s)
        {
            string result = "";
            for (int i = 0; i < s.Length; i++)
            {
                if (i == 0)
                    result += s[i].ToString().ToUpper();
                else
                    result += s[i];
            }

            return result;
        }
        private void OnLoaded(object sender, EventArgs e)
        {
            SelectActiveTabItems();
            DelayedStartupLoad();
        }

        // METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Tabs //
        private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl.SelectedIndex == _previousMainTabSelectedIndex ||
                e.RemovedItems.Count <= 0 ||
                !(e.RemovedItems[0] is TabItem)) return;

            switch (_previousMainTabSelectedIndex)
            {
                case 1: //"TODOs":
                    UpdateNotes(_lbIncompleteItems, tbIncompleteItemsNotes);
                    UpdateTitle(_lbIncompleteItems, tbIncompleteItemsTitle);
                    UpdateProblem(_lbIncompleteItems, tbIncompleteItemsProblem);
                    UpdateSolution(_lbIncompleteItems, tbIncompleteItemsSolution);
                    break;
                case 2: //"Kanban":
                    UpdateNotes(_lbKanbanItems, tbKanbanNotes);
                    UpdateTitle(_lbKanbanItems, tbKanbanTitle);
                    UpdateProblem(_lbKanbanItems, tbKanbanProblem);
                    UpdateSolution(_lbKanbanItems, tbKanbanSolution);
                    break;
            }

            _previousMainTabSelectedIndex = tabControl.SelectedIndex;
            SelectActiveTabItems();
        }

        // Get Active Items
        private void SelectActiveTabItems()
        {
            _lbCurrentItems = null;
            _currentItems = null;
            _currentSelectedSubTab = null;
            _lbNotesPanelHashTags = null;

            int tabIndex = tabControl.SelectedIndex;
            switch (tabIndex)
            {
                case 0:
                    // History Tab
                    break;
                case 1:
                    // IncompleteItems (TO DO) Tab
                    IncompleteItemsUpdateHandler();
                    _currentSelectedSubTab = incompleteItemsTodoTabs;
                    _lbCurrentItems = _lbIncompleteItems;
                    _currentItems = _incompleteItems;
                    _tbNewTodo = tbIncompleteItemsNewTodo;
                    _cbSeverity = cbIncompleteItemsSeverity;
                    _lbNotesPanelHashTags = lbIncompleteItemsHashTags;
                    break;
                case 2:
                    // Kanban Tab
                    KanbanUpdateHandler();
                    _currentSelectedSubTab = kanbanTodoTabs;
                    _lbCurrentItems = _lbKanbanItems;
                    _currentItems = _kanbanItems;
                    _tbNewTodo = tbKanbanNewTodo;
                    _cbSeverity = cbKanbanSeverity;
                    _lbNotesPanelHashTags = lbKanbanHashTags;
                    break;
                case 3:
                    // Log Tab
                    break;
            }
        }
        private List<TodoItemHolder> GetActiveItemList()
        {
            switch (tabControl.SelectedIndex)
            {
                case 1:
                    return IncompleteItems;
                case 2:
                    return KanbanItems;
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
                        IncompleteItemsInitialize();
                    return _lbIncompleteItems;
                case 2:
                    if (_lbKanbanItems == null)
                        KanbanInitialize();
                    return _lbKanbanItems;
                default:
                    _errorMessage = "Function: GetActiveListBox()\n" +
                                    "\tSelectedIndex: " + tabControl.SelectedIndex + "\n" +
                                    "\tNot a valid tab with a ListBox\n" +
                                    _errorMessage;
                    return null;
            }
        }
        private TextBox GetActiveTextBoxTitle()
        {
            switch (tabControl.SelectedIndex)
            {
                case 1:
                    return tbIncompleteItemsTitle;
                case 2:
                    return tbKanbanTitle;
                default:
                    _errorMessage = "Function: GetActiveTextBox()\n" +
                                    "\tSelectedIndex: " + tabControl.SelectedIndex + "\n" +
                                    "\tNot a valid tab with a Title textbox\n" +
                                    _errorMessage;
                    return null;
            }
        }
        private TextBox GetActiveTextBoxNotes()
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
        private TextBox GetActiveTextBoxProblem()
        {
            switch (tabControl.SelectedIndex)
            {
                case 1:
                    return tbIncompleteItemsProblem;
                case 2:
                    return tbKanbanProblem;
                default:
                    _errorMessage = "Function: GetActiveTextBoxProblem()\n" +
                                    "\tSelectedIndex: " + tabControl.SelectedIndex + "\n" +
                                    "\tNot a valid tab with a Problem textbox\n" +
                                    _errorMessage;
                    return null;
            }
        }
        private TextBox GetActiveTextBoxSolution()
        {
            switch (tabControl.SelectedIndex)
            {
                case 1:
                    return tbIncompleteItemsSolution;
                case 2:
                    return tbKanbanSolution;
                default:
                    _errorMessage = "Function: GetActiveTextBoxSolution()\n" +
                                    "\tSelectedIndex: " + tabControl.SelectedIndex + "\n" +
                                    "\tNot a valid tab with a Solution textbox\n" +
                                    _errorMessage;
                    return null;
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
                td.TimeTaken = td.TimeTaken.AddSeconds(1);

            foreach (TodoItemHolder itemHolder in from list in _incompleteItems from itemHolder in list where itemHolder.TD.IsTimerOn select itemHolder)
                itemHolder.TimeTaken = itemHolder.TD.TimeTaken;

            lblPomo.Content = $"{_pomoTimer.Ticks / TimeSpan.TicksPerMinute:00}:{_pomoTimer.Second:00}";
            if (_isPomoTimerOn)
            {
                pbPomo.Background = Brushes.Maroon;
                _pomoTimer = _pomoTimer.AddSeconds(1);

                if (_isPomoWorkTimerOn)
                {
                    long ticks = _pomoWorkTime * TimeSpan.TicksPerMinute;
                    PomoTimeLeft = (int)((float)_pomoTimer.Ticks / ticks * 100);
                    pbPomo.Background = Brushes.DarkGreen;
                    if (_pomoTimer.Ticks < ticks)
                        return;

                    _isPomoWorkTimerOn = false;
                    _pomoTimer = DateTime.MinValue;
                }
                else
                {
                    long ticks = _pomoBreakTime * TimeSpan.TicksPerMinute;
                    PomoTimeLeft = (int)((float)(ticks - _pomoTimer.Ticks) / ticks * 100);
                    if (_pomoTimer.Ticks < ticks)
                        return;

                    _isPomoWorkTimerOn = true;
                    _pomoTimer = DateTime.MinValue;
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
            string args = "/c call \"" + GIT_EXE_PATH + "git\" --git-dir=\"" + gitPath + "\\.git\" log > \"" + _historyLogPath + "\"";
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
                if (line.Split(' ')[0] == "commit")
                    log.Add("=====================================================================================" + Environment.NewLine);

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
            if (dir == null)
                return null;
            while (true)
            {
                if (dir == Directory.GetDirectoryRoot(dir))
                    return null;

                List<string> dirs = Directory.GetDirectories(dir).ToList();
                if (dirs.Any(s => s.Contains(".git")))
                    return dir;

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
                    leftover += name[i];

                if (!_hashShortcuts.ContainsKey(hashShortcut))
                    return hashShortcut;

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
                        t = "#FEATURE";

                    if (t.Equals("#BUGS"))
                        t = "#BUG";

                    foreach (string hash in from pair in _hashShortcuts where t.Equals("#" + pair.Key.ToUpper()) select "#" + pair.Value)
                        s = hash;

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

        // METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// IncompleteItems //
        private void IncompleteItemsTabs_OnLoaded(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            IncompleteItemsUpdateHandler();
            SelectActiveTabItems();
        }
        private void IncompleteItemsInitialize()
        {
            Application.Current.Dispatcher.Invoke(
                async () =>
                {
                    if (!(incompleteItemsTodoTabs.Template.FindName("PART_SelectedContentHost", incompleteItemsTodoTabs) is ContentPresenter incompleteItemsContentPresenter))
                        return;

                    incompleteItemsContentPresenter.ApplyTemplate();
                    if (incompleteItemsContentPresenter.ContentTemplate == null)
                        return;
                    _lbIncompleteItems = incompleteItemsContentPresenter.ContentTemplate.FindName("lbIncompleteItems", incompleteItemsContentPresenter) as ListBox;
                    if (_lbIncompleteItems == null)
                        return;

                    IncompleteItemsSortToTabs();
                    _lbIncompleteItems.ItemsSource = IncompleteItems;
                    _lbIncompleteItems.Items.Refresh();
                    _lbIncompleteItems.SelectionChanged += IncompleteItems_OnSelectionChanged;
                    _lbIncompleteItems.UnselectAll();
                    double height = _windowHeight - NEW_TODO_PANEL_HEIGHT - TOP_OF_PANEL_STUFF_HEIGHT;
                    _lbIncompleteItems.Height = height > 0 ? height : 1;
                }, DispatcherPriority.ApplicationIdle);
        }
        private void IncompleteItemsUpdateHandler()
        {
            if (_lbIncompleteItems == null)
                IncompleteItemsInitialize();
            if (_cbIncompleteItemsHashTags == null)
                IncompleteItemsHashTagsInitialize();
            if (_lbIncompleteItems == null || _cbIncompleteItemsHashTags == null)
                return;

            if (incompleteItemsTodoTabs.Items.Count <= 0)
                return;
            if (incompleteItemsTodoTabs.SelectedIndex < 0)
                incompleteItemsTodoTabs.SelectedIndex = 0;

            IncompleteItemsRefresh();
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
            _incompleteItemsHashTags.Add(new List<string>());
            _incompleteItemsTabsList.Add(ti);
            if (!doSave)
            {
                return;
            }

            incompleteItemsTodoTabs.ItemsSource = _incompleteItemsTabsList;
            incompleteItemsTodoTabs.Items.Refresh();
            AutoSave();
        }
        private void IncompleteItemsTab_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            if (_lbIncompleteItems != null && _previousTodoTabSelectedIndex != -1)
            {
                NotesPanelUpdate();
                IncompleteItemsRefresh();
                RefreshNotes();
            }

            Dispatcher.BeginInvoke(new Action(IncompleteItemsUpdateHandler));
            _previousTodoTabSelectedIndex = incompleteItemsTodoTabs.SelectedIndex;
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
            e.Handled = true;

            List<TodoItem> list = IncompleteItems.Select(itemHolder => itemHolder.TD).ToList();
            if (_lbIncompleteItems.SelectedIndex < 0)
            {
                if (_currentTodoItemInNotesPanelIndex < _lbIncompleteItems.Items.Count && _currentTodoItemInNotesPanelIndex != -1)
                    _lbIncompleteItems.SelectedItem = _lbIncompleteItems.Items.GetItemAt(_currentTodoItemInNotesPanelIndex);
                return;
            }

            _currentTodoItemInNotesPanelIndex = _lbIncompleteItems.SelectedIndex;
            _currentTodoItemInNotesPanel = list[_currentTodoItemInNotesPanelIndex];
            tbIncompleteItemsNotes.Text = _currentTodoItemInNotesPanel.Notes.Replace("/n", Environment.NewLine);
            tbIncompleteItemsTitle.Text = _currentTodoItemInNotesPanel.Todo;
            tbIncompleteItemsProblem.Text = _currentTodoItemInNotesPanel.Problem;
            tbIncompleteItemsSolution.Text = _currentTodoItemInNotesPanel.Solution;
            NotesPanelLoadHashTags();
        }
        private void IncompleteItemsHashTags_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            if (IncompleteItemsHashTags.Count == 0)
                return;

            _hashToSortBy = IncompleteItemsHashTags[0];
            if (_cbIncompleteItemsHashTags.SelectedItem != null)
                _hashToSortBy = _cbIncompleteItemsHashTags.SelectedItem.ToString();

            _hashSortSelected = true;
            _currentSort = "hash";
            IncompleteItemsRefresh();
        }
        private void IncompleteItemsSortToTabs()
        {
            List<TodoItem> incompleteItems = _masterList.ToList();
            foreach (TodoItem td in incompleteItems)
            {
                bool sortedToTab = false;

                TodoItemHolder itemHolder = new TodoItemHolder(td);
                _incompleteItems[0].Add(itemHolder);

                foreach (int index in from hash in _tabHash where td.Tags.Contains(hash) select _tabHash.IndexOf(hash) + 1)
                {
                    _incompleteItems[index].Add(itemHolder);
                    sortedToTab = true;
                }

                if (sortedToTab)
                    continue;

                if (_incompleteItems.Count > 1)
                    _incompleteItems[1].Add(itemHolder);
            }
        }
        private void IncompleteItemsRefresh()
        {
            if (_skipUpdate)
            {
                _skipUpdate = false;
                return;
            }

            for (int i = 0; i < _incompleteItems.Count; i++)
            {
                _incompleteItems[i].Clear();
                _incompleteItemsHashTags[i].Clear();
            }

            SortCompleteTodosToHistory();
            IncompleteItemsSortToTabs();
            SortHashTagLists(_incompleteItems, _incompleteItemsHashTags);
            IncompleteItemsCountTabItems();
            CheckForHashTagListChanges();
            IncompleteItemsFixRankings();

            int tabIndex = incompleteItemsTodoTabs.SelectedIndex;
            if (tabIndex < 0 || tabIndex >= _incompleteItemsTabsList.Count)
                return;

            SortLists(_incompleteItems, _incompleteItemsHashTags, tabIndex);

            if (_lbIncompleteItems != null)
            {
                _lbIncompleteItems.ItemsSource = IncompleteItems;
                _lbIncompleteItems.Items.Refresh();
            }

            if (_cbIncompleteItemsHashTags != null)
            {
                _cbIncompleteItemsHashTags.ItemsSource = IncompleteItemsHashTags;
                _cbIncompleteItemsHashTags.Items.Refresh();
            }
        }
        private void IncompleteItemsHashTagsInitialize()
        {
            if (!(incompleteItemsTodoTabs.Template.FindName("PART_SelectedContentHost", incompleteItemsTodoTabs) is ContentPresenter hashTagsContentPresenter))
                return;

            _cbIncompleteItemsHashTags = hashTagsContentPresenter.ContentTemplate.FindName("cbIncompleteItemsHashTags", hashTagsContentPresenter) as ComboBox;
            if (_cbIncompleteItemsHashTags == null)
                return;

            _cbIncompleteItemsHashTags.ItemsSource = IncompleteItemsHashTags;
            _cbIncompleteItemsHashTags.Items.Refresh();
        }
        private void IncompleteItemsCountTabItems()
        {
            for (int i = 1; i < _incompleteItemsTabsList.Count; i++)
                _incompleteItemsTabsList[i].Header = _tabHash[i - 1] + " " + _incompleteItems[i].Count;

            _incompleteItemsTabsList[0].Header = "All " + _incompleteItems[0].Count;
        }
        private void IncompleteItemsFixRankings()
        {
            if (incompleteItemsTodoTabs.Items.Count == 0 ||
                incompleteItemsTodoTabs.SelectedIndex < 0 ||
                incompleteItemsTodoTabs.SelectedIndex >= incompleteItemsTodoTabs.Items.Count)
                return;

            string currentHash = _incompleteItemsTabsList[incompleteItemsTodoTabs.SelectedIndex].Name;
            foreach (TodoItemHolder itemHolder in _incompleteItems[incompleteItemsTodoTabs.SelectedIndex].Where(itemHolder => !itemHolder.TD.Rank.ContainsKey(currentHash)))
                itemHolder.TD.Rank.Add(currentHash, 99);

            _incompleteItems[incompleteItemsTodoTabs.SelectedIndex] = _incompleteItems[incompleteItemsTodoTabs.SelectedIndex].OrderBy(o => o.TD.Rank[currentHash]).ToList();
            for (int rank = 0; rank < _incompleteItems[incompleteItemsTodoTabs.SelectedIndex].Count; rank++)
            {
                _incompleteItems[incompleteItemsTodoTabs.SelectedIndex][rank].TD.Rank[currentHash] = rank + 1;
                _incompleteItems[incompleteItemsTodoTabs.SelectedIndex][rank].Rank = _incompleteItems[incompleteItemsTodoTabs.SelectedIndex][rank].TD.Rank[currentHash];
            }
        }

        // METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Kanban //
        private void KanbanTabs_OnLoaded(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            KanbanUpdateHandler();
            SelectActiveTabItems();
        }
        private void KanbanInitialize()
        {
            Application.Current.Dispatcher.Invoke(
                async () =>
                {
                    if (!(kanbanTodoTabs.Template.FindName("PART_SelectedContentHost", kanbanTodoTabs) is ContentPresenter kanbanContentPresenter))
                        return;
                    kanbanContentPresenter.ApplyTemplate();
                    _lbKanbanItems = kanbanContentPresenter.ContentTemplate.FindName("lbKanbanItems", kanbanContentPresenter) as ListBox;
                    if (_lbKanbanItems == null)
                        return;

                    KanbanSortToTabs();
                    _lbKanbanItems.ItemsSource = KanbanItems;
                    _lbKanbanItems.Items.Refresh();
                    _lbKanbanItems.SelectionChanged += KanbanItems_OnSelectionChange;
                    _lbKanbanItems.UnselectAll();
                    double height = _windowHeight - NEW_TODO_PANEL_HEIGHT - TOP_OF_PANEL_STUFF_HEIGHT;
                    _lbKanbanItems.Height = height > 0 ? height : 1;
                }, DispatcherPriority.ApplicationIdle);
        }
        private void KanbanUpdateHandler()
        {
            if (_lbKanbanItems == null)
                KanbanInitialize();
            if (_cbKanbanHashTags == null)
                KanbanHashTagsInitialize();
            if (_lbKanbanItems == null || _cbKanbanHashTags == null)
                return;

            if (kanbanTodoTabs.Items.Count <= 0)
                return;
            if (kanbanTodoTabs.SelectedIndex < 0)
                kanbanTodoTabs.SelectedIndex = 0;

            KanbanRefresh();
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

            _kanbanHashTags.Add(new List<string>());
            _kanbanTabsList.Add(ti);
            _kanbanItems.Add(new List<TodoItemHolder>());
            kanbanTodoTabs.Items.Refresh();
        }
        private void KanbanTab_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            if (_lbKanbanItems != null && _previousKanbanTabSelectedIndex != -1)
            {
                UpdateNotes(_lbKanbanItems, tbKanbanNotes);
                UpdateTitle(_lbKanbanItems, tbKanbanTitle);
                UpdateProblem(_lbKanbanItems, tbKanbanProblem);
                UpdateSolution(_lbKanbanItems, tbKanbanSolution);

                NotesPanelLoadHashTags();
                KanbanRefresh();
                RefreshNotes();
            }

            Dispatcher.BeginInvoke(new Action(KanbanUpdateHandler));
            _previousKanbanTabSelectedIndex = kanbanTodoTabs.SelectedIndex;
        }
        private void KanbanCreateTabs()
        {
            _kanbanTabHeaders.Add("None");
            _kanbanTabHeaders.Add("Backlog");
            _kanbanTabHeaders.Add("Next");
            _kanbanTabHeaders.Add("Current");

            foreach (string s in _kanbanTabHeaders)
                KanbanAddNewTab(s);
        }
        private void KanbanItems_OnSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            if (kanbanTodoTabs.SelectedIndex < 0 || kanbanTodoTabs.SelectedIndex >= _kanbanItems.Count)
                kanbanTodoTabs.SelectedIndex = 0;

            List<TodoItem> list = KanbanItems.Select(itemHolder => itemHolder.TD).ToList();
            if (_lbKanbanItems.SelectedIndex < 0)
            {
                if (_currentTodoItemInNotesPanelIndex < _lbKanbanItems.Items.Count && _currentTodoItemInNotesPanelIndex != -1)
                    _lbKanbanItems.SelectedItem = _lbKanbanItems.Items.GetItemAt(_currentTodoItemInNotesPanelIndex);
                return;
            }

            _currentTodoItemInNotesPanelIndex = _lbKanbanItems.SelectedIndex;
            _currentTodoItemInNotesPanel = list[_currentTodoItemInNotesPanelIndex];
            tbKanbanNotes.Text = _currentTodoItemInNotesPanel.Notes.Replace("/n", Environment.NewLine);
            tbKanbanTitle.Text = _currentTodoItemInNotesPanel.Todo;
            tbKanbanProblem.Text = _currentTodoItemInNotesPanel.Problem;
            tbKanbanSolution.Text = _currentTodoItemInNotesPanel.Solution;
            NotesPanelLoadHashTags();
        }
        private void KanbanHashTags_OnSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            if (KanbanHashTags.Count == 0)
                return;

            _hashToSortBy = KanbanHashTags[0];
            if (_cbKanbanHashTags.SelectedItem != null)
                _hashToSortBy = _cbKanbanHashTags.SelectedItem.ToString();

            _hashSortSelected = true;
            _currentSort = "hash";
            KanbanRefresh();
        }
        private void KanbanSortToTabs()
        {
            List<TodoItem> kanbanItems = _masterList.ToList();

            foreach (TodoItem todoItem in kanbanItems)
            {
                int kanbanIndex = todoItem.Kanban;
                _kanbanItems[kanbanIndex].Add(new TodoItemHolder(todoItem));
            }
        }
        private void KanbanRefresh()
        {
            if (tabControl.SelectedIndex != 2)
                return;
            if (_skipUpdate)
            {
                _skipUpdate = false;
                return;
            }

            for (int i = 0; i < _kanbanItems.Count; i++)
            {
                _kanbanItems[i].Clear();
                _kanbanHashTags[i].Clear();
            }

            SortCompleteTodosToHistory();
            KanbanSortToTabs();
            SortHashTagLists(_kanbanItems, _kanbanHashTags);
            KanbanCountTabItems();
            KanbanFixRankings();

            int tabIndex = kanbanTodoTabs.SelectedIndex;
            if (tabIndex < 0 || tabIndex >= _kanbanTabsList.Count)
                return;

            SortLists(_kanbanItems, _kanbanHashTags, tabIndex);

            if (_lbKanbanItems != null)
            {
                _lbKanbanItems.ItemsSource = KanbanItems;
                _lbKanbanItems.Items.Refresh();
            }

            if (_cbKanbanHashTags != null)
            {
                _cbKanbanHashTags.ItemsSource = KanbanHashTags;
                _cbKanbanHashTags.Items.Refresh();
            }
        }
        private void KanbanHashTagsInitialize()
        {
            if (!(kanbanTodoTabs.Template.FindName("PART_SelectedContentHost", kanbanTodoTabs) is ContentPresenter hashTagsKanbanContentPresenter))
                return;

            _cbKanbanHashTags = hashTagsKanbanContentPresenter.ContentTemplate.FindName("cbKanbanHashTags", hashTagsKanbanContentPresenter) as ComboBox;
            if (_cbKanbanHashTags == null)
                return;

            _cbKanbanHashTags.ItemsSource = KanbanHashTags;
            _cbKanbanHashTags.Items.Refresh();
        }
        private void KanbanCountTabItems()
        {
            for (int i = 0; i < _kanbanTabsList.Count; i++)
                _kanbanTabsList[i].Header = _kanbanTabHeaders[i] + " " + _kanbanItems[i].Count;
        }
        private void KanbanFixRankings()
        {
            if (_currentItems == null || _currentSelectedSubTab == null)
                return;
            if (_currentSelectedSubTab.SelectedIndex < 0)
                return;
            List<TodoItemHolder> currentList = _currentItems[_currentSelectedSubTab.SelectedIndex].OrderBy(o => o.TD.KanbanRank).ToList();
            int rank = 1;
            foreach (TodoItemHolder todoItemHolder in currentList)
            {
                todoItemHolder.KanbanRank = rank;
                rank++;
            }
        }

        // METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Hotkeys //
        private void HkSwitchTab(object sender, ExecutedRoutedEventArgs e)
        {
            int index = incompleteItemsTodoTabs.SelectedIndex;
            switch ((string)e.Parameter)
            {
                case "right" when tabControl.SelectedIndex == 0:
                    tabControl.SelectedIndex = 1;
                    return;
                case "right":
                {
                    index++;
                    if (index >= incompleteItemsTodoTabs.Items.Count)
                        index = incompleteItemsTodoTabs.Items.Count - 1;

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

            incompleteItemsTodoTabs.SelectedIndex = index;
        }
        private void HkSwitchSeverity(object sender, ExecutedRoutedEventArgs e)
        {
            int index = _cbSeverity.SelectedIndex;
            switch ((string)e.Parameter)
            {
                case "down":
                {
                    index++;
                    if (index >= _cbSeverity.Items.Count)
                        index = _cbSeverity.Items.Count - 1;

                    break;
                }
                case "up":
                {
                    index--;
                    if (index < 0)
                        index = 0;

                    break;
                }
            }

            _cbSeverity.SelectedIndex = index;
            _cbSeverity.Items.Refresh();
        }
        private void HkComplete(object sender, ExecutedRoutedEventArgs e)
        {
            if (tabHistory.IsSelected)
                return;

            if (_tbNewTodo.IsFocused)
            {
                QuickComplete();
                return;
            }

            if (GetActiveTextBoxTitle().IsFocused ||
                GetActiveTextBoxNotes().IsFocused ||
                GetActiveTextBoxProblem().IsFocused ||
                GetActiveTextBoxSolution().IsFocused)
            {
                TodoComplete();
                return;
            }

            TodoItemHolder itemHolder = (TodoItemHolder)_lbIncompleteItems.SelectedItem;
            if (itemHolder != null)
            {
                DlgTodoItemEditor dlgTodoItemEditor = new DlgTodoItemEditor(itemHolder.TD, TabNames);
                dlgTodoItemEditor.ShowDialog();
                if (dlgTodoItemEditor.Result)
                    AddItemToMasterList(dlgTodoItemEditor.ResultTodoItem);
            }

            IncompleteItemsRefresh();
            KanbanRefresh();
        }
        private void HkAdd(object sender, ExecutedRoutedEventArgs e)
        {
            if (tabHistory.IsSelected)
                return;

            Add_OnClick(sender, e);
        }
        private void HkEdit(object sender, EventArgs e)
        {
            if (_tbNewTodo.IsFocused)
            {
                Add_OnClick(sender, e);
                return;
            }

            ListBox lb = null;
            List<TodoItem> list = new List<TodoItem>();

            if (tabTodos.IsSelected)
            {
                lb = _lbIncompleteItems;
                list.AddRange(IncompleteItems.Select(itemHolder => itemHolder.TD));
            }
            else if (tabKanban.IsSelected)
            {
                lb = _lbKanbanItems;
                list.AddRange(KanbanItems.Select(itemHolder => itemHolder.TD));
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
                Load(RecentFiles[1]);
        }
        private void HkStartStopTimer(object sender, ExecutedRoutedEventArgs e)
        {
            if (!tabTodos.IsSelected)
                return;

            int index = _lbIncompleteItems.SelectedIndex;
            IncompleteItems[index].TD.IsTimerOn = !IncompleteItems[index].TD.IsTimerOn;
            _lbIncompleteItems.Items.Refresh();
        }
        private void QuickComplete()
        {
            TodoItem newTodo = new TodoItem
            {
                Todo = _tbNewTodo.Text,
                Severity = _currentSeverity,
                IsComplete = true
            };
            switch (tabControl.SelectedIndex)
            {
                case 1:
                    newTodo.Rank[TabNames] = IncompleteItems.Count;
                    newTodo.Tags.Add("#" + TabNames);
                    if (newTodo.Severity == 3)
                        newTodo.Rank[TabNames] = 0;
                    break;
                case 2:
                    newTodo.Kanban = kanbanTodoTabs.SelectedIndex;
                    newTodo.KanbanRank = _lbKanbanItems.Items.Count;
                    break;
            }

            ExpandHashTags(newTodo);
            AddItemToMasterList(newTodo);
            AutoSave();
            IncompleteItemsRefresh();
            KanbanRefresh();
            _tbNewTodo.Clear();
        }

        // METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// MenuCommands //
        // Main FILE menu
        private void mnuNew_OnClick(object sender, EventArgs e)
        {
            AutoSave();
            DlgYesNo dlg = new DlgYesNo("New file", "Are you sure?");
            dlg.ShowDialog();

            if (!dlg.Result)
                return;
            if (!NewFile())
                return;

            ClearLists();
            IncompleteItemsCreateTabs();
            ConvertProjectVersion("0.0.0.0");

            _currentHistoryItem = new HistoryItem("", "");
            AddNewHistoryItem();
            RefreshHistory();
            IncompleteItemsRefresh();
            KanbanRefresh();

            _currentOpenFile = "";
            Title = WindowTitle;
            Save(RecentFiles[0]);
        }
        private void mnuLoadFiles_OnClick(object sender, RoutedEventArgs e)
        {
            if (_recentFilesIndex < 0)
                return;

            MenuItem mi = (MenuItem)e.OriginalSource;
            if (!(mi.DataContext is string path))
                return;

            if (_isChanged)
            {
                DlgYesNo dlgYesNo = new DlgYesNo("Close", "Maybe save first?");
                dlgYesNo.ShowDialog();
                if (dlgYesNo.Result)
                    Save(_currentOpenFile);
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
                return;

            if (_isChanged)
            {
                DlgYesNo dlgYesNo = new DlgYesNo("Close", "Maybe save first?");
                dlgYesNo.ShowDialog();
                if (dlgYesNo.Result)
                    Save(_currentOpenFile);
            }

            Load(openFileDialog.FileName);
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
        private void mnuSaveAs_OnClick(object sender, EventArgs e)
        {
            SaveAs();
        }
        private void mnuOptions_OnClick(object sender, EventArgs e)
        {
            DlgOptions options = new DlgOptions(_autoSave, _globalHotkeys, _autoBackup, _backupTime);
            options.ShowDialog();
            if (!options.Result)
                return;

            _autoSave = options.AutoSave;
            _globalHotkeys = options.GlobalHotkeys;
            _autoBackup = options.AutoBackup;
            _backupTime = options.BackupTime;

            GlobalHotkeysToggle();
            AutoSave();
        }
        private void mnuQuit_OnClick(object sender, EventArgs e)
        {
            Close();
        }
        private void mnuHelp_OnClick(object sender, EventArgs e)
        {
            DlgHelp dlgH = new DlgHelp();
            dlgH.ShowDialog();
        }

        // RECENT FILES menu
        private void mnuRecentLoads_OnRMBUp(object sender, MouseButtonEventArgs e)
        {
            _recentFilesIndex = -1;
            if (!(e.OriginalSource is TextBlock mi))
                return;

            string path = (string)mi.DataContext;
            _recentFilesIndex = mnuRecentLoads.Items.IndexOf(path);
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

        // Incomplete Items Tabs Context menu
        private void mnuEditTabs_OnClick(object sender, EventArgs e)
        {
            List<TabItem> list = _incompleteItemsTabsList.ToList();
            DlgEditTabs rt = new DlgEditTabs(list);
            rt.ShowDialog();
            if (!rt.Result)
                return;

            _incompleteItemsTabsList.Clear();
            _hashShortcuts.Clear();
            _tabHash.Clear();
            _incompleteItemsHashTags.Clear();
            _incompleteItems.Clear();
            foreach (string s in rt.ResultList)
            {
                if (rt.ResultList.IndexOf(s) < rt.ResultList.Count - 1)
                    IncompleteItemsAddNewTab(s, false);
                else
                    IncompleteItemsAddNewTab(s);
            }

            IncompleteItemsRefresh();
            IncompleteItemsInitialize();

            foreach (TodoItem td in _masterList)
                CleanTodoHashRanks(td);
        }

        // History Item Context menus
        private void mnuResetHistoryCopied_OnClick(object sender, EventArgs e)
        {
            int index = lbHistory.SelectedIndex;
            HistoryItems[index].ResetCopied();
        }
        private void mnuEditHistoryTodo_OnClick(object sender, EventArgs e)
        {
            if (lbCompletedTodos.IsMouseOver)
                EditItem(lbCompletedTodos, _currentHistoryItem.CompletedTodos);
            else if (lbCompletedTodosFeatures.IsMouseOver)
                EditItem(lbCompletedTodosFeatures, _currentHistoryItem.CompletedTodosFeatures);
            else if (lbCompletedTodosBugs.IsMouseOver)
                EditItem(lbCompletedTodosBugs, _currentHistoryItem.CompletedTodosBugs);

            RefreshHistory();
        }

        // IncompleteItems / Kanban Context Menus
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

            string command = menuItem.CommandParameter.ToString();
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
                case "MoveUp":
                    mnuMoveItemToTop_OnClick();
                    break;
                case "MoveDown":
                    mnuMoveItemToBottom_OnClick();
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
            IncompleteItemsRefresh();
            KanbanRefresh();
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
            IncompleteItemsRefresh();
            KanbanRefresh();
            RefreshNotes();
        }
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
        private void mnuKanban_OnClick(int kanbanRank)
        {
            if (_lbCurrentItems.SelectedItems.Count >= 1)
                foreach (TodoItemHolder todo in _lbCurrentItems.SelectedItems)
                    todo.Kanban = kanbanRank;

            IncompleteItemsRefresh();
            KanbanRefresh();
        }
        private void mnuMoveItemToTop_OnClick()
        {
            TodoItem todoItem = GetSelectedTodo();
            if (todoItem == null)
            {
                _errorMessage = "\nFunction: mnuMoveItemToTop_OnClick()" +
                                "\n\n" + _errorMessage;
                return;
            }

            switch (tabControl.SelectedIndex)
            {
                case 1:
                    todoItem.Rank[_incompleteItemsTabsList[incompleteItemsTodoTabs.SelectedIndex].Name] = 0;
                    break;
                case 2:
                    todoItem.KanbanRank = 0;
                    break;
            }
        }
        private void mnuMoveItemToBottom_OnClick()
        {
            TodoItem todoItem = GetSelectedTodo();
            if (todoItem == null)
            {
                _errorMessage = "\nFunction: mnuMoveItemToBottom_OnClick()" +
                                "\n\n" + _errorMessage;
                return;
            }

            switch (tabControl.SelectedIndex)
            {
                case 1:
                    todoItem.Rank[_incompleteItemsTabsList[incompleteItemsTodoTabs.SelectedIndex].Name] = _lbIncompleteItems.Items.Count + 1;
                    break;
                case 2:
                    todoItem.KanbanRank = _lbKanbanItems.Items.Count + 1;
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
                    if (_lbIncompleteItems == null)
                    {
                        _errorMessage = "\t_lbIncompleteItems == null";
                        break;
                    }

                    selectedIndex = _lbIncompleteItems.SelectedIndex;
                    if (selectedIndex < 0)
                    {
                        _errorMessage = "\t_lbIncompleteItems.SelectedIndex == -1";
                        break;
                    }

                    if (selectedIndex >= _lbIncompleteItems.Items.Count)
                    {
                        _errorMessage = "\t_lbIncompleteItems.SelectedIndex out of range";
                        break;
                    }

                    if (selectedIndex >= IncompleteItems.Count)
                    {
                        _errorMessage = "\tIncompleteItems[SelectedIndex] out of range";
                        break;
                    }

                    todoItemHolder = IncompleteItems[selectedIndex];
                    break;
                case 2:
                    tabName = "KanbanItems";
                    if (_lbKanbanItems == null)
                    {
                        _errorMessage = "\t_lbKanbanItems == null";
                        break;
                    }

                    selectedIndex = _lbKanbanItems.SelectedIndex;
                    if (selectedIndex < 0)
                    {
                        _errorMessage = "\t_lbKanbanItems.SelectedIndex == -1";
                        break;
                    }

                    if (selectedIndex >= _lbKanbanItems.Items.Count)
                    {
                        _errorMessage = "\t_lbKanbanItems.SelectedIndex out of range";
                        break;
                    }

                    if (selectedIndex >= KanbanItems.Count)
                    {
                        _errorMessage = "\tKanbanItems[SelectedIndex] out of range";
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
        private void SortCompleteTodosToHistory()
        {
            List<TodoItem> list = _masterList.ToList();
            foreach (var todoItem in list.Where(todoItem => todoItem.IsComplete))
            {
                RemoveItemFromMasterList(todoItem);
                AddTodoToHistory(todoItem);
            }
        }
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

            if (_currentHistoryItemIndex >= HistoryItems.Count) _currentHistoryItemIndex = 0;

            if (_currentHistoryItemIndex < 0) _currentHistoryItemIndex = HistoryItems.Count - 1;

            if (HistoryItems.Count == 0)
            {
                _currentHistoryItem = new HistoryItem("", "");
                return;
            }

            if (prevIndex == _currentHistoryItemIndex) return;

            _currentHistoryItem = lbHistory.Items[_currentHistoryItemIndex] as HistoryItem;
            if (_currentHistoryItem != null) lblHTotalTime.Content = _currentHistoryItem.TotalTime;

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
            if (_didMouseSelect) _currentHistoryItemIndex = lbHistory.SelectedIndex;

            if (lbHistory.Items.Count == 0) return;

            if (_currentHistoryItemIndex >= lbHistory.Items.Count) _currentHistoryItemIndex = lbHistory.Items.Count - 1;

            if (lbHistory.Items[_currentHistoryItemIndex] is HistoryItem hi)
            {
                _currentHistoryItem = hi;
                if (_currentHistoryItem != null) lblHTotalTime.Content = _currentHistoryItem.TotalTime;
            }

            RefreshHistory();
            _didMouseSelect = false;
        }
        private void AddTodoToHistory(TodoItem td)
        {
            if (_currentHistoryItem.DateAdded == "") AddNewHistoryItem();

            RefreshHistory();
            _currentHistoryItem = HistoryItems[0];
            _currentHistoryItem.AddCompletedTodo(td);
            RefreshHistory();
            AutoSave();
        }
        private void AddNewHistoryItem()
        {
            UpdateCurrentVersion();

            _currentHistoryItem = new HistoryItem(DateTime.Now)
            {
                Title = "v" + MakeCurrentVersion() + " "
            };
            HistoryItems.Add(_currentHistoryItem);
            AutoSave();
            RefreshHistory();
        }
        private void RefreshHistory()
        {
            List<HistoryItem> temp = HistoryItems.OrderByDescending(o => o.DateTimeAdded).ToList();
            HistoryItems.Clear();
            foreach (HistoryItem hi in temp) HistoryItems.Add(hi);

            if (HistoryItems.Count == 0) _currentHistoryItem = new HistoryItem("", "");

            if (HistoryItems.Count > 0 && _currentHistoryItem.DateAdded == "") lbHistory.SelectedIndex = 0;

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
            iudVersionA.Value = VersionA;
            iudVersionB.Value = VersionB;
            iudVersionC.Value = VersionC;
            iudVersionD.Value = VersionD;
        }
        private void DeleteHistory_OnClick(object sender, EventArgs e)
        {
            if (HistoryItems.Count == 0) return;

            DlgYesNo dlgYesNo = new DlgYesNo("Delete", "Are you sure you want to delete this History Item?");
            dlgYesNo.ShowDialog();
            if (!dlgYesNo.Result) return;

            HistoryItems.Remove(_currentHistoryItem);

            _currentHistoryItem = HistoryItems.Count > 0 ? HistoryItems[0] : new HistoryItem("", "");
            AutoSave();
            RefreshHistory();
        }
        private void CopyHistory_OnClick(object sender, EventArgs e)
        {
            Button b = sender as Button;
            HistoryItem hi = (HistoryItem)b?.DataContext;
            if (hi == null) return;

            int totalTime = HistoryItems.Sum(hist => Convert.ToInt32(hist.TotalTime));
            string time = $"{totalTime / 60:00} : {totalTime % 60:00}";
            Clipboard.SetText(hi.ToClipboard(time));
            hi.SetCopied();
            if (lbHistory.Items.IndexOf(hi) != 0) return;

            DlgYesNo dlgYesNo = new DlgYesNo("New History", "Add a new History Item?");
            dlgYesNo.ShowDialog();
            if (dlgYesNo.Result) AddNewHistoryItem();
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
        private void DeleteTodo_OnClick(object sender, EventArgs e)
        {
            Button b = sender as Button;
            TodoItem td = b?.DataContext as TodoItem;
            DlgYesNo dlgYesNo = new DlgYesNo("Delete", "Are you sure you want to delete this Todo?");
            dlgYesNo.ShowDialog();
            if (!dlgYesNo.Result) return;

            if (td != null)
            {
                td.IsComplete = false;
                AddItemToMasterList(td);
                IncompleteItemsRefresh();
                KanbanRefresh();
                if (_currentHistoryItem.CompletedTodos.Contains(td))
                    _currentHistoryItem.CompletedTodos.Remove(td);
                else if (_currentHistoryItem.CompletedTodosBugs.Contains(td))
                    _currentHistoryItem.CompletedTodosBugs.Remove(td);
                else if (_currentHistoryItem.CompletedTodosFeatures.Contains(td))
                    _currentHistoryItem.CompletedTodosFeatures.Remove(td);

                AutoSave();
            }

            RefreshHistory();
        }

        // METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// TODOS //
        private void AddNewTagsToTodo()
        {
            int index = _currentTodoItemInNotesPanelIndex;
            _currentTodoItemInNotesPanel.Tags.Clear();
            foreach (TagHolder tagHolder in _lbNotesPanelHashTags.Items)
            {
                _currentTodoItemInNotesPanel.Tags.Add(tagHolder.Text.ToUpper());
            }

            NotesPanelLoadHashTags();
            IncompleteItemsRefresh();
            KanbanRefresh();
            _lbCurrentItems.SelectedItem = _lbCurrentItems.Items.GetItemAt(index);
        }
        private void Severity_OnClick(object sender, EventArgs e)
        {
            if (!(sender is Button b)) return;

            if (b.DataContext is TodoItemHolder itemHolder)
            {
                int value = itemHolder.Severity;
                if (value == 3) value = 0;
                else value++;

                itemHolder.Severity = value;
            }

            IncompleteItemsRefresh();
            KanbanRefresh();
        }
        private void SeverityComboBox_OnSelectionChange(object sender, EventArgs e)
        {
            if (!(sender is ComboBox cb)) return;

            int index = cb.SelectedIndex;
            _currentSeverity = index;
        }
        private void SeverityComboBox_OnIsLoaded(object sender, EventArgs e)
        {
            if (sender is ComboBox cb) cb.SelectedIndex = _currentSeverity;
        }
        private void Add_OnClick(object sender, EventArgs e)
        {
            string name = TabNames;
            int newIndex = 0;
            TodoItem td = new TodoItem() { Todo = _tbNewTodo.Text, Severity = _currentSeverity };
            switch (tabControl.SelectedIndex)
            {
                case 1:
                    td.Tags.Add("#" + name);
                    newIndex = _lbIncompleteItems.Items.Count + 1;
                    td.Rank[TabNames] = newIndex;
                    if (td.Severity == 3)
                        td.Rank[TabNames] = 0;
                    break;
                case 2:
                    td.Kanban = kanbanTodoTabs.SelectedIndex;
                    newIndex = _lbKanbanItems.Items.Count + 1;
                    td.KanbanRank = newIndex;
                    break;
            }

            ExpandHashTags(td);
            AddItemToMasterList(td);
            AutoSave();
            IncompleteItemsRefresh();
            KanbanRefresh();
            _tbNewTodo.Clear();
            RefreshNotes(newIndex - 1);
        }
        private void RankAdjust_OnClick(object sender, EventArgs e)
        {
            if (!(sender is Button b))
                return;
            TodoItemHolder todoItemHolder = b.DataContext as TodoItemHolder;
            List<TodoItemHolder> todoItemHolderList = _currentItems[_currentSelectedSubTab.SelectedIndex];

            if (todoItemHolderList.Count == 0)
                return;

            int index = todoItemHolderList.IndexOf(todoItemHolder);
            switch ((string)b.CommandParameter)
            {
                case "up" when index == 0:
                    return;
                case "up":
                    RankAdjustUp(todoItemHolder, todoItemHolderList, index);
                    break;
                case "down" when index >= todoItemHolderList.Count - 1:
                    return;
                case "down":
                    RankAdjustDown(todoItemHolder, todoItemHolderList, index);
                    break;
            }

            IncompleteItemsRefresh();
            KanbanRefresh();
        }
        private void RankAdjustUp(TodoItemHolder todoItemHolder, IReadOnlyList<TodoItemHolder> todoItemHolderList, int index)
        {
            if (todoItemHolder == null) return;

            int newRank;
            switch (tabControl.SelectedIndex)
            {
                case 1:
                    newRank = todoItemHolderList[index - 1].TD.Rank[TabNames];
                    todoItemHolderList[index - 1].TD.Rank[TabNames] = todoItemHolder.Rank;
                    todoItemHolder.TD.Rank[TabNames] = newRank;
                    break;
                case 2:
                    newRank = todoItemHolderList[index - 1].TD.KanbanRank;
                    todoItemHolderList[index - 1].TD.KanbanRank = todoItemHolder.KanbanRank;
                    todoItemHolder.TD.KanbanRank = newRank;
                    break;
            }

            AutoSave();
        }
        private void RankAdjustDown(TodoItemHolder todoItemHolder, IReadOnlyList<TodoItemHolder> todoItemHolderList, int index)
        {
            if (todoItemHolder == null) return;

            int newRank;
            switch (tabControl.SelectedIndex)
            {
                case 1:
                    newRank = todoItemHolderList[index + 1].TD.Rank[TabNames];
                    todoItemHolderList[index + 1].TD.Rank[TabNames] = todoItemHolder.Rank;
                    todoItemHolder.TD.Rank[TabNames] = newRank;
                    break;
                case 2:
                    newRank = todoItemHolderList[index + 1].TD.KanbanRank;
                    todoItemHolderList[index + 1].TD.KanbanRank = todoItemHolder.KanbanRank;
                    todoItemHolder.TD.KanbanRank = newRank;
                    break;
            }

            AutoSave();
        }
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
        private void EditItem(Selector lb, IReadOnlyList<TodoItem> list)
        {
            int index = lb.SelectedIndex;
            if (index < 0)
                return;

            TodoItem td = list[index];
            DlgTodoItemEditor itemEditor = new DlgTodoItemEditor(td, TabNames);

            itemEditor.ShowDialog();
            if (itemEditor.Result)
            {
                RemoveItemFromMasterList(td);
                if (_currentHistoryItem.CompletedTodos.Contains(td))
                    _currentHistoryItem.CompletedTodos.Remove(td);

                if (_currentHistoryItem.CompletedTodosBugs.Contains(td))
                    _currentHistoryItem.CompletedTodosBugs.Remove(td);

                if (_currentHistoryItem.CompletedTodosFeatures.Contains(td))
                    _currentHistoryItem.CompletedTodosFeatures.Remove(td);

                AddItemToMasterList(itemEditor.ResultTodoItem);
                AutoSave();
            }

            IncompleteItemsRefresh();
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
                foreach (string tag in itemHolder.TD.Tags)
                {
                    if (!tags.Contains(tag))
                        tags.Add(tag);
                    else if (!commonTagsTemp.Contains(tag))
                        commonTagsTemp.Add(tag);
                }
            }

            List<string> commonTags = commonTagsTemp.ToList();
            foreach (TodoItemHolder itemHolder in lb.SelectedItems)
            foreach (string tag in commonTagsTemp.Where(tag => !itemHolder.TD.Tags.Contains(tag)))
                commonTags.Remove(tag);

            if (firstTd == null)
                return;

            DlgTodoMultiItemEditor dlgTodoMultiItemEditor = new DlgTodoMultiItemEditor(firstTd.TD, TabNames, commonTags);
            dlgTodoMultiItemEditor.ShowDialog();
            if (!dlgTodoMultiItemEditor.Result)
                return;

            List<string> tagsToRemove = commonTags.Where(tag => !dlgTodoMultiItemEditor.ResultTags.Contains(tag)).ToList();

            foreach (TodoItemHolder itemHolder in lb.SelectedItems)
            {
                if (dlgTodoMultiItemEditor.ChangeTag)
                {
                    foreach (string tag in tagsToRemove)
                        itemHolder.TD.Tags.Remove(tag);
                    foreach (string tag in dlgTodoMultiItemEditor.ResultTags.Where(tag => !itemHolder.TD.Tags.Contains(tag.ToUpper())))
                        itemHolder.TD.Tags.Add(tag.ToUpper());
                }

                if (dlgTodoMultiItemEditor.ChangeRank)
                    itemHolder.TD.Rank = dlgTodoMultiItemEditor.ResultTD.Rank;
                if (dlgTodoMultiItemEditor.ChangeSev)
                    itemHolder.TD.Severity = dlgTodoMultiItemEditor.ResultTD.Severity;
                if (dlgTodoMultiItemEditor.ResultIsComplete && dlgTodoMultiItemEditor.ChangeComplete)
                    itemHolder.TD.IsComplete = true;
                if (!dlgTodoMultiItemEditor.ChangeTodo)
                    continue;

                itemHolder.TD.Todo += Environment.NewLine + dlgTodoMultiItemEditor.ResultTD.Todo;
                foreach (string tag in dlgTodoMultiItemEditor.ResultTD.Tags.Where(tag => !itemHolder.TD.Tags.Contains(tag)))
                    itemHolder.TD.Tags.Add(tag);
            }

            IncompleteItemsRefresh();
            KanbanRefresh();
        }
        private void TimeTakenTimer_OnClick(object sender, EventArgs e)
        {
            if (sender is Label l)
            {
                if (l.DataContext is TodoItemHolder itemHolder)
                    itemHolder.TD.IsTimerOn = !itemHolder.TD.IsTimerOn;
                AutoSave();
            }

            _lbCurrentItems.Items.Refresh();
        }

        // METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Sorting //
        private void Sort_OnClick(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (_currentSort != (string)b?.CommandParameter)
            {
                _reverseSort = false;
                _currentSort = (string)b?.CommandParameter;
            }

            if ((string)b?.CommandParameter == "hash")
            {
                List<string> hashTagsList;
                switch (tabControl.SelectedIndex)
                {
                    case 1:
                        hashTagsList = IncompleteItemsHashTags;
                        break;
                    case 2:
                        hashTagsList = KanbanHashTags;
                        break;
                    default:
                        return;
                }

                if (hashTagsList.Count == 0)
                    return;

                _currentHashTagSortIndex++;
                if (_currentHashTagSortIndex >= hashTagsList.Count)
                    _currentHashTagSortIndex = 0;
            }

            _reverseSort = !_reverseSort;
            IncompleteItemsRefresh();
            KanbanRefresh();
        }
        private List<TodoItemHolder> SortByHashTag(List<TodoItemHolder> list, List<string> hashTags)
        {
            if (_didHashChange)
                _currentHashTagSortIndex = 0;

            List<TodoItemHolder> incompleteItems = new List<TodoItemHolder>();
            List<string> sortedHashTags = new List<string>();

            if (hashTags.Count == 0)
                return list;

            if (_hashSortSelected)
            {
                _currentHashTagSortIndex = 0;
                foreach (string unused in hashTags.TakeWhile(s => !s.Equals(_hashToSortBy)))
                    _currentHashTagSortIndex++;
            }

            for (int i = 0 + _currentHashTagSortIndex; i < hashTags.Count; i++)
                sortedHashTags.Add(hashTags[i]);

            for (int i = 0; i < _currentHashTagSortIndex; i++)
                sortedHashTags.Add(hashTags[i]);

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

                    foreach (string unused in itemHolder.TD.Tags.Where(t => s.Equals(t)))
                    {
                        incompleteItems.Add(itemHolder);
                        list.Remove(itemHolder);
                    }
                }
            }

            incompleteItems.AddRange(list);

            return incompleteItems;
        }
        private void CheckForHashTagListChanges()
        {
            if (tabControl.SelectedIndex != 1)
                return;

            _didHashChange = false;
            if (_incompleteItemsHashTags[0].Count != _prevHashTagList.Count)
            {
                _didHashChange = true;
            }
            else
            {
                for (int i = 0; i < _incompleteItemsHashTags[0].Count; i++)
                {
                    if (_incompleteItemsHashTags[0][i] != _prevHashTagList[i])
                        _didHashChange = true;
                }
            }

            _prevHashTagList = _incompleteItemsHashTags[0].ToList();
        }
        private void SortHashTagLists(List<List<TodoItemHolder>> todoItemHolderList, List<List<string>> hashTagList)
        {
            for (int i = 0; i < todoItemHolderList.Count; i++)
            {
                foreach (string tag in todoItemHolderList[i].SelectMany(itemHolder => itemHolder.TD.Tags))
                {
                    if (!hashTagList[i].Contains(tag))
                        hashTagList[i].Add(tag);
                    hashTagList[i] = hashTagList[i].OrderBy(o => o).ToList();
                }
            }
        }
        private void SortLists(List<List<TodoItemHolder>> list, List<List<string>> hashTagsList, int tabIndex)
        {
            switch (_currentSort)
            {
                case "severity":
                    list[tabIndex] = _reverseSort
                        ? list[tabIndex].OrderByDescending(o => o.Severity).ToList()
                        : list[tabIndex].OrderBy(o => o.Severity).ToList();
                    break;
                case "date":
                    list[tabIndex] = _reverseSort
                        ? list[tabIndex].OrderByDescending(o => o.TimeStarted).ToList()
                        : list[tabIndex].OrderBy(o => o.TimeStarted).ToList();
                    list[tabIndex] = _reverseSort
                        ? list[tabIndex].OrderByDescending(o => o.DateStarted).ToList()
                        : list[tabIndex].OrderBy(o => o.DateStarted).ToList();
                    break;
                case "hash":
                    list[tabIndex] = SortByHashTag(list[tabIndex], hashTagsList[tabIndex]);
                    break;
                case "rank":
                    switch (tabControl.SelectedIndex)
                    {
                        case 1:
                            list[tabIndex] = _reverseSort
                                ? list[tabIndex].OrderByDescending(o => o.TD.Rank[TabNames]).ToList()
                                : list[tabIndex].OrderBy(o => o.TD.Rank[TabNames]).ToList();
                            break;
                        case 2:
                            list[tabIndex] = _reverseSort
                                ? list[tabIndex].OrderByDescending(o => o.TD.KanbanRank).ToList()
                                : list[tabIndex].OrderBy(o => o.TD.KanbanRank).ToList();
                            break;
                    }

                    break;
                case "kanban":
                    list[tabIndex] = _reverseSort
                        ? list[tabIndex].OrderByDescending(o => o.TD.Kanban).ToList()
                        : list[tabIndex].OrderBy(o => o.TD.Kanban).ToList();
                    break;
                case "active":
                    list[tabIndex] = _reverseSort
                        ? list[tabIndex].OrderByDescending(o => o.TimeTaken).ToList()
                        : list[tabIndex].OrderBy(o => o.TimeTaken).ToList();
                    list[tabIndex] = _reverseSort
                        ? list[tabIndex].OrderByDescending(o => o.IsTimerOn).ToList()
                        : list[tabIndex].OrderBy(o => o.IsTimerOn).ToList();
                    break;
            }
        }
        private void CleanTodoHashRanks(TodoItem td)
        {
            List<string> tabNames = _incompleteItemsTabsList.Select(ti => ti.Name).ToList();
            List<string> remove = (from pair in td.Rank where !tabNames.Contains(pair.Key) select pair.Key).ToList();
            foreach (string hash in remove)
                td.Rank.Remove(hash);
            foreach (string name in tabNames.Where(name => !td.Rank.ContainsKey(name)))
                td.Rank.Add(name, -1);
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
        /*		private void PomoWorkInc_OnClick(object sender, EventArgs e)
                {
                    int value = Convert.ToInt16((string) (sender as Button)?.CommandParameter);
                    PomoWorkTime += value;
                    lblPomoWork.Content = _pomoWorkTime.ToString();
                }*/
        private void PomoWork_OnValueChanged(object sender, EventArgs e)
        {
            if (iudPomoWork.Value != null)
                PomoWorkTime = (int)iudPomoWork.Value;
        }
        /*	private void PomoWorkDec_OnClick(object sender, EventArgs e)
            {
                int value = Convert.ToInt16((string) (sender as Button)?.CommandParameter);
                PomoWorkTime -= value;
                if (PomoWorkTime <= 0)
                    PomoWorkTime = value;
                lblPomoWork.Content = _pomoWorkTime.ToString();
            }
            private void PomoBreakInc_OnClick(object sender, EventArgs e)
            {
                int value = Convert.ToInt16((string) (sender as Button)?.CommandParameter);
                PomoBreakTime += value;
                lblPomoBreak.Content = _pomoBreakTime.ToString();
            }
            private void PomoBreakDec_OnClick(object sender, EventArgs e)
            {
                int value = Convert.ToInt16((string) (sender as Button)?.CommandParameter);
                PomoBreakTime -= value;
                if (PomoBreakTime <= 0)
                    PomoBreakTime = value;
                lblPomoBreak.Content = _pomoBreakTime.ToString();
            }*/
        private void PomoBreak_OnValueChanged(object sender, EventArgs e)
        {
            if (iudPomoBreak.Value != null)
                PomoBreakTime = (int)iudPomoBreak.Value;
        }

        // METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// FileIO //
        private string GetFilePath()
        {
            string result = "";
            if (RecentFiles.Count == 0)
                return BASE_PATH;

            string[] sa = RecentFiles[0].Split('\\');
            for (int i = 0; i < sa.Length - 1; i++)
                result += sa[i] + "\\";

            return result;
        }
        private string GetFileName()
        {
            if (RecentFiles.Count == 0)
                return "";

            string[] sa = RecentFiles[0].Split('\\');
            return sa[sa.Length - 1];
        }
        private void Load(string path)
        {
            SortRecentFiles(path);
            SaveSettings();

            StreamReader stream = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            ClearLists();
            KanbanCreateTabs();

            string line = stream.ReadLine();
            Load2_1SaveFile(stream, line);

            stream.Close();

            IncompleteItemsRefresh();
            KanbanRefresh();
            RefreshHistory();
            incompleteItemsTodoTabs.Items.Refresh();
            kanbanTodoTabs.Items.Refresh();
            kanbanTodoTabs.SelectedIndex = 3;

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
                return;

            DlgYesNo dlgYesNo = new DlgYesNo("New History", "Start a new History Item?");
            dlgYesNo.ShowDialog();
            if (dlgYesNo.Result)
                AddNewHistoryItem();
        }
        private void ClearLists()
        {
            _masterList.Clear();
            _incompleteItems.Clear();
            _kanbanItems.Clear();
            _incompleteItemsHashTags.Clear();
            _kanbanHashTags.Clear();
            _tabHash.Clear();
            _incompleteItemsTabsList.Clear();
            _kanbanTabsList.Clear();
            _kanbanTabHeaders.Clear();
            HistoryItems.Clear();
            _hashShortcuts.Clear();

            _lbKanbanItems = null;
            _lbIncompleteItems = null;
            _cbIncompleteItemsHashTags = null;
            _cbKanbanHashTags = null;

            incompleteItemsTodoTabs.SelectedIndex = -1;
            kanbanTodoTabs.SelectedIndex = -1;
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
            ConvertProjectVersion(stream.ReadLine());

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
                if (RecentFiles.Count > 1)
                    Save(RecentFiles[0]);
                else
                    SaveAs();
            }
        }
        private bool NewFile()
        {
            const string newFileName = "EToDo.txt";
            SaveFileDialog sfd = new SaveFileDialog
            {
                Title = @"Select folder to save file in.",
                FileName = newFileName,
                InitialDirectory = GetFilePath(),
                Filter = @"txt files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            DialogResult dr = sfd.ShowDialog();

            if (dr != System.Windows.Forms.DialogResult.OK)
                return false;

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

            stream.WriteLine("====================================TABS");
            foreach (TabItem ti in _incompleteItemsTabsList)
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
            int versionCheckBoxChecked = 0;
            if (cbVersionB.IsChecked == true)
                versionCheckBoxChecked = 1;
            else if (cbVersionC.IsChecked == true)
                versionCheckBoxChecked = 2;
            else if (cbVersionD.IsChecked == true)
                versionCheckBoxChecked = 3;

            stream.WriteLine(MakeCurrentVersion() + "." + versionCheckBoxChecked);
            stream.WriteLine("====================================TODO");
            foreach (TodoItem td in _masterList)
                stream.WriteLine(td.ToString());

            stream.WriteLine("====================================VCS");
            foreach (HistoryItem hi in HistoryItems)
                stream.Write(hi.ToString());

            stream.Close();
        }
        private void BackupSave()
        {
            if (!_autoBackup || !_doBackup)
                return;

            string path = RecentFiles[0] + ".bak" + _backupIncrement;
            _backupIncrement++;
            _backupIncrement = _backupIncrement > 9 ? 0 : _backupIncrement;
            SaveFile(path);
            _doBackup = false;
        }
        private void DelayedStartupLoad()
        {
            int noGoodRecentFilesCount = 0;
            for (int i = 0; i < RecentFiles.Count; i++)
            {
                if (File.Exists(RecentFiles[i]))
                {
                    Load(RecentFiles[i]);
                    tabControl.SelectedIndex = _previousSessionLastActiveTab;
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
        }

        // METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Settings //
        private void LoadSettings()
        {
            RecentFiles = new ObservableCollection<string>();

            const string filePath = BASE_PATH + "TDHistory.settings";
            if (!File.Exists(filePath))
                SaveSettings();

            DlgYesNo dlg;

            StreamReader stream = new StreamReader(File.Open(filePath, FileMode.Open));
            string line = stream.ReadLine();
            if (line != "RECENTFILES")
            {
                Top = 0;
                Left = 0;
                Height = 1080;
                Width = 1920;
                RecentFiles = new ObservableCollection<string>();

                DlgYesNo dlgYesNo = new DlgYesNo("Corrupted or missing file", "Error with the settings file, create a new one?");
                dlgYesNo.ShowDialog();
                if (dlgYesNo.Result)
                {
                    SaveSettings();
                    dlg = new DlgYesNo("New settings file created");
                    dlg.ShowDialog();
                }
            }

            LoadV2_1Settings(stream, line);
            stream.Close();

            if (RecentFiles.Count != 0)
                return;

            dlg = new DlgYesNo("New file created");
            dlg.ShowDialog();
        }
        private void LoadV2_1Settings(TextReader stream, string line)
        {
            while (line != null)
            {
                line = stream.ReadLine();
                if (line == "RECENTFILES" || line == "")
                    continue;

                if (line == "WINDOWPOSITION")
                    break;

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
            stream.ReadLine();
            _previousSessionLastActiveTab = Convert.ToInt16(stream.ReadLine());
        }
        private void SaveSettings()
        {
            const string filePath = BASE_PATH + "TDHistory.settings";
            StreamWriter stream = new StreamWriter(File.Open(filePath, FileMode.Create));

            stream.WriteLine("RECENTFILES");
            foreach (string s in RecentFiles)
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
            stream.WriteLine("PreviousSessionLastActiveTab");
            stream.WriteLine(tabControl.SelectedIndex);

            stream.Close();
        }
        private void SortRecentFiles(string recent)
        {
            if (RecentFiles.Contains(recent))
                RecentFiles.Remove(recent);
            RecentFiles.Insert(0, recent);

            while (RecentFiles.Count >= 10)
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }

        // Version stuff
        private void ConvertProjectVersion(string version)
        {
            string[] parts = version.Split('.');
            VersionA = Convert.ToInt16(parts[0]);
            VersionB = Convert.ToInt16(parts[1]);
            VersionC = Convert.ToInt16(parts[2]);
            VersionD = Convert.ToInt16(parts[3]);
            int checkBoxChecked = Convert.ToInt16(parts[4]);
            switch (checkBoxChecked)
            {
                case 0:
                    cbVersionA.IsChecked = true;
                    break;
                case 1:
                    cbVersionB.IsChecked = true;
                    break;
                case 2:
                    cbVersionC.IsChecked = true;
                    break;
                case 3:
                    cbVersionD.IsChecked = true;
                    break;
            }
        }
        private string MakeCurrentVersion()
        {
            return VersionA + "." +
                   VersionB + "." +
                   VersionC + "." +
                   VersionD;
        }
        private void UpdateCurrentVersion()
        {
            if (cbVersionA.IsChecked == true)
                VersionA++;

            if (cbVersionB.IsChecked == true)
                VersionB++;

            if (cbVersionC.IsChecked == true)
                VersionC++;

            if (cbVersionD.IsChecked == true)
                VersionD++;
        }
        private void VersionCheckBox_OnChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox == null)
                return;

            if (_isUpdatingCheckBoxes)
                return;

            _isUpdatingCheckBoxes = true;
            switch (checkBox.Name)
            {
                case "cbVersionA":
                    if (checkBox.IsChecked == true)
                    {
                        cbVersionB.IsChecked = false;
                        cbVersionC.IsChecked = false;
                        cbVersionD.IsChecked = false;
                    }
                    else
                    {
                        cbVersionB.IsChecked = true;
                    }

                    _isUpdatingCheckBoxes = false;
                    break;
                case "cbVersionB":
                    if (checkBox.IsChecked == true)
                    {
                        cbVersionA.IsChecked = false;
                        cbVersionC.IsChecked = false;
                        cbVersionD.IsChecked = false;
                    }
                    else
                    {
                        cbVersionC.IsChecked = true;
                    }

                    _isUpdatingCheckBoxes = false;
                    break;
                case "cbVersionC":
                    if (checkBox.IsChecked == true)
                    {
                        cbVersionA.IsChecked = false;
                        cbVersionB.IsChecked = false;
                        cbVersionD.IsChecked = false;
                    }
                    else
                    {
                        cbVersionB.IsChecked = true;
                    }

                    _isUpdatingCheckBoxes = false;
                    break;
                case "cbVersionD":
                    if (checkBox.IsChecked == true)
                    {
                        cbVersionA.IsChecked = false;
                        cbVersionB.IsChecked = false;
                        cbVersionC.IsChecked = false;
                    }
                    else
                    {
                        cbVersionB.IsChecked = true;
                    }

                    _isUpdatingCheckBoxes = false;
                    break;
            }
        }
        private void Version_OnValueChangedA(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (iudVersionA.Value != null)
                VersionA = (int)iudVersionA.Value;
        }
        private void Version_OnValueChangedB(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (iudVersionB.Value != null)
                VersionB = (int)iudVersionB.Value;
        }
        private void Version_OnValueChangedC(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (iudVersionC.Value != null)
                VersionC = (int)iudVersionC.Value;
        }
        private void Version_OnValueChangedD(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (iudVersionD.Value != null)
                VersionD = (int)iudVersionD.Value;
        }

        // Notes Panel stuff
        private void TodoTitle_OnGotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = GetActiveTextBoxTitle();
            textBox.Select(textBox.Text.Length, 0);
            _testIfChanged = textBox.Text;
        }
        private void Notes_OnGotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = GetActiveTextBoxNotes();
            textBox.Select(textBox.Text.Length, 0);
            _testIfChanged = textBox.Text;
        }
        private void Problem_OnGotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = GetActiveTextBoxProblem();
            textBox.Select(textBox.Text.Length, 0);
            _testIfChanged = textBox.Text;
        }
        private void Solution_OnGotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = GetActiveTextBoxSolution();
            textBox.Select(textBox.Text.Length, 0);
            _testIfChanged = textBox.Text;
        }
        private void TodoTitle_OnLostFocus(object sender, RoutedEventArgs e)
        {
            List<TodoItemHolder> todoItemList = GetActiveItemList();
            ListBox listBox = GetActiveListBox();
            TextBox textBox = GetActiveTextBoxTitle();

            if (textBox == null || listBox == null || todoItemList == null)
            {
                _errorMessage = "Function: Notes_OnLostFocus()\n" +
                                "\ttodoItemList == " + todoItemList + "\n" +
                                "\tlistBox == " + listBox + "\n" +
                                "\ttextBox == " + textBox + "\n" +
                                _errorMessage;
                new DlgErrorMessage(_errorMessage).ShowDialog();
                _errorMessage = string.Empty;
                return;
            }

            if (_testIfChanged == textBox.Text)
            {
                _skipUpdate = true;
                return;
            }

            UpdateTitle(listBox, textBox);
            IncompleteItemsRefresh();
            KanbanRefresh();
        }
        private void Notes_OnLostFocus(object sender, RoutedEventArgs e)
        {
            List<TodoItemHolder> todoItemList = GetActiveItemList();
            ListBox listBox = GetActiveListBox();
            TextBox textBox = GetActiveTextBoxNotes();

            if (textBox == null || listBox == null || todoItemList == null)
            {
                _errorMessage = "Function: Notes_OnLostFocus()\n" +
                                "\ttodoItemList == " + todoItemList + "\n" +
                                "\tlistBox == " + listBox + "\n" +
                                "\ttextBox == " + textBox + "\n" +
                                _errorMessage;
                new DlgErrorMessage(_errorMessage).ShowDialog();
                _errorMessage = string.Empty;
                return;
            }

            if (_testIfChanged == textBox.Text)
            {
                _skipUpdate = true;
                return;
            }

            UpdateNotes(listBox, textBox);
            IncompleteItemsRefresh();
            KanbanRefresh();
        }
        private void Problem_OnLostFocus(object sender, RoutedEventArgs e)
        {
            List<TodoItemHolder> todoItemList = GetActiveItemList();
            ListBox listBox = GetActiveListBox();
            TextBox textBox = GetActiveTextBoxProblem();

            if (textBox == null || listBox == null || todoItemList == null)
            {
                _errorMessage = "Function: Problem_OnLostFocus()\n" +
                                "\ttodoItemList == " + todoItemList + "\n" +
                                "\tlistBox == " + listBox + "\n" +
                                "\ttextBox == " + textBox + "\n" +
                                _errorMessage;
                new DlgErrorMessage(_errorMessage).ShowDialog();
                _errorMessage = string.Empty;
                return;
            }

            if (_testIfChanged == textBox.Text)
            {
                _skipUpdate = true;
                return;
            }

            UpdateProblem(listBox, textBox);
            IncompleteItemsRefresh();
            KanbanRefresh();
        }
        private void Solution_OnLostFocus(object sender, RoutedEventArgs e)
        {
            List<TodoItemHolder> todoItemList = GetActiveItemList();
            ListBox listBox = GetActiveListBox();
            TextBox textBox = GetActiveTextBoxSolution();

            if (textBox == null || listBox == null || todoItemList == null)
            {
                _errorMessage = "Function: Solution_OnLostFocus()\n" +
                                "\ttodoItemList == " + todoItemList + "\n" +
                                "\tlistBox == " + listBox + "\n" +
                                "\ttextBox == " + textBox + "\n" +
                                _errorMessage;
                new DlgErrorMessage(_errorMessage).ShowDialog();
                _errorMessage = string.Empty;
                return;
            }

            if (_testIfChanged == textBox.Text)
            {
                _skipUpdate = true;
                return;
            }

            UpdateSolution(listBox, textBox);
            IncompleteItemsRefresh();
            KanbanRefresh();
        }
        private void NotesPanelUpdate()
        {
            UpdateNotes(_lbIncompleteItems, tbIncompleteItemsNotes);
            UpdateTitle(_lbIncompleteItems, tbIncompleteItemsTitle);
            UpdateProblem(_lbIncompleteItems, tbIncompleteItemsProblem);
            UpdateSolution(_lbIncompleteItems, tbIncompleteItemsSolution);
            NotesPanelLoadHashTags();
        }
        private void UpdateNotes(ListBox listBox, TextBox textBox)
        {
            if (listBox.SelectedIndex >= 0 && _currentTodoItemInNotesPanel != null)
                _currentTodoItemInNotesPanel.Notes = textBox.Text;
        }
        private void UpdateTitle(ListBox listBox, TextBox textBox)
        {
            if (listBox.SelectedIndex >= 0 && _currentTodoItemInNotesPanel != null)
                _currentTodoItemInNotesPanel.Todo = textBox.Text;
        }
        private void UpdateProblem(ListBox listBox, TextBox textBox)
        {
            if (listBox.SelectedIndex >= 0 && _currentTodoItemInNotesPanel != null)
                _currentTodoItemInNotesPanel.Problem = textBox.Text;
        }
        private void UpdateSolution(ListBox listBox, TextBox textBox)
        {
            if (listBox.SelectedIndex >= 0 && _currentTodoItemInNotesPanel != null)
                _currentTodoItemInNotesPanel.Solution = textBox.Text;
        }
        private void NotesPanelLoadHashTags()
        {
            if (_currentTodoItemInNotesPanel == null ||
                _lbNotesPanelHashTags == null)
                return;

            _notesPanelHashTags.Clear();
            foreach (string tag in _currentTodoItemInNotesPanel.Tags)
                _notesPanelHashTags.Add(tag);

            _lbNotesPanelHashTags.ItemsSource = _notesPanelHashTags;
            _lbNotesPanelHashTags.Items.Refresh();
        }
        private void TodoComplete_OnClick(object sender, EventArgs e)
        {
            TodoComplete();
        }
        private void TodoComplete()
        {
            List<TodoItemHolder> todoItemList = GetActiveItemList();
            ListBox listBox = GetActiveListBox();
            TextBox titleTextBox = GetActiveTextBoxTitle();
            TextBox notesTextBox = GetActiveTextBoxNotes();
            TextBox problemTextBox = GetActiveTextBoxProblem();
            TextBox solutionTextBox = GetActiveTextBoxSolution();

            if (titleTextBox == null ||
                problemTextBox == null ||
                notesTextBox == null ||
                solutionTextBox == null ||
                listBox == null ||
                todoItemList == null)
            {
                _errorMessage = "Function: Problem_OnLostFocus()\n" +
                                "\ttodoItemList == " + todoItemList + "\n" +
                                "\tlistBox == " + listBox + "\n" +
                                "\ttitleTextBox == " + titleTextBox + "\n" +
                                "\tnotesTextBox == " + notesTextBox + "\n" +
                                "\tproblemTextBox == " + problemTextBox + "\n" +
                                "\tsolutionTextBox == " + solutionTextBox + "\n" +
                                _errorMessage +
                                "Did not complete todo!";
                new DlgErrorMessage(_errorMessage).ShowDialog();
                _errorMessage = string.Empty;
                return;
            }

            if (_currentTodoItemInNotesPanel == null)
                return;

            UpdateTitle(listBox, titleTextBox);
            UpdateNotes(listBox, notesTextBox);
            UpdateProblem(listBox, problemTextBox);
            UpdateSolution(listBox, solutionTextBox);
            _currentTodoItemInNotesPanel.IsComplete = true;
            _lbCurrentItems.SelectedItem = _lbCurrentItems.Items.GetItemAt(0);
            KanbanRefresh();
            IncompleteItemsRefresh();
        }
        private void AddTag_OnClick(object sender, EventArgs e)
        {
            if (_currentTodoItemInNotesPanel == null)
                return;

            SortHashTagLists(_incompleteItems, _incompleteItemsHashTags);

            List<string> selectedTags = new List<string>();
            foreach (TodoItemHolder tdi in _lbCurrentItems.SelectedItems)
            foreach (string tag in tdi.TD.Tags.Where(tag => !selectedTags.Contains(tag)))
                selectedTags.Add(tag);

            bool multi = _lbCurrentItems.SelectedItems.Count > 1;
            TagPicker tp = new TagPicker(multi);
            tp.LoadTags(_incompleteItemsHashTags[0], selectedTags);
            tp.ShowDialog();
            if (tp.Result == false)
                return;

            if (_lbCurrentItems.SelectedItems.Count > 1)
            {
                if (tp.Multi)
                    foreach (TodoItemHolder tdi in _lbCurrentItems.SelectedItems)
                        tdi.TD.Tags = tp.NewTags;
                else
                    foreach (string s in tp.NewTags)
                    foreach (TodoItemHolder tdi in _lbCurrentItems.SelectedItems)
                        tdi.TD.AddTag(s);
            }
            else
            {
                _currentTodoItemInNotesPanel.Tags = tp.NewTags;
            }

            _lbNotesPanelHashTags.ItemsSource = _currentTodoItemInNotesPanel.Tags;
            _lbNotesPanelHashTags.Items.Refresh();
            IncompleteItemsRefresh();
            KanbanRefresh();
        }
        private void NewTag_LostFocus(object sender, RoutedEventArgs e)
        {
            AddNewTagsToTodo();
        }
        private void DeleteTag_OnClick(object sender, EventArgs e)
        {
            if (!(sender is Button b))
                return;

            TagHolder th = b.DataContext as TagHolder;
            if (th == null)
                return;

            _currentTodoItemInNotesPanel.Tags.Remove(th.Text);

            NotesPanelLoadHashTags();
            IncompleteItemsRefresh();
            KanbanRefresh();
        }
        private void RefreshNotes(int index = 0)
        {
            if (_lbCurrentItems == null || index >= _lbCurrentItems.Items.Count || index < 0)
                return;

            _lbCurrentItems.SelectedItem = _lbCurrentItems.Items.GetItemAt(index);
        }
    }
}
