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
using WindowState = Avalonia.Controls.WindowState;

namespace Echoslate.Avalonia;

public partial class App : Application {
	private MainWindow MainWindow;

	public override void Initialize() {
		AppServices.InitializeBrushService();
		AppServices.BrushService.SetBrushFactory((color) => new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B)));
		BrushServiceResourceExporter.ExportTo(this.Resources, AppServices.BrushService);
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
		Window mainWindow = AppServices.ApplicationService.GetWindow() as Window;
		if (mainWindow != null) {
			AppSettings.Instance.WindowLeft = double.IsNaN(mainWindow.Position.X) ? 0 : mainWindow.Position.X;
			AppSettings.Instance.WindowTop = double.IsNaN(mainWindow.Position.Y) ? 0 : mainWindow.Position.Y;
			AppSettings.Instance.WindowWidth = mainWindow.Width;
			AppSettings.Instance.WindowHeight = mainWindow.Height;
			AppSettings.Instance.WindowState = mainWindow.WindowState switch {
				WindowState.Maximized => Core.Services.WindowState.Maximized,
				WindowState.Minimized => Core.Services.WindowState.Minimized,
				_ => Core.Services.WindowState.Normal
			};
		}
	}
}