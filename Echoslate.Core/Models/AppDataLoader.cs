using System.Text.Json;
using System.Text.Json.Serialization;
using Echoslate.Core.Services;

namespace Echoslate.Core.Models;

public class AppDataLoader {
	private static readonly JsonSerializerOptions Options = new() {
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		PropertyNameCaseInsensitive = true
	};

	public static AppData Load(string path, AppData? prevData) {
		Log.Print($"Loading {path}");
		if (!File.Exists(path)) {
			Log.Error($"File not found: {path}");
			AppServices.DialogService.Show($"File not found: {path}", "Error", DialogButton.Ok, DialogIcon.Error);
			if (prevData != null) {
				Log.Error($"Reloading previous data: {prevData}");
				return prevData;
			}
			Log.Error("Creating new AppData");
			return new AppData();
		}

		string json = File.ReadAllText(path);
		AppData? data = JsonSerializer.Deserialize<AppData>(json, Options);

#if DEBUG
		Log.Debug("Disabling AutoSave and AutoBackup while debugging");
		data.FileSettings.AutoSave = false;
		data.FileSettings.AutoBackup = false;
#endif

		if (data == null) {
			Log.Error($"Deserialization failed: {path}");
			if (prevData != null) {
				Log.Error($"Reloading previous data: {prevData}");
				return prevData;
			}
			Log.Error("Creating new AppData");
			return new AppData();
		}

		Log.Print($"Deserialization succeeded: {path}");
		data.CurrentFilePath = path;

		Log.Print("Getting current HistoryItem...");
		data.CurrentHistoryItem = data.HistoryList.FirstOrDefault(h => !h.IsCommitted);
		if (data.CurrentHistoryItem == null) {
			Log.Print("No CurrentHistoryItem available. Creating new HistoryItem");

			Version version = new Version("0.1.2.3");
			if (data.HistoryList.FirstOrDefault() != null) {
				version = data.HistoryList.FirstOrDefault().Version;
			}

			data.CurrentHistoryItem = new HistoryItem {
				Title = "",
				IsCommitted = false,
				Version = version
			};
			data.HistoryList.Insert(0, data.CurrentHistoryItem);
		}

		Log.Print("Initializing CompletedTodoItems...");
		foreach (var history in data.HistoryList) {
			history.CompletedTodoItems ??= [];
		}
		
		data.DebugFiltersList();

		Log.Print($"Git path: {data.FileSettings.GitRepoPath}");
		Log.Success("AppData loaded.");
		return data;
	}
}