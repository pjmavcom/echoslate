using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Echoslate.Wpf.Windows;

public partial class AboutWindow : UserControl {
	public string Version { get; set; }
		
	
	public AboutWindow(string version) {
		InitializeComponent();
		
		Version = version;
	}
	private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
		Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
		e.Handled = true;
	}
}