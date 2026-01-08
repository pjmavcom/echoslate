using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Echoslate.ViewModels;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using Echoslate.Services;
using Echoslate.Windows;

namespace Echoslate {
	public partial class App {
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			PresentationTraceSources.DataBindingSource.Listeners.Clear();
			PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
			PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;
			PresentationTraceSources.Refresh();
		}
		private void Application_Startup(object sender, StartupEventArgs e) {
			AppServices.Initialize(new WpfBrushService(), new WpfMessageDialogService());
			AppSettings.Load();
			// WpfMessageDialogService wpfMessageDialogService = new();
			GitHelper.Initialize(AppServices.MessageDialogService);
			// GitHelper.Initialize(wpfMessageDialogService);
			
			var mainVM = new MainWindowViewModel(AppSettings.Instance);
			var mainWindow = new MainWindow { DataContext = mainVM };
			mainWindow.Closing += mainVM.OnClosing;

			if (AppSettings.Instance.SkipWelcome && !string.IsNullOrEmpty(AppSettings.Instance.LastFilePath) && File.Exists(AppSettings.Instance.LastFilePath)) {
				mainVM.Load(AppSettings.Instance.LastFilePath);
				mainWindow.Show();
				return;
			}

			var welcomeWindow = new WelcomeWindow();
			var welcomeVM = new WelcomeViewModel(
				createNew: () => {
					mainVM.CreateNewFile();
					mainWindow.Show();
					welcomeWindow.Close();
				},
				openExisting: () => {
					if (mainVM.OpenFile()) {
						mainWindow.Show();
						welcomeWindow.Close();
					}
				});

			welcomeWindow.DataContext = welcomeVM;
			welcomeWindow.Closed += (s, args) => welcomeVM.SavePreference();

			welcomeWindow.ShowDialog();
		}
	}
}