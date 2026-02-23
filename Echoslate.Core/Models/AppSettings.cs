using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Echoslate.Core.Services;


namespace Echoslate.Core.Models;

public class AppSettings {
	private static readonly JsonSerializerOptions _options = new() {
		WriteIndented = true, // pretty print
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // optional
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	public bool ShowWelcomeWindow { get; set; }
	public ObservableCollection<string> RecentFiles { get; set; }
	[JsonIgnore] public string LastFilePath { get; set; }

	public double WindowLeft { get; set; }
	public double WindowTop { get; set; }
	public double WindowWidth { get; set; }
	public double WindowHeight { get; set; }
	public WindowState WindowState { get; set; }

	public TimeSpan PomoWorkTimerLength { get; set; }
	public TimeSpan PomoBreakTimerLength { get; set; }
	public bool GlobalHotkeysEnabled { get; set; }
	public TimeSpan BackupTime { get; set; }

	public int LastActiveTabIndex { get; set; }

	private static AppSettings _instance;
	public static AppSettings Instance => _instance ??= new AppSettings();


	public AppSettings() {
		LoadDefaults();
	}
	public void LoadDefaults() {
		RecentFiles = [];
		ShowWelcomeWindow = true;
		WindowLeft = 0;
		WindowTop = 0;
		WindowWidth = 1920;
		WindowHeight = 1080;
		WindowState = WindowState.Normal;
		PomoWorkTimerLength = new TimeSpan(0, 25, 0);
		PomoBreakTimerLength = new TimeSpan(0, 5, 0);
		GlobalHotkeysEnabled = false;
		BackupTime = new TimeSpan(0, 5, 0);
		LastActiveTabIndex = 1;
	}

	public static void Save() {
		AppPaths.EnsureFolder(AppPaths.AppDataFolder);
		if (Instance.RecentFiles.Count > 0) {
			Log.Print($"Setting LastFilePath: {Instance.RecentFiles[0]}");
			Instance.LastFilePath = Instance.RecentFiles[0];
		}

		Log.Print("Serializing data...");
		string json = JsonSerializer.Serialize(AppSettings.Instance, _options);

		Log.Print($"Writing settings to: {AppPaths.SettingsFile}");
		File.WriteAllText(AppPaths.SettingsFile, json);

		Log.Print("Settings saved.");
	}
	public static void Load() {
		Log.Print($"Loading AppSettings from {AppPaths.SettingsFile}");
		if (File.Exists(AppPaths.SettingsFile)) {
			try {
				string json = File.ReadAllText(AppPaths.SettingsFile);
				AppSettings? loaded = JsonSerializer.Deserialize<AppSettings>(json, _options);
				if (loaded != null) {
					Log.Print("AppSettings deserialized");
					Instance.ShowWelcomeWindow = loaded.ShowWelcomeWindow;
					Log.Print($"ShowWelcomeWindow: {Instance.ShowWelcomeWindow}");

					Instance.RecentFiles = loaded.RecentFiles;
					CleanRecentFiles();

					Instance.WindowLeft = loaded.WindowLeft;
					Instance.WindowTop = loaded.WindowTop;
					Log.Print($"Window Position: {Instance.WindowLeft}, {Instance.WindowTop}");
					Instance.WindowWidth = loaded.WindowWidth;
					if (Instance.WindowWidth == 0) {
						Log.Warn($"Setting default window width to {Instance.WindowWidth}.");
						Instance.WindowWidth = 1920;
					}
					Instance.WindowHeight = loaded.WindowHeight;
					if (Instance.WindowHeight == 0) {
						Log.Warn($"Setting default window height to {Instance.WindowHeight}.");
						Instance.WindowHeight = 1080;
					}
					Log.Print($"Window size: {Instance.WindowWidth}, {Instance.WindowHeight}");
					Instance.WindowState = loaded.WindowState;
					Log.Print($"Window State: {Instance.WindowState}");

					Instance.PomoWorkTimerLength = loaded.PomoWorkTimerLength;
					if (Instance.PomoWorkTimerLength == TimeSpan.Zero) {
						Instance.PomoWorkTimerLength = new TimeSpan(0, 25, 0);
						Log.Warn($"Setting PomoWorkTimerLength to default: {Instance.PomoWorkTimerLength}.");
					}
					Instance.PomoBreakTimerLength = loaded.PomoBreakTimerLength;
					if (Instance.PomoBreakTimerLength == TimeSpan.Zero) {
						Instance.PomoBreakTimerLength = new TimeSpan(0, 5, 0);
						Log.Warn($"Setting PomoBreakTimerLength to default: {Instance.PomoBreakTimerLength}.");
					}
					Log.Print($"PomoTimer set to: {Instance.PomoWorkTimerLength}/{Instance.PomoBreakTimerLength}");

					// Instance.GlobalHotkeysEnabled = loaded.GlobalHotkeysEnabled;
					Log.Print($"GlobalHotkeysEnabled: {Instance.GlobalHotkeysEnabled}. NOT ENABLED YET");

					Instance.BackupTime = loaded.BackupTime;
					if (Instance.BackupTime == TimeSpan.Zero) {
						Log.Warn($"Setting BackupTime to default: {Instance.BackupTime}.");
						Instance.BackupTime = new TimeSpan(0, 5, 0);
					}
					Log.Print($"Backup time: {Instance.BackupTime}");

					Instance.LastActiveTabIndex = loaded.LastActiveTabIndex;
					Log.Print($"LastActiveTabIndex: {Instance.LastActiveTabIndex}");
				}
			} catch (Exception ex) {
				Log.Error("Settings could not be loaded. Loading default settings");
				Instance.LoadDefaults();
				System.Diagnostics.Debug.WriteLine($"Settings load failed: {ex.Message}");
			}
			Log.Success("AppSettings loaded.");
		} else {
			Log.Warn("Settings file does not exist. Using defaults.");
			Instance.LoadDefaults();
		}
	}
	public void AddRecentFile(string recent) {
		if (!RecentFiles.Contains(recent)) {
			Log.Print($"Adding recent file: {recent}");
			RecentFiles.Add(recent);
		} else {
			Log.Print($"RecentFiles already contains file: {recent}");
		}
		Log.Print("Sorting RecentFiles...");
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
	public static void CleanRecentFiles() {
		Log.Print("Cleaning recent files...");
		var list = Instance.RecentFiles.ToList();
		foreach (string path in list) {
			if (!File.Exists(path)) {
				Log.Print($"File path does not exist. Removing: {path}...");
				Instance.RecentFiles.Remove(path);
			}
		}
		if (Instance.RecentFiles.Count > 0) {
			Instance.LastFilePath = Instance.RecentFiles.FirstOrDefault();
			Log.Print($"Setting LastFilePath to {Instance.LastFilePath}");
		} else {
			Log.Print("No recent files found. Setting ShowWelcomeWindow to true.");
			Instance.ShowWelcomeWindow = true;
		}
	}
}