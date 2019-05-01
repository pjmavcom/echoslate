using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace TODOList
{
	public partial class DlgOptions
	{
		public bool AutoSave { get; set; }
		public bool GlobalHotkeys { get; set; }
		public bool AutoBackup { get; set; }
		public TimeSpan BackupTime { get; set; }
		public float CurrentProjectVersion { get; set; }
		public float ProjectVersionIncrement { get; set; }
		public bool Result;
		
		public DlgOptions(bool autoSave, bool hotkeys, bool autoBackup, TimeSpan backupTime, float currentProjectVersion, float projectVersionIncrement)
		{
			InitializeComponent();
			AutoSave = autoSave;
			GlobalHotkeys = hotkeys;
			AutoBackup = autoBackup;
			BackupTime = backupTime;
			CurrentProjectVersion = currentProjectVersion;
			ProjectVersionIncrement = projectVersionIncrement;

			cbAS.IsChecked = AutoSave;
			cbGHK.IsChecked = GlobalHotkeys;
			cbAB.IsChecked = AutoBackup;

			int totalMinutes = BackupTime.Days * 24 * 60 +BackupTime.Hours * 60 + BackupTime.Minutes;
			iudBackupTime.Value = totalMinutes;
			iudCPV.Value = CurrentProjectVersion;
			iudPVI.Value = ProjectVersionIncrement;
			iudCPV.Increment = ProjectVersionIncrement;
			iudPVI.Increment = 0.01f;
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
		private void PCI_OnValueChanged(object sender, EventArgs e)
		{
			iudCPV.Increment = iudPVI.Value;
		}
//		private void BT_OnValueChanged(object sender, TextCompositionEventArgs e)
//		{
////			if (!(sender is TextBox tb))
////				return;
////			var fullText = tb.Text.Insert(tb.SelectionStart, e.Text);
////			e.Handled = !Int32.TryParse(fullText, out _);
//			totalMinutes = 
//		}
		private void CPV_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!(sender is TextBox tb))
				return;
			var fullText = tb.Text.Insert(tb.SelectionStart, e.Text);
			e.Handled = !float.TryParse(fullText, out _);
		}
		private void PVI_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!(sender is TextBox tb))
				return;
			var fullText = tb.Text.Insert(tb.SelectionStart, e.Text);
			e.Handled = !float.TryParse(fullText, out _);
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
			CurrentProjectVersion = (float) iudCPV.Value;
			ProjectVersionIncrement = (float) iudPVI.Value;
			
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
