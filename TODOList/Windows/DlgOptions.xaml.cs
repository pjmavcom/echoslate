using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Echoslate
{
	public partial class DlgOptions : INotifyPropertyChanged
	{
		public bool AutoSave { get; set; }
		public bool GlobalHotkeys { get; set; }
		public bool WelcomeWindow { get; set; }
		public bool AutoBackup { get; set; }
		private int _backupTime;
		public int BackupTime {
			get => _backupTime;
			set {
				_backupTime = value;
				OnPropertyChanged();
			}
		}

		public bool Result;
		
		public DlgOptions(bool autoSave, bool hotkeys, bool autoBackup, int backupTime, bool welcomeWindow)
		{
			InitializeComponent();
			AutoSave = autoSave;
			GlobalHotkeys = hotkeys;
			AutoBackup = autoBackup;
			BackupTime = backupTime;
			WelcomeWindow = !welcomeWindow;

			cbAS.IsChecked = AutoSave;
			cbWW.IsChecked = WelcomeWindow;
			cbAB.IsChecked = AutoBackup;
			iudBackupTime.Value = BackupTime;

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
			if (cbAS.IsChecked != null)
				AutoSave = (bool) cbAS.IsChecked;
			if (cbWW.IsChecked != null)
				WelcomeWindow = (bool) cbWW.IsChecked;
			if (cbAB.IsChecked != null)
				AutoBackup = (bool) cbAB.IsChecked;

			BackupTime = (int)iudBackupTime.Value;
			Result = true;
			Close();
		}
		private void Cancel_OnClick(object sender, EventArgs e)
		{
			Result = false;
			Close();
		}
		public event PropertyChangedEventHandler? PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
			if (EqualityComparer<T>.Default.Equals(field, value)) return false;
			field = value;
			OnPropertyChanged(propertyName);
			return true;
		}
	}
}
