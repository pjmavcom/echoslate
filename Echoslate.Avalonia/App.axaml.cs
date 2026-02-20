using System;
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
	private DateTime _startupTime;
	private TimeSpan _finishTime;

	public override void Initialize() {
		_startupTime = DateTime.Now;
		Log.Initialize();

		Log.Print("Initializing BrushService...");
		AppServices.InitializeBrushService();
		AppServices.BrushService.SetBrushFactory((color) => new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B)));
		BrushServiceResourceExporter.ExportTo(this.Resources, AppServices.BrushService);

		Log.Print("Loading AvaloniaXAML...");
		AvaloniaXamlLoader.Load(this);
	}
	public override async void OnFrameworkInitializationCompleted() {
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			Log.Print("Loading AppSettings...");
			AppSettings.Load();

			Log.Print("Creating MainWindowViewModel mainVM...");
			MainWindowViewModel mainVM = new(AppSettings.Instance);

			Log.Print("Creating MainWindow...");
			MainWindow = new MainWindow {
				DataContext = mainVM
			};

			Log.Print("Initializing AppServices...");
			AppServices.Initialize(
				mainVM,
				new AvaloniaApplicationService(desktop),
				new AvaloniaDispatcherService(),
				new AvaloniaClipboardService(MainWindow),
				new AvaloniaDialogService(MainWindow)
			);
			AppServices.ApplicationService.Initialize(MainWindow);

			Log.Print("Setting MainWindow events...");
			MainWindow.Closing += mainVM.OnClosing;
			MainWindow.Closing += SaveWindowProperties;

			// ────────────────────────────────────────────────────────────────
			// Skip welcome → load last file and show main immediately
			// ────────────────────────────────────────────────────────────────
			if (!AppSettings.Instance.ShowWelcomeWindow &&
				!string.IsNullOrEmpty(AppSettings.Instance.LastFilePath) &&
				File.Exists(AppSettings.Instance.LastFilePath)) {
				Log.Print($"Loading last used file: {AppSettings.Instance.LastFilePath}");
				mainVM.Load(AppSettings.Instance.LastFilePath);

				desktop.MainWindow = MainWindow;
				Log.Print("Showing MainWindow...");
				AppServices.ApplicationService.Show();

				_finishTime = DateTime.Now - _startupTime;
				Log.Success($"Application ready for use. Startup Time: {_finishTime}");
				return;
			}

			// ────────────────────────────────────────────────────────────────
			// Show welcome as the FIRST main window
			// ────────────────────────────────────────────────────────────────
			Log.Print("Showing WelcomeWindow...");
			WelcomeViewModel vm = new WelcomeViewModel();
			WelcomeWindow view = new WelcomeWindow(vm);

			var welcomeWindow = new Window {
				Content = view,
				Title = "Welcome to Echoslate",
				WindowStartupLocation = WindowStartupLocation.CenterScreen,
				SizeToContent = SizeToContent.WidthAndHeight,
				ShowInTaskbar = true,
				CanResize = false
			};
			desktop.MainWindow = welcomeWindow;
			welcomeWindow.Show();

			Log.Print("Creating TCS");
			var tcs = new TaskCompletionSource<bool>();

			Log.Print("Hooking into close signal from WelcomeViewModel...");
			vm.RequestClose += success => {
				tcs.TrySetResult(success);
			};

			Log.Print("Waiting for result...");
			bool success = await tcs.Task;

			Log.Print("Removing hook.");
			vm.RequestClose -= _ => { };

			if (success) {
				Log.Print("User chose to proceed — swapping to MainWindow...");
				desktop.MainWindow = MainWindow;
				Log.Print("Showing MainWindow...");
				AppServices.ApplicationService.Show();
				Log.Success("Application ready for use.");
			} else {
				Log.Print("User canceled — shutting down...");
				AppServices.ApplicationService.Shutdown();
			}
		}

		base.OnFrameworkInitializationCompleted();
	}
	public void SaveWindowProperties(object? sender, CancelEventArgs cancelEventArgs) {
		Log.Print("Saving WindowProperties...");
		Window mainWindow = AppServices.ApplicationService.GetWindow() as Window;
		if (mainWindow != null) {
			AppSettings.Instance.WindowLeft = double.IsNaN(mainWindow.Position.X) ? 0 : mainWindow.Position.X;
			AppSettings.Instance.WindowTop = double.IsNaN(mainWindow.Position.Y) ? 0 : mainWindow.Position.Y;
			Log.Print($"Saving WindowPosition: {AppSettings.Instance.WindowLeft}, {AppSettings.Instance.WindowTop}");

			AppSettings.Instance.WindowWidth = mainWindow.Width;
			AppSettings.Instance.WindowHeight = mainWindow.Height;
			Log.Print($"Saving WindowSize: {AppSettings.Instance.WindowWidth}, {AppSettings.Instance.WindowHeight}");

			AppSettings.Instance.WindowState = mainWindow.WindowState switch {
				WindowState.Maximized => Core.Services.WindowState.Maximized,
				WindowState.Minimized => Core.Services.WindowState.Minimized,
				_ => Core.Services.WindowState.Normal
			};
			Log.Print($"Saving WindowState: {AppSettings.Instance.WindowState}");
		} else {
			Log.Error("MainWindow == null. Cannot save WindowProperties.");
		}
	}
}