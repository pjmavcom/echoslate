using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace Echoslate.Windows;

public partial class AboutWindow : Window {
	public AboutWindow() {
		InitializeComponent();
			CenterWindowOnMouse();
		}
		private void CenterWindowOnMouse()
		{
			Window win = Application.Current.MainWindow;

			if (win == null)
				return;
			double centerX = win.Width / 2 + win.Left;
			double centerY = win.Height / 2 + win.Top;
			Left = centerX - Width / 2;
			Top = centerY - Height / 2;
		}
	private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
	{
		Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
		e.Handled = true;
	}
	public void CloseButton(object sender, EventArgs e) {
		Close();
	}
}