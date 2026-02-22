using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Echoslate.Core.Services;

namespace Echoslate.Wpf.Services;

public class WpfDispatcherService : IDispatcherService {
	public void Invoke(Action action) {
		Application.Current.Dispatcher.Invoke(action);
	}
	public async Task InvokeAsync(Action action) {
		await Application.Current.Dispatcher.InvokeAsync(action);
	}
	public async Task InvokeAsync(Action action, AppDispatcherPriority priority, int delay = 0) {
		DispatcherPriority wpfPriority = priority switch {
			AppDispatcherPriority.Render => DispatcherPriority.Render,
			AppDispatcherPriority.Normal => DispatcherPriority.Normal,
			AppDispatcherPriority.Background => DispatcherPriority.Background,
			AppDispatcherPriority.Low => DispatcherPriority.ApplicationIdle,
			AppDispatcherPriority.High => DispatcherPriority.Send,
			_ => DispatcherPriority.Normal
		};
		if (delay > 0) {
			await Task.Delay(delay);
		}
		await Application.Current.Dispatcher.InvokeAsync(action, wpfPriority);
	}
}