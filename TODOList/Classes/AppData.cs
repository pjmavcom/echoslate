using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;

namespace Echoslate;

public class AppData {
	public AppDataFileSettings FileSettings { get; set; }
	public ObservableCollection<TodoItem> TodoList { get; set; }
	public ObservableCollection<HistoryItem> HistoryList { get; set; }
	public ObservableCollection<string> FiltersList { get; set; }

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

		TodoList = [];
		HistoryList = [];
		FiltersList = [];
		_currentHistoryItem = null;
	}
}