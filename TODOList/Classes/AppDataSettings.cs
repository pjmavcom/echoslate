using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace Echoslate {
	public class AppDataSettings {
		private static readonly JsonSerializerOptions Options = new() {
																		  WriteIndented = true,
																		  PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
																		  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
																		  PropertyNameCaseInsensitive = true
																	  };

		[JsonIgnore] public string SettingsFileName { get; set; }
		public ObservableCollection<string> RecentFiles { get; set; }

		public double WindowLeft { get; set; }
		public double WindowTop { get; set; }
		public double WindowWidth { get; set; }
		public double WindowHeight { get; set; }
		public WindowState WindowState { get; set; } = WindowState.Normal;

		public int PomoWorkTimerLength { get; set; }
		public int PomoBreakTimerLength { get; set; }
		public bool GlobalHotkeysEnabled { get; set; }

		[JsonIgnore] public string WindowTitle { get; set; }

		public AppDataSettings() {
			SettingsFileName = "Echoslate.Settings";
			RecentFiles = [];
			WindowLeft = 0;
			WindowTop = 0;
			WindowWidth = 1920;
			WindowHeight = 1080;
			WindowState = WindowState.Normal;

			PomoWorkTimerLength = 25;
			PomoBreakTimerLength = 5;
			WindowTitle = string.Empty;
		}

		public static AppDataSettings Create(string windowTitle) {
			return new AppDataSettings { WindowTitle = windowTitle };
		}

		public void SaveSettings() {
			var mainWindow = Application.Current.MainWindow;
			if (mainWindow != null) {
				WindowLeft = double.IsNaN(mainWindow.Left) ? 0 : mainWindow.Left;
				WindowTop = double.IsNaN(mainWindow.Top) ? 0 : mainWindow.Top;
				WindowWidth = mainWindow.Width;
				WindowHeight = mainWindow.Height;
				WindowState = mainWindow.WindowState;
			}
			
			string filePath = AppDomain.CurrentDomain.BaseDirectory + SettingsFileName;
			Log.Print($"Saving settings file: {filePath}");

			string json = JsonSerializer.Serialize(this, Options);
			File.WriteAllText(filePath, json);
		}
		public void LoadSettings() {
			string filePath = AppDomain.CurrentDomain.BaseDirectory + SettingsFileName;
			if (!File.Exists(filePath)) {
				Log.Warn("Settings file does not exist. Creating new settings file.");
				SaveSettings();
				return;
			}

			Log.Print($"Loading settings file: {filePath}");
			string json = File.ReadAllText(filePath);
			AppDataSettings? settings = JsonSerializer.Deserialize<AppDataSettings>(json, Options);

			if (settings != null) {
				RecentFiles = new ObservableCollection<string>(settings.RecentFiles);
				
				WindowLeft = settings.WindowLeft;
				WindowTop = settings.WindowTop;
				WindowWidth = settings.WindowWidth;
				WindowHeight = settings.WindowHeight;
				WindowState = settings.WindowState;
				
				PomoWorkTimerLength = settings.PomoWorkTimerLength;
				PomoBreakTimerLength = settings.PomoBreakTimerLength;
				GlobalHotkeysEnabled = settings.GlobalHotkeysEnabled;
			} else {
				Log.Warn("Settings file is corrupted. Creating new settings file.");
				SaveSettings();
			}
		}
		public void AddRecentFile(string recent) {
			if (!RecentFiles.Contains(recent)) {
				RecentFiles.Add(recent);
			}
			SortRecentFiles(recent);
		}
		public void SortRecentFiles(string? recent) {
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