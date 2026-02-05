using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Echoslate.Avalonia.Services;
using Echoslate.Avalonia.Theming;
using Echoslate.Avalonia.Windows;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia;

public partial class App : Application {
	private MainWindow MainWindow;

	public override void Initialize() {
		AvaloniaXamlLoader.Load(this);
	}
	public async override void OnFrameworkInitializationCompleted() {
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			AppSettings.Load();

			MainWindowViewModel mainVM = new(AppSettings.Instance);
			MainWindow = new() {
				DataContext = mainVM
			};
			desktop.MainWindow = MainWindow;

			AppServices.Initialize(mainVM, new AvaloniaApplicationService(desktop), new AvaloniaDispatcherService(), new AvaloniaClipboardService(MainWindow), new AvaloniaDialogService(MainWindow));
			AppServices.BrushService.SetBrushFactory((color) => new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B)));
			BrushServiceResourceExporter.ExportTo(this.Resources, AppServices.BrushService);
			
			AppServices.ApplicationService.Initialize(MainWindow);

			MainWindow.Closing += mainVM.OnClosing;
			MainWindow.Closing += SaveWindowProperties;

			if (AppSettings.Instance.SkipWelcome && !string.IsNullOrEmpty(AppSettings.Instance.LastFilePath) && File.Exists(AppSettings.Instance.LastFilePath)) {
				mainVM.Load(AppSettings.Instance.LastFilePath);
				AppServices.ApplicationService.Show();
				return;
			}

			WelcomeViewModel vm = new WelcomeViewModel();
			WelcomeWindow view = new WelcomeWindow(vm);
			var window = new Window {
				Content = view,
				Title = "Welcome to Echoslate",
				WindowStartupLocation = WindowStartupLocation.CenterScreen,
				SizeToContent = SizeToContent.WidthAndHeight,
				ShowInTaskbar = true,
				CanResize = false
			};
			desktop.MainWindow = window;
			window.Show();
			await WaitForCloseAsync(window);
			if (view.Result) {
				desktop.MainWindow = MainWindow;
				AppServices.ApplicationService.Show();
			} else {
				AppServices.ApplicationService.Shutdown();
			}
		}

		base.OnFrameworkInitializationCompleted();
	}
	private static Task WaitForCloseAsync(Window w) {
		var tcs = new TaskCompletionSource();
		w.Closed += (_, _) => tcs.TrySetResult();
		return tcs.Task;
	}
	public void SaveWindowProperties(object? sender, CancelEventArgs cancelEventArgs) {
		// if (MainWindow != null) {
		// 	AppSettings.Instance.WindowLeft = double.IsNaN(MainWindow.Left) ? 0 : MainWindow.Left;
		// 	AppSettings.Instance.WindowTop = double.IsNaN(MainWindow.Top) ? 0 : MainWindow.Top;
		// 	AppSettings.Instance.WindowWidth = MainWindow.Width;
		// 	AppSettings.Instance.WindowHeight = MainWindow.Height;
		// 	AppSettings.Instance.WindowState = MainWindow.WindowState switch {
		// 		System.Windows.WindowState.Maximized => WindowState.Maximized,
		// 		System.Windows.WindowState.Minimized => WindowState.Minimized,
		// 		_ => WindowState.Normal
		// 	};
		// }
	}
}