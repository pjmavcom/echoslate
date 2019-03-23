using System;
using System.Windows;

namespace TODOList
{
	public partial class DlgOptions : Window
	{
		public bool AutoSave { get; set; }
		public bool GlobalHotkeys { get; set; }
		public bool AutoBackup { get; set; }
		public TimeSpan BackupTime { get; set; }
		public bool Result;
		public DlgOptions(bool autoSave, bool hotkeys, bool autoBackup, TimeSpan backupTime)
		{
			InitializeComponent();
			AutoSave = autoSave;
			GlobalHotkeys = hotkeys;
			AutoBackup = autoBackup;
			BackupTime = backupTime;

			cbAS.IsChecked = AutoSave;
			cbGHK.IsChecked = GlobalHotkeys;
			cbAB.IsChecked = AutoBackup;
			tbBT.Text = BackupTime.Minutes.ToString();
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
			AutoSave = (bool) cbAS.IsChecked;
			GlobalHotkeys = (bool) cbGHK.IsChecked;
			AutoBackup = (bool) cbAB.IsChecked;
			int backupTime = Convert.ToInt32(tbBT.Text);
			BackupTime = new TimeSpan(0, backupTime, 0);
			
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
