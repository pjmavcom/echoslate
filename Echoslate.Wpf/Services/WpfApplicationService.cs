using System.Windows;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;

namespace Echoslate.Wpf.Services;

public class WpfApplicationService : IApplicationService {
	public void Shutdown() {
		Application.Current.Shutdown();
	}
	public object? GetMainWindow() => Application.Current.MainWindow;
	public MainWindowViewModel? GetMainWindowViewModel() {
		if (Application.Current.MainWindow.DataContext is MainWindowViewModel vm) {
			return vm;
		}
		return null;
	}
	public void Show() {
		Application.Current.MainWindow.Show();
	}
}