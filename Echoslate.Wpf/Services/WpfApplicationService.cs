using System.Windows;
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
}