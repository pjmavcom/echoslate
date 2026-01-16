using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;
using Echoslate.Wpf.Services;
using WindowState = Echoslate.Core.Services.WindowState;


namespace Echoslate.Wpf;

public partial class App {
	private MainWindow MainWindow;
	protected override void OnStartup(StartupEventArgs e) {
		base.OnStartup(e);

		PresentationTraceSources.DataBindingSource.Listeners.Clear();
		PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
		PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;
		PresentationTraceSources.Refresh();
	}
	private async void Application_Startup(object sender, StartupEventArgs e) {
		AppSettings.Load();

		var mainVM = new MainWindowViewModel(AppSettings.Instance);
		MainWindow = new MainWindow { DataContext = mainVM };
		AppServices.Initialize(mainVM, new WpfApplicationService(), new WpfBrushService(), new WpfDispatcherService(), new WpfClipboardService(), new WpfDialogService(MainWindow));
		AppServices.ApplicationService.Initialize(MainWindow);

		MainWindow.Closing += mainVM.OnClosing;
		MainWindow.Closing += SaveWindowProperties;

		if (AppSettings.Instance.SkipWelcome && !string.IsNullOrEmpty(AppSettings.Instance.LastFilePath) && File.Exists(AppSettings.Instance.LastFilePath)) {
			mainVM.Load(AppSettings.Instance.LastFilePath);
			AppServices.ApplicationService.Show();
			return;
		}

		bool success = await AppServices.DialogService.ShowWelcomeWindowAsync();
		if (success) {
			AppServices.ApplicationService.Show();
		} else {
			AppServices.ApplicationService.Shutdown();
		}
	}
	public void SaveWindowProperties(object? sender, CancelEventArgs cancelEventArgs) {
		if (MainWindow != null) {
			AppSettings.Instance.WindowLeft = double.IsNaN(MainWindow.Left) ? 0 : MainWindow.Left;
			AppSettings.Instance.WindowTop = double.IsNaN(MainWindow.Top) ? 0 : MainWindow.Top;
			AppSettings.Instance.WindowWidth = MainWindow.Width;
			AppSettings.Instance.WindowHeight = MainWindow.Height;
			AppSettings.Instance.WindowState = MainWindow.WindowState switch {
				System.Windows.WindowState.Maximized => WindowState.Maximized,
				System.Windows.WindowState.Minimized => WindowState.Minimized,
				_ => WindowState.Normal
			};
		}
	}
}