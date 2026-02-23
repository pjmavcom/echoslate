using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using Echoslate.Core.Services;

namespace Echoslate.Avalonia.Services;

public class AvaloniaDispatcherService : IDispatcherService {
	public void Invoke(Action action) {
		Dispatcher.UIThread.Invoke(action);
	}

	public async Task InvokeAsync(Action action) {
		await Dispatcher.UIThread.InvokeAsync(action).GetTask();
	}
	public async Task InvokeAsync(Action action, AppDispatcherPriority priority, int delay = 0) {
		DispatcherPriority priorityValue = priority switch {
			AppDispatcherPriority.Render => DispatcherPriority.Render,
			AppDispatcherPriority.Normal => DispatcherPriority.Normal,
			AppDispatcherPriority.Background => DispatcherPriority.Background,
			AppDispatcherPriority.Low => DispatcherPriority.SystemIdle, // or ApplicationIdle
			AppDispatcherPriority.High => DispatcherPriority.Send, // or Input
			_ => DispatcherPriority.Normal
		};
		if (delay > 0) {
			await Task.Delay(delay);
		}
		await Dispatcher.UIThread.InvokeAsync(action, priorityValue).GetTask();
	}
}