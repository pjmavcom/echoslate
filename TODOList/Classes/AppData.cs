using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Echoslate;

public class AppData {
	public AppDataFileSettings FileSettings { get; set; }
	public ObservableCollection<TodoItem> TodoList { get; set; }
	public ObservableCollection<HistoryItem> HistoryList { get; set; }
	public ObservableCollection<string> FiltersList { get; set; }
	public ObservableCollection<string> CommitScopes { get; set; }

	[JsonIgnore] public HashSet<string> AllTags { get; set; }
	[JsonIgnore] public string CurrentFilePath;
	[JsonIgnore] public string? BasePath => Path.GetDirectoryName(CurrentFilePath);
	[JsonIgnore] public string? FileName => Path.GetFileNameWithoutExtension(CurrentFilePath);
	[JsonIgnore] public string? FileExtension => Path.GetExtension(CurrentFilePath);

	private HistoryItem? _currentHistoryItem;
	[JsonIgnore]
	public HistoryItem? CurrentHistoryItem {
		get {
			if (HistoryList.Count > 0) {
				return HistoryList[0];
			} else {
				return new HistoryItem();
			}
		}
		set { _currentHistoryItem = value; }
	}


	public AppData() {
		CurrentFilePath = string.Empty;
		FileSettings = new AppDataFileSettings();

		AllTags = [];
		TodoList = [];
		HistoryList = [];
		FiltersList = [];
		CommitScopes = [];
		_currentHistoryItem = null;
	}
	public string SuggestRepoPath() {
		string currentDir = Path.GetDirectoryName(CurrentFilePath);

		var dir = new DirectoryInfo(currentDir);
		while (dir != null) {
			if (Directory.Exists(Path.Combine(dir.FullName, ".git"))) {
				return dir.FullName;
			}
			dir = dir.Parent;
		}
		return null;
	}
	public void OnDataFileLoadedOrSaved() {
		string suggested = SuggestRepoPath();
		if (!string.IsNullOrEmpty(suggested) && string.IsNullOrEmpty(FileSettings.GitRepoPath)) {
			var result = MessageBox.Show($"Git repository detected at:\n{suggested}\nUse this path for branch detection and scope suggestions?",
				"Git Repository Found",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result == DialogResult.Yes) {
				FileSettings.GitRepoPath = suggested;
				FileSettings.UpdateGitFeaturesState();
			}
		}
	}
	public void DebugFiltersList() {
		FiltersList.CollectionChanged += (s, e) =>
		{
			Log.Debug($"[Filters] CollectionChanged: Action={e.Action}");
			if (e.OldItems != null)
				foreach (var item in e.OldItems)
					Log.Debug($"  Removed: {item}");
			if (e.NewItems != null)
				foreach (var item in e.NewItems)
					Log.Debug($"  Added: {item}");

			Log.Debug($"  Current count: {FiltersList.Count}");
			Log.Debug($"  Items: [{string.Join(", ", FiltersList)}]");
		};
	}
}