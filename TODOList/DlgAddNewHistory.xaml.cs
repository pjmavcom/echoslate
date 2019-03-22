using System;
using System.Windows;

namespace TODOList
{
	public partial class DlgAddNewHistory : Window
	{
		public string ResultTitle;
		public bool Result;
		private bool _addDecimal;
		private int _numAfterDecimal;
		
		public DlgAddNewHistory(float version, float increment)
		{
			InitializeComponent();
			CenterWindowOnMouse();
			SetVersion(version, increment);
			tbTitle.SelectAll();
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
			ResultTitle = tbTitle.Text;
			Result = true;
			Close();
		}
		private void Cancel_OnClick(object sender, EventArgs e)
		{
			Result = false;
			Close();
		}
		private void SetVersion(float version, float increment)
		{
			int maxVersion = (int) Math.Floor(version + 1);
			float newVersion = CalculateNextVersion(version, maxVersion, increment);
			if (_numAfterDecimal > 0)
				tbTitle.Text = String.Format("v{0:0.00}.{1:000}", newVersion, _numAfterDecimal);
			else
				tbTitle.Text = String.Format("v{0:0.00}", newVersion);
		}
		private float CalculateNextVersion(float currentVersion, int maxVersion, float increment)
		{
			float nextVersion = currentVersion + increment;
			if (nextVersion >= maxVersion)
			{
				_addDecimal = true;
				_numAfterDecimal++;
				return CalculateNextVersion(currentVersion, maxVersion, increment / 10);
			}
			else
				return nextVersion;
		}
	}
}
