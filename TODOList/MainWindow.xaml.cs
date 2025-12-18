using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Echoslate.ViewModels;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;
using MenuItem = System.Windows.Controls.MenuItem;


namespace Echoslate {
	public partial class MainWindow : INotifyPropertyChanged {
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
		private const string PROGRAM_VERSION = "3.40.20.1";
		public const string DATE_STRING_FORMAT = "yyyyMMdd";
		public const string TIME_STRING_FORMAT = "HHmmss";
		private const string GIT_EXE_PATH = "C:\\Program Files\\Git\\cmd\\";

		const string SETTINGS_FILENAME = "Echoslate.settings";
		public Settings Settings;

		public static MainWindow GetActiveWindow() {
			return (MainWindow)Application.Current.MainWindow;
		}


		// FILE IO
		// TODO Change this path to where the EXE is.
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


		private int _versionA;
		private int _versionB;
		private int _versionC;
		private int _versionD;

		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		private int VersionA {
			get => _versionA;
			set {
				_versionA = value;
			}
		}
		private int VersionB {
			get => _versionB;
			set {
				_versionB = value;
			}
		}
		private int VersionC {
			get => _versionC;
			set {
				_versionC = value;
			}
		}
		private int VersionD {
			get => _versionD;
			set {
				_versionD = value;
			}
		}


		private string WindowTitle => "Echoslate v" + PROGRAM_VERSION + " " + _currentOpenFile;

		public int PomoWorkTime {
			get => _pomoWorkTime;
			set {
				_pomoWorkTime = value;
				OnPropertyChanged();
			}
		}
		public int PomoTimeLeft {
			get => _pomoTimeLeft;
			set {
				_pomoTimeLeft = value;
				OnPropertyChanged();
			}
		}
		public int PomoBreakTime {
			get => _pomoBreakTime;
			set {
				_pomoBreakTime = value;
				OnPropertyChanged();
			}
		}

		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public MainWindow() {
			// UNCHECKED
			Log.Initialize();

			InitializeComponent();
			Closing += Window_Closed;
			Log.Print("Window Initialized");

#if DEBUG
			this.PreviewKeyDown += (_, e) => {
				if (e.Key == Key.Escape) {
					Log.Print("ESC pressed closing app (debug shortcut)");
					Application.Current.Shutdown();
				}
			};
			mnuMain.Background = Brushes.Red;
#endif

			Settings = new Settings(BASE_PATH, SETTINGS_FILENAME);
#if DEBUG
			_autoSave = false;
			_autoBackup = false;
#else
			_autoSave = true;
			_autoBackup = true;
#endif

			DataContext = new MainWindowViewModel(Settings);

			SetWindowPosition(Settings.Window);

			_backupTime = new TimeSpan(0, 5, 0);
			_backupIncrement = 0;

			var timer = new DispatcherTimer();
			timer.Tick += Timer_Tick;
			timer.Interval = new TimeSpan(TimeSpan.TicksPerSecond);
			timer.Start();

			mnuRecentLoads.ItemsSource = Settings.RecentFiles;
			_timeUntilBackup = _backupTime;
		}
		private void SetWindowPosition(Rectangle window) {
			Top = window.Y;
			Left = window.X;
			Height = window.Height;
			Width = window.Width;
		}
		private void ResetWindowPosition() {
			Top = 0;
			Left = 0;
			Height = 1080;
			Width = 1920;
		}

		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Windows METHODS //
		protected override void OnSourceInitialized(EventArgs e) {
			// UNCHECKED
			base.OnSourceInitialized(e);

			_handle = new WindowInteropHelper(this).Handle;
			_source = HwndSource.FromHwnd(_handle);
			_source?.AddHook(HwndHook);


			GlobalHotkeysToggle();
		}
		private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			// UNCHECKED
			const int wmHotkey = 0x0312;
			// switch (msg) {
			// case wmHotkey:
			// switch (wParam.ToInt32()) {
			// case HOTKEY_ID:
			// int vkey = (((int)lParam >> 16) & 0xFFFF);
			// if (vkey == 0x73) {
			// Activate();
			// FocusManager.SetFocusedElement(FocusManager.GetFocusScope(tbHNotes), tbHNotes);
			// _tbNewTodo.Focus();
			// }

			// handled = true;
			// break;
			// }

			// break;
			// }

			return IntPtr.Zero;
		}
		private void Window_Closed(object sender, CancelEventArgs e) {
			// UNCHECKED
			UnregisterHotKey(_handle, HOTKEY_ID);

			Log.Print("Saving settings...");
			// SaveSettings();
			Log.Print("Settings saved.");

			if (!_isChanged) {
				Log.Print("Shutting down...");
				Log.Shutdown();
				return;
			}

			DlgYesNo dlg = new DlgYesNo("Close", "Maybe save first?");
			dlg.ShowDialog();
			if (dlg.Result) {
				Save(_currentOpenFile);
			}

			Log.Print("Shutting down...");
			Log.Shutdown();
		}
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		private void GlobalHotkeysToggle() {
			// UNCHECKED
			if (_globalHotkeys)
				RegisterHotKey(_handle, HOTKEY_ID, MOD_WIN, 0x73);
			else
				UnregisterHotKey(_handle, HOTKEY_ID);
		}
		private static string UpperFirstLetter(string s) {
			// UNCHECKED
			string result = "";
			for (int i = 0; i < s.Length; i++) {
				if (i == 0)
					result += s[i].ToString().ToUpper();
				else
					result += s[i];
			}

			return result;
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Timers //
		private void Timer_Tick(object sender, EventArgs e) {
			// UNCHECKED
			_timeUntilBackup = _timeUntilBackup.Subtract(new TimeSpan(0, 0, 1));
			if (_timeUntilBackup <= TimeSpan.Zero) {
				_timeUntilBackup = _backupTime;
				BackupSave();
			}

			// foreach (var td in _masterList.Where(td => td.IsTimerOn))
				// td.TimeTaken = td.TimeTaken.AddSeconds(1);

			// foreach (TodoItemHolder itemHolder in from list in _incompleteItems
												  // from itemHolder in list
												  // where itemHolder.TD.IsTimerOn
												  // select itemHolder)
				// itemHolder.TimeTaken = itemHolder.TD.TimeTaken;

			lblPomo.Content = $"{_pomoTimer.Ticks / TimeSpan.TicksPerMinute:00}:{_pomoTimer.Second:00}";
			if (_isPomoTimerOn) {
				pbPomo.Background = Brushes.Maroon;
				_pomoTimer = _pomoTimer.AddSeconds(1);

				if (_isPomoWorkTimerOn) {
					long ticks = _pomoWorkTime * TimeSpan.TicksPerMinute;
					PomoTimeLeft = (int)((float)_pomoTimer.Ticks / ticks * 100);
					pbPomo.Background = Brushes.DarkGreen;
					if (_pomoTimer.Ticks < ticks)
						return;

					_isPomoWorkTimerOn = false;
					_pomoTimer = DateTime.MinValue;
				} else {
					long ticks = _pomoBreakTime * TimeSpan.TicksPerMinute;
					PomoTimeLeft = (int)((float)(ticks - _pomoTimer.Ticks) / ticks * 100);
					if (_pomoTimer.Ticks < ticks)
						return;

					_isPomoWorkTimerOn = true;
					_pomoTimer = DateTime.MinValue;
				}
			} else {
				pbPomo.Background = Brushes.Transparent;
				lblPomo.Background = Brushes.Transparent;
			}
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// History //
		private void LoadHistory() {
			// UNCHECKED
			string[] pathPieces = _currentOpenFile.Split('\\');
			string path = "";
			for (int i = 0; i < pathPieces.Length - 1; i++)
				path += pathPieces[i] + "\\";

			string gitPath = FindGitDirectory(path);
			_historyLogPath = path + "log.txt";
			ProcessStartInfo startInfo = new ProcessStartInfo {
																  CreateNoWindow = false,
																  UseShellExecute = false,
																  FileName = "cmd.exe",
																  WindowStyle = ProcessWindowStyle.Hidden
															  };
			string args = "/c call \"" + GIT_EXE_PATH + "git\" --git-dir=\"" + gitPath + "\\.git\" log > \"" +
						  _historyLogPath + "\"";
			startInfo.Arguments = args;
			Process p = new Process {
										StartInfo = startInfo
									};
			p.Start();
			p.WaitForExit();

			List<string> log = new List<string>();
			StreamReader stream = new StreamReader(File.Open(_historyLogPath, FileMode.OpenOrCreate));
			string line = stream.ReadLine();
			while (line != null) {
				if (line.Split(' ')[0] == "commit")
					log.Add("=====================================================================================" +
							Environment.NewLine);

				log.Add(line);
				line = stream.ReadLine();
			}

			lbHistoryLog.ItemsSource = log;
			lbHistoryLog.Items.Refresh();
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Git //
		private void RefreshLog_OnClick(object sender, EventArgs e) {
			// UNCHECKED
			LoadHistory();
		}
		private static string FindGitDirectory(string dir) {
			// UNCHECKED
			if (dir == null)
				return null;
			while (true) {
				if (dir == Directory.GetDirectoryRoot(dir))
					return null;

				List<string> dirs = Directory.GetDirectories(dir).ToList();
				if (dirs.Any(s => s.Contains(".git")))
					return dir;

				dir = Directory.GetParent(dir)?.FullName;
			}
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// MenuCommands //
		private void mnuNew_OnClick(object sender, EventArgs e) {
			// UNCHECKED
			AutoSave();
			DlgYesNo dlg = new DlgYesNo("New file", "Are you sure?");
			dlg.ShowDialog();

			if (!dlg.Result)
				return;
			if (!NewFile())
				return;

			ConvertProjectVersion("0.0.0.0");

			_currentOpenFile = "";
			Title = WindowTitle;
			Save(Settings.RecentFiles[0]);
		}
		private void mnuLoadFiles_OnClick(object sender, RoutedEventArgs e) {
			// UNCHECKED
			if (_recentFilesIndex < 0)
				return;

			MenuItem mi = (MenuItem)e.OriginalSource;
			if (!(mi.DataContext is string path))
				return;

			if (_isChanged) {
				DlgYesNo dlgYesNo = new DlgYesNo("Close", "Maybe save first?");
				dlgYesNo.ShowDialog();
				if (dlgYesNo.Result)
					Save(_currentOpenFile);
			}

			// Load(path);
		}
		private void mnuLoad_OnClick(object sender, EventArgs e) {
			// UNCHECKED
			OpenFileDialog openFileDialog = new OpenFileDialog {
																   Title = @"Open file: ",
																   InitialDirectory = GetFilePath(),
																   Filter =
																	   @"txt files (*.txt)|*.txt|All files (*.*)|*.*"
															   };

			DialogResult dr = openFileDialog.ShowDialog();

			if (dr != System.Windows.Forms.DialogResult.OK)
				return;

			if (_isChanged) {
				DlgYesNo dlgYesNo = new DlgYesNo("Close", "Maybe save first?");
				dlgYesNo.ShowDialog();
				if (dlgYesNo.Result)
					Save(_currentOpenFile);
			}

			// Load(openFileDialog.FileName);
		}
		private void mnuSave_OnClick(object sender, EventArgs e) {
			// UNCHECKED
			if (_currentOpenFile == null) {
				DlgYesNo dlg = new DlgYesNo("No current file");
				dlg.ShowDialog();
				return;
			}

			Save(_currentOpenFile);
		}
		private void mnuSaveAs_OnClick(object sender, EventArgs e) {
			// UNCHECKED
			SaveAs();
		}
		private void mnuOptions_OnClick(object sender, EventArgs e) {
			// UNCHECKED
			DlgOptions options = new DlgOptions(_autoSave, _globalHotkeys, _autoBackup, _backupTime);
			options.ShowDialog();
			if (!options.Result)
				return;

#if DEBUG
#else
			_autoSave = options.AutoSave;
			_globalHotkeys = options.GlobalHotkeys;
			_autoBackup = options.AutoBackup;
			_backupTime = options.BackupTime;
#endif
			GlobalHotkeysToggle();
			AutoSave();
		}
		private void mnuQuit_OnClick(object sender, EventArgs e) {
			// UNCHECKED
			Close();
		}
		private void mnuHelp_OnClick(object sender, EventArgs e) {
			// UNCHECKED
			DlgHelp dlgH = new DlgHelp();
			dlgH.ShowDialog();
		}

		// RECENT FILES menu
		private void mnuRecentLoads_OnRMBUp(object sender, MouseButtonEventArgs e) {
			// UNCHECKED
			_recentFilesIndex = -1;
			if (!(e.OriginalSource is TextBlock mi))
				return;

			string path = (string)mi.DataContext;
			_recentFilesIndex = mnuRecentLoads.Items.IndexOf(path);
		}
		private void mnuRemoveFile_OnClick(object sender, RoutedEventArgs e) {
			// UNCHECKED
			if (_recentFilesIndex < 0) {
				_recentFilesIndex = 0;
				return;
			}

			Settings.RecentFiles.RemoveAt(_recentFilesIndex);
			mnuRecentLoads.Items.Refresh();
		}


		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// POMO STUFF //
		private void PomoTimerToggle_OnClick(object sender, EventArgs e) {
			// UNCHECKED
			_isPomoTimerOn = !_isPomoTimerOn;
		}
		private void PomoTimerReset_OnClick(object sender, EventArgs e) {
			// UNCHECKED
			_isPomoTimerOn = true;
			_pomoTimer = DateTime.MinValue;
			PomoTimeLeft = 0;
		}
		private void PomoWork_OnValueChanged(object sender, EventArgs e) {
			// UNCHECKED
			if (iudPomoWork.Value != null)
				PomoWorkTime = (int)iudPomoWork.Value;
		}
		private void PomoBreak_OnValueChanged(object sender, EventArgs e) {
			// UNCHECKED
			if (iudPomoBreak.Value != null)
				PomoBreakTime = (int)iudPomoBreak.Value;
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// FileIO //
		private string GetFilePath() {
			// UNCHECKED
			string result = "";
			if (Settings.RecentFiles.Count == 0)
				return BASE_PATH;

			string[] sa = Settings.RecentFiles[0].Split('\\');
			for (int i = 0; i < sa.Length - 1; i++)
				result += sa[i] + "\\";

			return result;
		}
		private string GetFileName() {
			// UNCHECKED
			if (Settings.RecentFiles.Count == 0)
				return "";

			string[] sa = Settings.RecentFiles[0].Split('\\');
			return sa[sa.Length - 1];
		}
		private void AutoSave() {
			// UNCHECKED
			_isChanged = true;
			_doBackup = true;
			if (_currentOpenFile == "") {
				SaveAs();
				return;
			}

			if (_autoSave) {
				if (Settings.RecentFiles.Count >= 1)
					Save(Settings.RecentFiles[0]);
				else
					SaveAs();
			}
		}
		private bool NewFile() {
			// UNCHECKED
			const string newFileName = "EToDo.txt";
			SaveFileDialog sfd = new SaveFileDialog {
														Title = @"Select folder to save file in.",
														FileName = newFileName,
														InitialDirectory = GetFilePath(),
														Filter = @"txt files (*.txt)|*.txt|All files (*.*)|*.*"
													};

			DialogResult dr = sfd.ShowDialog();

			if (dr != System.Windows.Forms.DialogResult.OK)
				return false;

			// SortRecentFiles(sfd.FileName);
			// SaveSettings();
			return true;
		}
		private void SaveAs() {
			// UNCHECKED
			Log.Print("Saving file as...");
			SaveFileDialog sfd = new SaveFileDialog {
														Title = @"Select folder to save file in.",
														FileName = GetFileName(),
														InitialDirectory = GetFilePath(),
														Filter = @"txt files (*.txt)|*.txt|All files (*.*)|*.*"
													};

			DialogResult dr = sfd.ShowDialog();
			if (dr != System.Windows.Forms.DialogResult.OK) {
				Log.Warn("File not saved. Continuing...");
				return;
			}

			SaveFile(sfd.FileName);
		}
		private void Save(string path) {
			// UNCHECKED
			Log.Print($"Saving file {path}");
			if (!File.Exists(path)) {
				Log.Warn($"Can not find file: {path}");
				SaveAs();
				return;
			}
			SaveFile(path);
		}
		private void SaveFile(string path) {
			// UNCHECKED
			// SortRecentFiles(path);
			// SaveSettings();

			Log.Print("Opening stream...");
			StreamWriter stream = new StreamWriter(File.Open(path, FileMode.Create));
			Log.Print("Writing TABS...");
			stream.WriteLine("====================================TABS");
			// foreach (TabItem ti in _incompleteItemsTabsList)
				// stream.WriteLine(ti.Name);

			Log.Print("Writing FILESETTINGS...");
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
			// if (cbVersionB.IsChecked == true)
			// versionCheckBoxChecked = 1;
			// else if (cbVersionC.IsChecked == true)
			// versionCheckBoxChecked = 2;
			// else if (cbVersionD.IsChecked == true)
			// versionCheckBoxChecked = 3;
			stream.WriteLine(MakeCurrentVersion() + "." + versionCheckBoxChecked);

			Log.Print("Writing TODOs...");
			stream.WriteLine("====================================TODO");
			// foreach (TodoItem td in _masterList)
				// stream.WriteLine(td.ToString());

			Log.Print("Writing VCS Items...");
			stream.WriteLine("====================================VCS");
			// foreach (HistoryItem hi in HistoryItems)
				// stream.Write(hi.ToString());

			stream.Close();
			Log.Print("Saving complete.");

			_currentOpenFile = path;
			Title = WindowTitle;
			// Why is this here twice? Needs to be above so Title will be correct after changing a file name, but after?
			// _currentOpenFile = path;
			_isChanged = false;
		}
		private void BackupSave() {
			// UNCHECKED
			if (!_autoBackup || !_doBackup)
				return;

			string path = Settings.RecentFiles[0] + ".bak" + _backupIncrement;
			_backupIncrement++;
			_backupIncrement = _backupIncrement > 9 ? 0 : _backupIncrement;
			SaveFile(path);
			_doBackup = false;
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// Settings //

		// Version stuff
		private void ConvertProjectVersion(string version) {
			// UNCHECKED
			string[] parts = version.Split('.');
			VersionA = Convert.ToInt16(parts[0]);
			VersionB = Convert.ToInt16(parts[1]);
			VersionC = Convert.ToInt16(parts[2]);
			VersionD = Convert.ToInt16(parts[3]);
			int checkBoxChecked = Convert.ToInt16(parts[4]);
			switch (checkBoxChecked) {
				case 0:
					// cbVersionA.IsChecked = true;
					break;
				case 1:
					// cbVersionB.IsChecked = true;
					break;
				case 2:
					// cbVersionC.IsChecked = true;
					break;
				case 3:
					// cbVersionD.IsChecked = true;
					break;
			}
		}
		private string MakeCurrentVersion() {
			// UNCHECKED
			return VersionA + "." +
				   VersionB + "." +
				   VersionC + "." +
				   VersionD;
		}
	}
}