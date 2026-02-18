namespace Echoslate.Core.Models;

public static class AppPaths {
	public static string AppDataFolder => Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"Echoslate");

	public static string SettingsFile => Path.Combine(AppDataFolder, "settings.json");

	public static void EnsureFolder(string folderName) {
		if (!Directory.Exists(folderName)) {
			Log.Print($"Creating folder {folderName}.");
			Directory.CreateDirectory(folderName);
			return;
		}
		Log.Print($"{folderName} exists.");
	}
}