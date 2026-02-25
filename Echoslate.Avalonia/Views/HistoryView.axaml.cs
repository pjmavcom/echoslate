using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Echoslate.Core.Models;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Views;

public partial class HistoryView : UserControl {
	private Grid? _mainGrid;
	private ColumnDefinition? _leftCol;
	private ColumnDefinition? _rightCol;
	private const double MinLeft = 1000;
	private const double MinLeft2 = 500;
	private const double MinRight = 600;
	private const double SwitchPoint = MinLeft + MinRight;

	private DataGridColumn? _colTypeHl;
	private DataGridColumn? _colVer;
	private DataGridColumn? _colDate;
	private DataGridColumn? _colType;
	private DataGridColumn? _colScope;


	public HistoryView() {
		InitializeComponent();
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);

		_mainGrid = this.FindControl<Grid>("MainGrid");
		if (_mainGrid is not null) {
			_leftCol = _mainGrid.ColumnDefinitions[0];
			_rightCol = _mainGrid.ColumnDefinitions[1];
		} else {
			Log.Error("Cannot find MainGrid");
		}

		DataGrid? historyViewDataGrid = this.FindControl<DataGrid>("HistoryViewDataGrid");
		if (historyViewDataGrid is not null) {
			_colTypeHl = historyViewDataGrid.Columns[0];
			_colVer = historyViewDataGrid.Columns[1];
			_colDate = historyViewDataGrid.Columns[2];
			_colType = historyViewDataGrid.Columns[3];
			_colScope = historyViewDataGrid.Columns[4];
		} else {
			Log.Error("Cannot find HistoryViewDataGrid");
		}
		SizeChanged += OnSizeChanged;
	}
	private void OnSizeChanged(object? sender, SizeChangedEventArgs e) {
		if (e.NewSize.Width <= 0) {
			return;
		}
		Log.Print($"Size: {_leftCol.Width.Value} - Width: {e.NewSize.Width}");
		UpdateColumnVisibility(e.NewSize.Width);
	}
	private void UpdateColumnVisibility(double width) {
		if (_leftCol == null || _rightCol == null) {
			Log.Error("LeftCol or RightCol is null");
			return;
		}
		if (width <= MinRight) {
			Log.Warn("MinWidth reached");
			return;
		}
		if (width >= SwitchPoint) {
			_leftCol.Width = new GridLength(MinLeft, GridUnitType.Pixel);
			_rightCol.Width = new GridLength(1, GridUnitType.Star);
		} else if (width >= MinLeft2 + MinRight) {
			_leftCol.Width = new GridLength(width - MinRight, GridUnitType.Pixel);
			_rightCol.Width = new GridLength(MinRight, GridUnitType.Pixel);
		} else {
			_leftCol.Width = new GridLength(MinLeft2, GridUnitType.Pixel);
			_rightCol.Width = new GridLength(width - MinLeft2, GridUnitType.Pixel);
		}
		_colTypeHl.IsVisible = _leftCol.Width.Value < 500 ? false : true;
		_colVer.IsVisible = _leftCol.Width.Value < 600 ? false : true;
		_colDate.IsVisible = _leftCol.Width.Value < 900 ? false : true;
		_colType.IsVisible = _leftCol.Width.Value < 700 ? false : true;
		_colScope.IsVisible = _leftCol.Width.Value < 800 ? false : true;
		if (DataContext is HistoryViewModel vm) {
			if (_leftCol.Width.Value < 850) {
				vm.IsTypeScope2Layer = true;
			} else {
				vm.IsTypeScope2Layer = false;
			}
		}
	}

	private void TitleTextBox_DoubleTapped(object sender, RoutedEventArgs e) {
		if (sender is TextBox textBox) {
			textBox.SelectAll();
			e.Handled = true;
		}
	}

}