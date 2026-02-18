using System.Text.Json;
using System.Text.Json.Serialization;

namespace Echoslate.Core.Models;

public class AppDataSaver {
	private readonly JsonSerializerOptions _options = new() {
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	public void Save(string path, AppData data) {
		if (!data.FileSettings.AutoSave) {
			Log.Warn("Attempting to save, but Auto save is not enabled");
			return;
		}
		Log.Print($"Saving {data.FileName} to {path}");

		data.CurrentFilePath = path;
		string json = JsonSerializer.Serialize(data, _options);
		Log.Print("Data serialized.");

		string? directory = Path.GetDirectoryName(path);
		Log.Print($"Finding directory: {directory}");
		if (!string.IsNullOrEmpty(directory)) {
			AppPaths.EnsureFolder(directory);
		}

		Log.Print($"Writing {data.FileName} to {path}...");
		File.WriteAllText(path, json);
		Log.Print($"Saved {data.FileName} to {path}.");
	}
}