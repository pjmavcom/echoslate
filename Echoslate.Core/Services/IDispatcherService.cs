namespace Echoslate.Core.Services;

public enum AppDispatcherPriority
{
    Normal,
    Background,
    Low,
    High,
	Render,
}

public interface IDispatcherService {
	void Invoke(Action action);
	Task InvokeAsync(Action action);
	Task InvokeAsync(Action action, AppDispatcherPriority priority, int delay = 0);
}