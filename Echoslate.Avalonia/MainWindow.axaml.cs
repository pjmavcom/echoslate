using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;
using WindowState = Avalonia.Controls.WindowState;

namespace Echoslate.Avalonia;

public partial class MainWindow : Window {
	public int LastActiveTabIndex { get; set; }
	private Menu? _mainMenu;
	private StackPanel? _pomoPanel;
	private Grid? _pomoProgressBarGrid;
	private ProgressBar? _pomoProgressBar;
	private const double MaxPomoPanelWidth = 800;
	private const double MinPomoWidth = 100;
	private const double PomoControlsWidth = 400;


	public MainWindow() {
		InitializeComponent();

#if DEBUG
		mnuMain.Background = Brushes.Red;
#endif

		LastActiveTabIndex = AppSettings.Instance.LastActiveTabIndex;

		_mainMenu = this.FindControl<Menu>("mnuMain");
		_pomoPanel = this.FindControl<StackPanel>("PomoPanel");
		_pomoProgressBarGrid = this.FindControl<Grid>("PomoProgressBarGrid");
		_pomoProgressBar = this.FindControl<ProgressBar>("PomoProgressBar");

		KeyDown += OnKeyDown;
		Closed += (s, e) => Window_OnClosed();
		Loaded += (s, e) => Window_OnLoaded();
		SizeChanged += (s, e) => Window_OnSizeChanged(s, e);
		Log.Success("Window Initialized");
	}
	private void Window_OnSizeChanged(object? sender, SizeChangedEventArgs e) {
		if (_pomoProgressBar == null || _pomoProgressBarGrid == null || _pomoPanel == null || _mainMenu == null) {
			Log.Error("Pomo controls are null");
			return;
		}

		double width = e.NewSize.Width;
		if (width <= _mainMenu.Bounds.Width) {
			return;
		}
		if (DataContext is MainWindowViewModel mainWindowVM) {
			if (width - _mainMenu.Bounds.Width > MaxPomoPanelWidth) {
				mainWindowVM.ArePomoControlsVisible = true;
				_pomoPanel.Width = MaxPomoPanelWidth;
				_pomoProgressBarGrid.Width = MaxPomoPanelWidth - PomoControlsWidth;
				_pomoProgressBar.Width = MaxPomoPanelWidth - PomoControlsWidth;
			} else {
				mainWindowVM.ArePomoControlsVisible = false;
				_pomoPanel.Width = width - _mainMenu.Bounds.Width;
				_pomoProgressBarGrid.Width = _pomoPanel.Width;
				_pomoProgressBar.Width = _pomoPanel.Width;
			}
		}
		if (_pomoPanel.Width <= MinPomoWidth) {
			_pomoPanel.IsVisible = false;
		} else {
			_pomoPanel.IsVisible = true;
		}
	}
	private void OnKeyDown(object? sender, KeyEventArgs e) {
#if DEBUG
		if (e.Key == Key.Escape) {
			Log.Print("ESC pressed closing app (debug shortcut)");
			Log.Shutdown();
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
		Log.Shutdown();
	}
	private void Window_OnLoaded() {
		tabControl.SelectedIndex = LastActiveTabIndex;
		if (DataContext is MainWindowViewModel mainWindowVM) {
			mainWindowVM.SetWindowPosition();
		}
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