using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;
using Echoslate.Services;
using Echoslate.Windows;
using Echoslate.WPF.Services;
using WindowState = Echoslate.Core.Services.WindowState;


namespace Echoslate {
	public partial class App {
		private MainWindow MainWindow;
		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);

			PresentationTraceSources.DataBindingSource.Listeners.Clear();
			PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
			PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;
			PresentationTraceSources.Refresh();
		}
		private void Application_Startup(object sender, StartupEventArgs e) {
			AppServices.Initialize(new WpfApplicationService(), new WpfBrushService(), new WpfMessageDialogService(), new WpfFileDialogService(Current.MainWindow), new WpfDispatcherService(), new WpfClipboardService(), new WpfDialogService(Current.MainWindow));
			AppSettings.Load();
			GitHelper.Initialize(AppServices.MessageDialogService);
			
			var mainVM = new MainWindowViewModel(AppSettings.Instance);
			MainWindow = new MainWindow { DataContext = mainVM };

			MainWindow.Closing += mainVM.OnClosing;
			MainWindow.Closing += SaveWindowProperties;

			if (AppSettings.Instance.SkipWelcome && !string.IsNullOrEmpty(AppSettings.Instance.LastFilePath) && File.Exists(AppSettings.Instance.LastFilePath)) {
				mainVM.Load(AppSettings.Instance.LastFilePath);
				MainWindow.Show();
				return;
			}

			var welcomeWindow = new WelcomeWindow();
			var welcomeVM = new WelcomeViewModel(
				createNew: () => {
					mainVM.CreateNewFile();
					MainWindow.Show();
					welcomeWindow.Close();
				},
				openExisting: () => {
					if (mainVM.OpenFile()) {
						MainWindow.Show();
						welcomeWindow.Close();
					}
				});

			welcomeWindow.DataContext = welcomeVM;
			welcomeWindow.Closed += (s, args) => welcomeVM.SavePreference();

			welcomeWindow.ShowDialog();
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
}