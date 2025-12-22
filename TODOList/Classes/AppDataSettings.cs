using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace Echoslate {
	public class AppDataSettings {
		public static bool SkipWelcome {
			get => Properties.Settings.Default.SkipWelcome;
			set => Properties.Settings.Default.SkipWelcome = value;
		}
		public static ObservableCollection<string> RecentFiles { get; set; }
		public static string LastFilePath {
			get => Properties.Settings.Default.LastFilePath;
			set => Properties.Settings.Default.LastFilePath = value;
		}

		public static double WindowLeft {
			get => Properties.Settings.Default.WindowLeft;
			set => Properties.Settings.Default.WindowLeft = value;
		}
		public static double WindowTop {
			get => Properties.Settings.Default.WindowTop;
			set => Properties.Settings.Default.WindowTop = value;
		}
		public static double WindowWidth {
			get => Properties.Settings.Default.WindowWidth;
			set => Properties.Settings.Default.WindowWidth = value;
		}
		public static double WindowHeight {
			get => Properties.Settings.Default.WindowHeight;
			set => Properties.Settings.Default.WindowHeight = value;
		}
		public static WindowState WindowState {
			get => Properties.Settings.Default.WindowState;
			set => Properties.Settings.Default.WindowState = value;
		}

		public static TimeSpan PomoWorkTimerLength { get; set; }
		public static TimeSpan PomoBreakTimerLength { get; set; }
		public static bool GlobalHotkeysEnabled {
			get => Properties.Settings.Default.GlobalHotkeysEnabled;
			set => Properties.Settings.Default.GlobalHotkeysEnabled = value;
		}
		public static TimeSpan BackupTime { get; set; }

		public static int LastActiveTabIndex {
			get => Properties.Settings.Default.LastActiveTabIndex;
			set => Properties.Settings.Default.LastActiveTabIndex = value;
		}

		public static string WindowTitle { get; set; }


		public AppDataSettings() {
			RecentFiles = [];
		}
		public static void SaveSettings(int lastActiveTab = -1) {
			var mainWindow = Application.Current.MainWindow;
			if (mainWindow != null) {
				WindowLeft = double.IsNaN(mainWindow.Left) ? 0 : mainWindow.Left;
				WindowTop = double.IsNaN(mainWindow.Top) ? 0 : mainWindow.Top;
				WindowWidth = mainWindow.Width;
				WindowHeight = mainWindow.Height;
				WindowState = mainWindow.WindowState;
			}
			Properties.Settings.Default.PomoWorkTimer = PomoWorkTimerLength.Minutes;
			Properties.Settings.Default.PomoBreakTimer = PomoBreakTimerLength.Minutes;
			Properties.Settings.Default.BackupTime = BackupTime.Minutes;

			if (lastActiveTab > 0) {
				LastActiveTabIndex = lastActiveTab;
			}

			Properties.Settings.Default.RecentFiles ??= new StringCollection();
			Properties.Settings.Default.RecentFiles.Clear();
			Properties.Settings.Default.RecentFiles.AddRange(RecentFiles.ToArray());

			Properties.Settings.Default.Save();
		}
		public static void LoadSettings() {
			WindowLeft = Properties.Settings.Default.WindowLeft;
			WindowTop = Properties.Settings.Default.WindowTop;
			WindowWidth = Properties.Settings.Default.WindowWidth;
			WindowHeight = Properties.Settings.Default.WindowHeight;
			WindowState = Properties.Settings.Default.WindowState;

			PomoWorkTimerLength = new TimeSpan(0, Properties.Settings.Default.PomoWorkTimer, 0);
			PomoBreakTimerLength = new TimeSpan(0, Properties.Settings.Default.PomoWorkTimer, 0);
			BackupTime = new TimeSpan(0, Properties.Settings.Default.BackupTime, 0);

			GlobalHotkeysEnabled = Properties.Settings.Default.GlobalHotkeysEnabled;
			LastActiveTabIndex = Properties.Settings.Default.LastActiveTabIndex;

			if (Properties.Settings.Default.RecentFiles != null) {
				RecentFiles = new ObservableCollection<string>(Properties.Settings.Default.RecentFiles.Cast<string>());
				if (RecentFiles.Count > 0) {
					LastFilePath = RecentFiles[0];
				}
			}
		}
		public static void AddRecentFile(string recent) {
			if (!RecentFiles.Contains(recent)) {
				RecentFiles.Add(recent);
			}
			SortRecentFiles(recent);
		}
		public static void SortRecentFiles(string? recent) {
			Log.Print($"Sorting {recent} to top of list.");
			if (RecentFiles.Contains(recent)) {
				RecentFiles.Remove(recent);
			}
			RecentFiles.Insert(0, recent);

			while (RecentFiles.Count >= 10) {
				Log.Print($"Removing excess file: {RecentFiles[RecentFiles.Count - 1]}");
				RecentFiles.RemoveAt(RecentFiles.Count - 1);
			}
		}
	}
}