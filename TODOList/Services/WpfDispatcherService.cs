using System;
using System.Threading.Tasks;
using System.Windows;
using Echoslate.Core.Services;

namespace Echoslate.Services;

public class WpfDispatcherService : IDispatcherService {
	public void Invoke(Action action) {
		Application.Current.Dispatcher.Invoke(action);
	}
	public async Task InvokeAsync(Action action) {
		await Application.Current.Dispatcher.InvokeAsync(action);
	}
}