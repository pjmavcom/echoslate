using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using Echoslate.ViewModels;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;


namespace Echoslate {
	public partial class MainWindow : INotifyPropertyChanged {
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
		private const string PROGRAM_VERSION = "3.40.20.1";
		public const string DATE_STRING_FORMAT = "yyyyMMdd";
		public const string TIME_STRING_FORMAT = "HHmmss";
		private const string GIT_EXE_PATH = "C:\\Program Files\\Git\\cmd\\";

		const string SETTINGS_FILENAME = "Echoslate.settings";


		// FILE IO
		// TODO Change this path to where the EXE is.
		private const string BASE_PATH = @"C:\MyBinaries\";
		private bool _doBackup;
		private bool _autoSave;
		private bool _autoBackup;
		private TimeSpan _backupTime;
		private TimeSpan _timeUntilBackup;
		private int _backupIncrement;
		private string _historyLogPath;

		// Pomo Timer 
		private DateTime _pomoTimer;
		private bool _isPomoTimerOn;
		private bool _isPomoWorkTimerOn = true;
		private int _pomoWorkTime = 25;
		private int _pomoBreakTime = 5;
		private int _pomoTimeLeft;


		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		private string WindowTitle => "Echoslate v" + PROGRAM_VERSION;

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
			Log.Initialize();

			InitializeComponent();
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

			AppDataSettings appDataSettings = new AppDataSettings(BASE_PATH, SETTINGS_FILENAME, WindowTitle);
#if DEBUG
			_autoSave = false;
			_autoBackup = false;
#else
			_autoSave = true;
			_autoBackup = true;
#endif

			DataContext = new MainWindowViewModel(appDataSettings);

			if (appDataSettings == null) {
				SetWindowPosition(new Rectangle(0, 0, 1920, 1080));
			} else {
				SetWindowPosition(appDataSettings.Window);
			}

			_backupTime = new TimeSpan(0, 5, 0);
			_backupIncrement = 0;

			var timer = new DispatcherTimer();
			timer.Tick += Timer_Tick;
			timer.Interval = new TimeSpan(TimeSpan.TicksPerSecond);
			timer.Start();

			_timeUntilBackup = _backupTime;
		}
		private void SetWindowPosition(Rectangle window) {
			Top = window.Y;
			Left = window.X;
			Height = window.Height;
			Width = window.Width;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Timer_Tick(object sender, EventArgs e) {
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
			// string[] pathPieces = _currentOpenFile.Split('\\');
			string path = "";
			// for (int i = 0; i < pathPieces.Length - 1; i++)
				// path += pathPieces[i] + "\\";

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
			LoadHistory();
		}
		private static string FindGitDirectory(string dir) {
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

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// POMO STUFF //
		private void PomoTimerToggle_OnClick(object sender, EventArgs e) {
			_isPomoTimerOn = !_isPomoTimerOn;
		}
		private void PomoTimerReset_OnClick(object sender, EventArgs e) {
			_isPomoTimerOn = true;
			_pomoTimer = DateTime.MinValue;
			PomoTimeLeft = 0;
		}
		private void PomoWork_OnValueChanged(object sender, EventArgs e) {
			if (iudPomoWork.Value != null)
				PomoWorkTime = (int)iudPomoWork.Value;
		}
		private void PomoBreak_OnValueChanged(object sender, EventArgs e) {
			if (iudPomoBreak.Value != null)
				PomoBreakTime = (int)iudPomoBreak.Value;
		}

		// METHODS  /////////////////////////////////////////////////////////////////////////////////////////////////////////////// FileIO //
		private void BackupSave() {
			// UNCHECKED
			if (!_autoBackup || !_doBackup)
				return;

			// string path = AppDataSettings.RecentFiles[0] + ".bak" + _backupIncrement;
			_backupIncrement++;
			_backupIncrement = _backupIncrement > 9 ? 0 : _backupIncrement;
			// SaveFile(path);
			_doBackup = false;
		}
	}
}