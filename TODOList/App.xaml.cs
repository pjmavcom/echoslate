using System;
using System.IO;
using System.Windows;
using Echoslate.ViewModels;
using Echoslate.Windows;

namespace Echoslate {
	public partial class App {
		private void Application_Startup(object sender, StartupEventArgs e) {
			AppSettings.Load();
			var mainVM = new MainWindowViewModel(AppSettings.Instance);
			var mainWindow = new MainWindow { DataContext = mainVM };

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