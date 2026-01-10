using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Echoslate.Windows;

public partial class AboutWindow : UserControl {
	public AboutWindow() {
		InitializeComponent();
	}
	private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
		Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
		e.Handled = true;
	}
}