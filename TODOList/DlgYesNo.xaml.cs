using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace TODOList
{
	public partial class DlgYesNo : INotifyPropertyChanged
	{
		private string _windowTitle;
//		private string _windowMessage;
		public string WindowTitle
		{
			get => _windowTitle;
			set
			{
				_windowTitle = value;
				OnPropertyChanged();
			}
			
		}
//		public string WindowMessage
//		{
//			get => _windowMessage;
//			set
//			{
//				_windowMessage = value;
//				OnPropertyChanged();
//			}
//		}
		public bool Result;
		public DlgYesNo(string windowTitle, string windowMessage)
		{
			InitializeComponent();
			CenterWindowOnMouse();
			this.Title = windowTitle;
			WindowMessage.Text = windowMessage;
		}
		public DlgYesNo(string windowMessage)
		{
			InitializeComponent();
			CenterWindowOnMouse();
			this.Title = "";
			WindowMessage.Text = windowMessage;
//			btnOK.Visibility = Visibility.Hidden;
			btnCancel.Visibility = Visibility.Collapsed;
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
			Result = true;
			Close();
		}
		private void Cancel_OnClick(object sender, EventArgs e)
		{
			Result = false;
			Close();
		}
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
