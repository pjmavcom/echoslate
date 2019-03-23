using System;
using System.Windows;

namespace TODOList
{
	public partial class DlgAddNewHistory
	{
		public string ResultTitle;
		public bool Result;
		private int _numAfterDecimal;
		
		public DlgAddNewHistory(float version, float increment)
		{
			InitializeComponent();
			SetVersion(version, increment);
			tbTitle.SelectAll();
			
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
		private void SetVersion(float version, float increment)
		{
			int maxVersion = (int) Math.Floor(version + 1);
			float newVersion = CalculateNextVersion(version, maxVersion, increment);
			tbTitle.Text = _numAfterDecimal > 0
				? $"v{newVersion:0.00}.{_numAfterDecimal:000}"
				: $"v{newVersion:0.00}";
		}
		private float CalculateNextVersion(float currentVersion, int maxVersion, float increment)
		{
			float nextVersion = currentVersion + increment;
			if (!(nextVersion >= maxVersion))
				return nextVersion;
			_numAfterDecimal++;
			return CalculateNextVersion(currentVersion, maxVersion, increment / 10);
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
	}
}
