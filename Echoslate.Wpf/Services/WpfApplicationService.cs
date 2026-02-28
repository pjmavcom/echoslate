using System.Reflection;
using System.Windows;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;

namespace Echoslate.Wpf.Services;

public class WpfApplicationService : IApplicationService {
	private Window _mainWindow;

	public void Initialize(object mainWindow) {
		_mainWindow = mainWindow as Window;
	}
	public void Shutdown() {
		Application.Current.Shutdown();
	}
	public void Show() {
		_mainWindow.Show();
	}
	public object? GetWindow() => _mainWindow;
	public string GetVersion() {
		var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
		var fileVersionAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
		if (fileVersionAttribute != null && !string.IsNullOrWhiteSpace(fileVersionAttribute.Version)) {
			Log.Print($"FileVersion: {fileVersionAttribute.Version}");
			return fileVersionAttribute.Version;
		}

		// Fallback to AssemblyVersion if FileVersion attribute missing
		Log.Print($"FileVersion: {assembly.GetName().Version}");
		return assembly.GetName().Version?.ToString() ?? "Unknown";
	}
}