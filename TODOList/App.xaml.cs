using System.IO;
using System.Windows;
using Echoslate.ViewModels;
using Echoslate.Windows;

namespace Echoslate {
	public partial class App {
		private void Application_Startup(object sender, StartupEventArgs e) {
			var appDataSettings = new AppDataSettings();
			AppDataSettings.LoadSettings();
			var mainVM = new MainWindowViewModel(appDataSettings);
			var mainWindow = new MainWindow { DataContext = mainVM };

			if (AppDataSettings.SkipWelcome && !string.IsNullOrEmpty(AppDataSettings.LastFilePath) && File.Exists(AppDataSettings.LastFilePath)) {
				mainVM.Load(AppDataSettings.LastFilePath);
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