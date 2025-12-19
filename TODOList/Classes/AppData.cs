using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Echoslate;

public class AppData {
	public List<TodoItem> TodoList { get; set; }
	public List<HistoryItem> HistoryList { get; set; }
	public List<string> FiltersList { get; set; }

	[JsonIgnore]
	public string CurrentFilePath;
	[JsonIgnore]
	public string? BasePath => Path.GetDirectoryName(CurrentFilePath);
	[JsonIgnore]
	public string? FileName =>  Path.GetFileNameWithoutExtension(CurrentFilePath);
	[JsonIgnore]
	public string? FileExtension => Path.GetExtension(CurrentFilePath);

	private HistoryItem? _currentHistoryItem;
	[JsonIgnore]
	public HistoryItem? CurrentHistoryItem {
		get => HistoryList[0];
		set { _currentHistoryItem = value; }
	}


	public AppData() {
		CurrentFilePath = string.Empty;

		TodoList = [];
		HistoryList = [];
		FiltersList = [];
		_currentHistoryItem = null;
	}
}