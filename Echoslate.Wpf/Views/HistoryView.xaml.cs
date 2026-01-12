using System.Windows.Controls;
using System.Windows.Input;

namespace Echoslate.Wpf.Views;

public partial class HistoryView : UserControl {
	public HistoryView() {
		InitializeComponent();
	}
	private void TitleTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
		if (sender is TextBox textBox) {
			textBox.SelectAll();
			e.Handled = true;
		}
	}
}