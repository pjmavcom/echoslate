using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Echoslate.Core.Services;


namespace Echoslate.Core.Models {
	public class AppSettings {
		private static readonly JsonSerializerOptions _options = new() {
			WriteIndented = true,                              // pretty print
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // optional
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		public bool SkipWelcome { get; set; }
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

		public string WindowTitle { get; set; }

		private static AppSettings _instance;
		public static AppSettings Instance => _instance ??= new AppSettings();

		private static readonly string SettingsFile = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"Echoslate",
			"settings.json");

		public AppSettings() {
			RecentFiles = [];
		}

		public static void Save() {
			AppPaths.EnsureFolder();
			Directory.CreateDirectory(Path.GetDirectoryName(SettingsFile)!);
			if (Instance.RecentFiles.Count > 0) {
				Instance.LastFilePath = Instance.RecentFiles[0];
			}
			string json = JsonSerializer.Serialize(AppSettings.Instance, _options);
			File.WriteAllText(SettingsFile, json);
		}
		public static void Load() {
			if (File.Exists(SettingsFile)) {
				try {
					string json = File.ReadAllText(SettingsFile);
					AppSettings? loaded = JsonSerializer.Deserialize<AppSettings>(json, _options);
					if (loaded != null) {
						Instance.SkipWelcome = loaded.SkipWelcome;
						Instance.RecentFiles = loaded.RecentFiles;
						CleanRecentFiles();

						Instance.WindowLeft = loaded.WindowLeft;
						Instance.WindowTop = loaded.WindowTop;
						Instance.WindowWidth = loaded.WindowWidth;
						if (Instance.WindowWidth == 0) {
							Instance.WindowWidth = 1920;
						}
						Instance.WindowHeight = loaded.WindowHeight;
						if (Instance.WindowHeight == 0) {
							Instance.WindowHeight = 1080;
						}
						Instance.WindowState = loaded.WindowState;

						Instance.PomoWorkTimerLength = loaded.PomoWorkTimerLength;
						if (Instance.PomoWorkTimerLength == TimeSpan.Zero) {
							Instance.PomoWorkTimerLength = new TimeSpan(0, 25, 0);
						}
						Instance.PomoBreakTimerLength = loaded.PomoBreakTimerLength;
						if (Instance.PomoBreakTimerLength == TimeSpan.Zero) {
							Instance.PomoBreakTimerLength = new TimeSpan(0, 5, 0);
						}
						Instance.GlobalHotkeysEnabled = loaded.GlobalHotkeysEnabled;
						Instance.BackupTime = loaded.BackupTime;
						if (Instance.BackupTime == TimeSpan.Zero) {
							Instance.BackupTime = new TimeSpan(0, 5, 0);
						}

						Instance.LastActiveTabIndex = loaded.LastActiveTabIndex;
						Instance.WindowTitle = loaded.WindowTitle;
					}
				} catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine($"Settings load failed: {ex.Message}");
				}
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
		public static void CleanRecentFiles() {
			var list = Instance.RecentFiles.ToList();
			foreach (string path in list) {
				if (!File.Exists(path)) {
					Instance.RecentFiles.Remove(path);
				}
			}
			if (Instance.RecentFiles.Count > 0) {
				Instance.LastFilePath = Instance.RecentFiles.FirstOrDefault();
			} else {
				Instance.SkipWelcome = false;
			}
		}
	}
}