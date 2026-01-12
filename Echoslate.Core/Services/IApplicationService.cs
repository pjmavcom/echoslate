using Echoslate.Core.ViewModels;

namespace Echoslate.Core.Services;

public interface IApplicationService {
	void Shutdown();
	object? GetMainWindow();
	MainWindowViewModel? GetMainWindowViewModel();
	void Show();
}