using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Echoslate.Core.Models;
using Echoslate.Core.Services;

namespace Echoslate.Avalonia.Services;

public class AvaloniaApplicationService : IApplicationService {
	private Window _mainWindow;
	private readonly IClassicDesktopStyleApplicationLifetime _lifetime;

	public AvaloniaApplicationService(IClassicDesktopStyleApplicationLifetime lifetime) {
		_lifetime = lifetime;
	}
	public void Initialize(object mainWindow) {
		_mainWindow = mainWindow as Window;
		if (_mainWindow != null) {
			_lifetime.MainWindow = _mainWindow;
		}
	}
	public void Shutdown() {
		_lifetime.Shutdown();
	}
	public void Show() {
		_mainWindow?.Show();
	}
	public object GetWindow() => _mainWindow;
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