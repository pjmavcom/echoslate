using System;
using System.IO;

namespace Echoslate;

public static class AppPaths {
	public static string AppDataFolder => Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"Echoslate");

	public static string SettingsFile => Path.Combine(AppDataFolder, "Echoslate.settings");

	public static void EnsureFolder() {
		if (!Directory.Exists(AppDataFolder))
			Directory.CreateDirectory(AppDataFolder);
	}
}