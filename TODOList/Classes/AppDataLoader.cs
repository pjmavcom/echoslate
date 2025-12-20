using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Echoslate;

public class AppDataLoader {
	private static readonly JsonSerializerOptions Options = new() {
																WriteIndented = true,
																PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
																DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
																PropertyNameCaseInsensitive = true
															};

	public AppData Data;

	public AppDataLoader() {
		// Data = Load(filePath);
		// data = Data;
		// data = new AppData { CurrentFilePath = filePath };
		// Data = data;
		// Load2_1SaveFile();
	}
	public static AppData Load(string path) {
		return LoadNew(path);
		// return Load2_1SaveFile(path);
	}
	public static AppData LoadNew(string path) {
		if (!File.Exists(path)) {
			Log.Error($"File not found: {path}");
			return new AppData();
		}

		string json = File.ReadAllText(path);
		AppData? data = JsonSerializer.Deserialize<AppData>(json, Options);

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
		return data;
	}


	public static AppData Load2_1SaveFile(string path) {
		AppData data = new AppData { CurrentFilePath = path };
		StreamReader stream = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

		string? line;
		while (true) {
			line = stream.ReadLine();
			if (line != null && line.Contains("=====TABS"))
				continue;
			if (line != null && line.Contains("=====FILESETTINGS"))
				break;

			data.FiltersList.Add(line);
		}

		stream.ReadLine();
		data.FileSettings.BackupIncrement = Convert.ToInt16(stream.ReadLine());
		stream.ReadLine();
		data.FileSettings.BackupTime = Convert.ToInt16(stream.ReadLine());
		stream.ReadLine();
		data.FileSettings.AutoBackup = Convert.ToBoolean(stream.ReadLine());
		stream.ReadLine();
		data.FileSettings.AutoSave = Convert.ToBoolean(stream.ReadLine());
		stream.ReadLine(); 
		data.FileSettings.CurrentProjectVersion = ConvertProjectVersion(stream.ReadLine());

		while (true) {
			line = stream.ReadLine();
			if (line != null && line.Contains("=====VCS"))
				break;

			if (line != null && line.Contains("=====TODO"))
				continue;

			TodoItem td = TodoItem.Create(line);
			data.TodoList.Add(td);
		}

		List<string> history = [];
		while (line != null) {
			line = stream.ReadLine();
			switch (line) {
				case "NewVCS":
					history = [];
					continue;
				case "EndVCS":
					data.HistoryList.Add(HistoryItem.Create(history));
					continue;
				default:
					if (line != null) {
						history.Add(line);
					}
					break;
			}
		}
		stream.Close();
		return data;
	}
	public static Version ConvertProjectVersion(string version) {
		string[] v = version.Split('.');
		 return new Version(Convert.ToInt16(v[0]), Convert.ToInt16(v[1]), Convert.ToInt16(v[2]), Convert.ToInt16(v[3]));
	}
}