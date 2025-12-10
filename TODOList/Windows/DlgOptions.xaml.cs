using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Echoslate
{
	public partial class DlgOptions
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

			int totalMinutes = BackupTime.Days * 24 * 60 +BackupTime.Hours * 60 + BackupTime.Minutes;
			iudBackupTime.Value = totalMinutes;
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
		private void ConvertBackupTime()
		{
			int totalMinutes = (int) iudBackupTime.Value;
//			int totalMinutes = Convert.ToInt32(tbBT.Text);
			int hours = totalMinutes / 60;
			int minutes = totalMinutes % 60;
			BackupTime = new TimeSpan(hours, minutes, 0);
		}
		private void Ok_OnClick(object sender, EventArgs e)
		{
			if (cbAS.IsChecked != null)
				AutoSave = (bool) cbAS.IsChecked;
			if (cbGHK.IsChecked != null)
				GlobalHotkeys = (bool) cbGHK.IsChecked;
			if (cbAB.IsChecked != null)
				AutoBackup = (bool) cbAB.IsChecked;
			ConvertBackupTime();
			
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
