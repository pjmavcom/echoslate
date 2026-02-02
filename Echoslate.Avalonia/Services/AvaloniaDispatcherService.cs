using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using Echoslate.Core.Services;

namespace Echoslate.Avalonia.Services;

public class AvaloniaDispatcherService : IDispatcherService {
	public void Invoke(Action action) {
		Dispatcher.UIThread.Invoke(action);
	}

	public Task InvokeAsync(Action action) {
		return Dispatcher.UIThread.InvokeAsync(action).GetTask();
	}
}