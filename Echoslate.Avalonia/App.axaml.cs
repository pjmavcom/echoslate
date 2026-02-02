using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Echoslate.Avalonia.Services;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia;

public partial class App : Application {
	public override void Initialize() {
		AvaloniaXamlLoader.Load(this);

		
	}

	public override void OnFrameworkInitializationCompleted() {
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			AppSettings.Load();
			MainWindowViewModel mainVM = new(AppSettings.Instance);
			MainWindow mainWindow = new() { DataContext = mainVM };
			desktop.MainWindow = mainWindow;
			
			AppServices.Initialize(mainVM, new AvaloniaApplicationService(desktop), new AvaloniaDispatcherService(), new AvaloniaClipboardService(mainWindow), new AvaloniaDialogService(mainWindow));
			AppServices.BrushService.SetBrushFactory((color) => new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B)));
			AppServices.ApplicationService.Initialize(mainWindow);
		}

		base.OnFrameworkInitializationCompleted();
	}
}