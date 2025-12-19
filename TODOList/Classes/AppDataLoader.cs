using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Echoslate;

public class AppDataLoader {
	private readonly JsonSerializerOptions _options = new() {
																WriteIndented = true,
																PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
																DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
																PropertyNameCaseInsensitive = true
															};

	public AppData Data;

	public AppDataLoader(string? filePath, out AppData data) {
		Data = Load(filePath);
		data = Data;
		// data = new AppData { CurrentFilePath = filePath };
		// Data = data;
		// Load2_1SaveFile();
	}
	public AppData Load(string path) {
		if (!File.Exists(path)) {
			Log.Error($"File not found: {path}");
			return new AppData();
		}

		string json = File.ReadAllText(path);
		AppData? data = JsonSerializer.Deserialize<AppData>(json, _options);

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


	private void Load2_1SaveFile() {
		StreamReader stream = new StreamReader(File.Open(Data.CurrentFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

		string? line;
		while (true) {
			line = stream.ReadLine();
			if (line != null && line.Contains("=====TABS"))
				continue;
			if (line != null && line.Contains("=====FILESETTINGS"))
				break;

			Data.FiltersList.Add(line);
		}

		stream.ReadLine();
		stream.ReadLine();
		// _backupIncrement = Convert.ToInt16(stream.ReadLine());
		stream.ReadLine();
		stream.ReadLine();
		// int backupMinutes = Convert.ToInt16(stream.ReadLine());
		// _backupTime = new TimeSpan(0, backupMinutes, 0);
		stream.ReadLine();
		stream.ReadLine();
		// _autoBackup = Convert.ToBoolean(stream.ReadLine());
		stream.ReadLine();
		stream.ReadLine();
		// _autoSave = Convert.ToBoolean(stream.ReadLine());
		stream.ReadLine();
		stream.ReadLine();
		// ConvertProjectVersion(stream.ReadLine());

		while (true) {
			line = stream.ReadLine();
			if (line != null && line.Contains("=====VCS"))
				break;

			if (line != null && line.Contains("=====TODO"))
				continue;

			TodoItem td = TodoItem.Create(line);
			Data.TodoList.Add(td);
		}

		List<string> history = [];
		while (line != null) {
			line = stream.ReadLine();
			switch (line) {
				case "NewVCS":
					history = [];
					continue;
				case "EndVCS":
					Data.HistoryList.Add(HistoryItem.Create(history));
					continue;
				default:
					if (line != null) {
						history.Add(line);
					}
					break;
			}
		}
		stream.Close();
	}
}