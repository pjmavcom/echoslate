using System.Windows;
using Echoslate.Core.Services;

namespace Echoslate.Services;

public class WpfApplicationService : IApplicationService {
	public void Shutdown() {
		Application.Current.Shutdown();
	}
	public object? GetMainWindow() => Application.Current.MainWindow;
}