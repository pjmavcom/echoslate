using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using WindowState = Avalonia.Controls.WindowState;

namespace Echoslate.Avalonia;

public partial class MainWindow : Window {
	private const string PROGRAM_VERSION = "4.1.1.3";
	public const string DATE_STRING_FORMAT = "yyyyMMdd";
	public const string TIME_STRING_FORMAT = "HHmmss";

	private string WindowTitle => "Echoslate v" + PROGRAM_VERSION;
	public int LastActiveTabIndex { get; set; }


	public MainWindow() {
		InitializeComponent();

#if DEBUG
		mnuMain.Background = Brushes.Red;
#endif

		LastActiveTabIndex = AppSettings.Instance.LastActiveTabIndex;
		AppSettings.Instance.WindowTitle = WindowTitle;
		KeyDown += OnKeyDown;
		Closed += (s, e) => Window_OnClosed();
		Loaded += (s, e) => Window_OnLoaded();
		Log.Success("Window Initialized");
	}
	private void OnKeyDown(object? sender, KeyEventArgs e) {
#if DEBUG
		if (e.Key == Key.Escape) {
			Log.Print("ESC pressed closing app (debug shortcut)");
			Close();
			e.Handled = true;
		}
#endif
		if (e.KeyModifiers == KeyModifiers.Alt) {
			Log.Debug(e.Key.ToString());
			if (e.Key == Key.H) {
				SwitchTab(-1);
				e.Handled = true;
			} else if (e.Key == Key.L) {
				SwitchTab(1);
				e.Handled = true;
			}
		}
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
		Window mainWindow = AppServices.ApplicationService.GetWindow() as Window;
		
		mainWindow.Position = new PixelPoint((int)AppSettings.Instance.WindowLeft, (int)AppSettings.Instance.WindowTop);
		mainWindow.Width = AppSettings.Instance.WindowWidth;
		mainWindow.Height = AppSettings.Instance.WindowHeight;
		mainWindow.WindowState = AppSettings.Instance.WindowState switch {
			Core.Services.WindowState.Maximized => WindowState.Maximized,
			Core.Services.WindowState.Minimized => WindowState.Minimized,
			_ => WindowState.Normal
		};
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
}