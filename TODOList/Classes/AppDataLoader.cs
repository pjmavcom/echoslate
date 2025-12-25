using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Echoslate.ViewModels;

namespace Echoslate;

public class AppDataLoader {
	private static readonly JsonSerializerOptions Options = new() {
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		PropertyNameCaseInsensitive = true
	};

	public static AppData Load(string path) {
		return LoadNew(path);
	}
	public static AppData LoadNew(string path) {
		if (!File.Exists(path)) {
			Log.Error($"File not found: {path}");
			return new AppData();
		}

		string json = File.ReadAllText(path);
		AppData? data = JsonSerializer.Deserialize<AppData>(json, Options);

#if DEBUG
		data.FileSettings.AutoSave = false;
		data.FileSettings.AutoBackup = false;
#endif

		if (data == null) {
			Log.Error($"Deserialization failed: {path}");
			return new AppData();
		}

		data.CurrentHistoryItem = data.HistoryList.FirstOrDefault(h => !h.IsCommitted);

		if (data.CurrentHistoryItem == null) {
			Version lastVersion;
			if (data.HistoryList.FirstOrDefault() != null) {
				lastVersion = data.HistoryList.FirstOrDefault().Version;
			}
			data.CurrentHistoryItem = new HistoryItem {
				Title = "",
				IsCommitted = false,
				Version = new Version("0.1.2.3")
			};
			data.HistoryList.Insert(0, data.CurrentHistoryItem);
		}

		foreach (var history in data.HistoryList) {
			history.CompletedTodoItems ??= [];
		}
		data.CurrentFilePath = path;
		data.DebugFiltersList();
		return data;
	}
}