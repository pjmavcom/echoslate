using Echoslate.Core.ViewModels;

namespace Echoslate.Core.Services;

public interface IApplicationService {
	void Initialize(object mainWindow);
	void Shutdown();
	void Show();
	object? GetWindow();
}