using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Echoslate.Avalonia.Views;

public partial class HistoryView : UserControl {
	public HistoryView() {
		InitializeComponent();
	}
	private void TitleTextBox_DoubleTapped(object sender, RoutedEventArgs e) {
		if (sender is TextBox textBox) {
			textBox.SelectAll();
			e.Handled = true;
		}
	}
}