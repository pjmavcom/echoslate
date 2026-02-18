using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Echoslate.Core.Models;
using Application = System.Windows.Application;


namespace Echoslate.Wpf;

public partial class MainWindow : INotifyPropertyChanged {
	// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
	private const string PROGRAM_VERSION = "4.1.1.0";
	public const string DATE_STRING_FORMAT = "yyyyMMdd";
	public const string TIME_STRING_FORMAT = "HHmmss";
	private const string GIT_EXE_PATH = "C:\\Program Files\\Git\\cmd\\";

	private string WindowTitle => "Echoslate v" + PROGRAM_VERSION;
	public int LastActiveTabIndex { get; set; }

	// TODO Change this path to where the EXE is.
	private string _historyLogPath;


	// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
	public MainWindow() {
		InitializeComponent();
		Log.Print("Window Initialized");

#if DEBUG
		PreviewKeyDown += (_, e) => {
			if (e.Key == Key.Escape) {
				Log.Print("ESC pressed closing app (debug shortcut)");
				Application.Current.Shutdown();
			}
		};
		mnuMain.Background = Brushes.Red;
#endif

		LastActiveTabIndex = AppSettings.Instance.LastActiveTabIndex;
		AppSettings.Instance.WindowTitle = WindowTitle;
		Closed += (s, e) => Window_OnClosed();
		Loaded += (s, e) => Window_OnLoaded();
	}
	private void Window_OnClosed() {
		AppSettings.Instance.LastActiveTabIndex = tabControl.SelectedIndex;
		AppSettings.Save();
	}
	private void Window_OnLoaded() {
		tabControl.SelectedIndex = LastActiveTabIndex;
		SetWindowPosition();
	}
	private void SetWindowPosition() {
		var mainWindow = Application.Current.MainWindow;

		mainWindow.Left = AppSettings.Instance.WindowLeft;
		mainWindow.Top = AppSettings.Instance.WindowTop;
		mainWindow.Width = AppSettings.Instance.WindowWidth;
		mainWindow.Height = AppSettings.Instance.WindowHeight;
		mainWindow.WindowState = AppSettings.Instance.WindowState switch {
			Core.Services.WindowState.Maximized => WindowState.Maximized,
			Core.Services.WindowState.Minimized => WindowState.Minimized,
			_ => WindowState.Normal
		};
	}
	public void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
		if (Keyboard.Modifiers == ModifierKeys.Alt) {
			Log.Debug(e.Key.ToString());
			Key actualKey = (e.Key == Key.System) ? e.SystemKey : e.Key;
			if (actualKey == Key.H) {
				SwitchTab(-1);
				e.Handled = true;
			} else if (actualKey == Key.L) {
				SwitchTab(1);
				e.Handled = true;
			}
		}
	}
	private void SwitchTab(int direction) {
		if (tabControl.Items.Count == 0) {
			return;
		}
		int newIndex = tabControl.SelectedIndex + direction;
		if (newIndex < 0) {
			newIndex = 0;
		} else if (newIndex >= tabControl.Items.Count) {
			newIndex = tabControl.Items.Count - 1;
		}
		tabControl.SelectedIndex = newIndex;
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

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

		// lbHistoryLog.ItemsSource = log;
		// lbHistoryLog.Items.Refresh();
	}
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
}