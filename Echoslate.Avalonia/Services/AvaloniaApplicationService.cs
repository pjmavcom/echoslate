using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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
}