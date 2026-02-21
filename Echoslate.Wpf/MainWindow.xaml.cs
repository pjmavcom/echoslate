using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Echoslate.Core.Models;
using Application = System.Windows.Application;


namespace Echoslate.Wpf;

public partial class MainWindow : INotifyPropertyChanged {
	public int LastActiveTabIndex { get; set; }


	public MainWindow() {
		InitializeComponent();
		Log.Print("Window Initialized");

#if DEBUG
		PreviewKeyDown += (_, e) => {
			if (e.Key == Key.Escape) {
				Log.Print("ESC pressed closing app (debug shortcut)");
				Application.Current.Shutdown();
			}
		};
		mnuMain.Background = Brushes.Red;
#endif

		LastActiveTabIndex = AppSettings.Instance.LastActiveTabIndex;
		Closed += (s, e) => Window_OnClosed();
		Loaded += (s, e) => Window_OnLoaded();
	}
	private void Window_OnClosed() {
		AppSettings.Instance.LastActiveTabIndex = tabControl.SelectedIndex;
		AppSettings.Save();
	}
	private void Window_OnLoaded() {
		tabControl.SelectedIndex = LastActiveTabIndex;
		SetWindowPosition();
	}
	private void SetWindowPosition() {
		var mainWindow = Application.Current.MainWindow;

		mainWindow.Left = AppSettings.Instance.WindowLeft;
		mainWindow.Top = AppSettings.Instance.WindowTop;
		mainWindow.Width = AppSettings.Instance.WindowWidth;
		mainWindow.Height = AppSettings.Instance.WindowHeight;
		mainWindow.WindowState = AppSettings.Instance.WindowState switch {
			Core.Services.WindowState.Maximized => WindowState.Maximized,
			Core.Services.WindowState.Minimized => WindowState.Minimized,
			_ => WindowState.Normal
		};
	}
	public void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
		if (Keyboard.Modifiers == ModifierKeys.Alt) {
			Log.Debug(e.Key.ToString());
			Key actualKey = (e.Key == Key.System) ? e.SystemKey : e.Key;
			if (actualKey == Key.H) {
				SwitchTab(-1);
				e.Handled = true;
			} else if (actualKey == Key.L) {
				SwitchTab(1);
				e.Handled = true;
			}
		}
	}
	private void SwitchTab(int direction) {
		if (tabControl.Items.Count == 0) {
			return;
		}
		int newIndex = tabControl.SelectedIndex + direction;
		if (newIndex < 0) {
			newIndex = 0;
		} else if (newIndex >= tabControl.Items.Count) {
			newIndex = tabControl.Items.Count - 1;
		}
		tabControl.SelectedIndex = newIndex;
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}