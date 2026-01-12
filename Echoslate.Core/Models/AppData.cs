using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Echoslate.Core.Models;

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