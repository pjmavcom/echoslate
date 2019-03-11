using System;
using System.Windows;

namespace TODOList
{
	public partial class AddTab : Window
	{
		public string Name;
		public bool Result;
		
		public AddTab()
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
		
		private void Ok_OnClick(object sender, EventArgs e)
		{
			Name = tbName.Text;
			Result = true;
			Close();
		}
		private void Cancel_OnClick(object sender, EventArgs e)
		{
			Result = false;
			Close();
		}
	}
}
