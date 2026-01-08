namespace Echoslate.Core.Services;

public interface IApplicationService {
	void Shutdown();
	object? GetMainWindow();
}