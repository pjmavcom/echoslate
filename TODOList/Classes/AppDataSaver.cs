using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Echoslate;

public class AppDataSaver {
	private readonly JsonSerializerOptions _options = new() {
																WriteIndented = true, // pretty print
																PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // optional
																DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
															};

	public void Save(string path, AppData data) {
		if (!data.FileSettings.AutoSave) {
			return;
		}
		
		data.CurrentFilePath = path;
		string json = JsonSerializer.Serialize(data, _options);

		string? directory = Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(directory))
			Directory.CreateDirectory(directory);

		File.WriteAllText(path, json);
	}
}