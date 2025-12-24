using System;
using System.Windows;

namespace Echoslate
{
	public partial class DlgHelp
	{
		public DlgHelp()
		{
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
		public void CloseButton(object sender, EventArgs e) {
			Close();
		}
	}
}
