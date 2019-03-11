using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TODOList
{
	public partial class RemoveTab : Window
	{
		public List<string> newTabItemList;
		public bool Result;
		public int Index;
		
		public RemoveTab(List<TabItem> list)
		{
			newTabItemList = new List<string>();
			foreach (TabItem ti in list)
				newTabItemList.Add(ti.Name);
			InitializeComponent();
			lbTabs.ItemsSource = newTabItemList;
			lbTabs.Items.Refresh();
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
			Index = lbTabs.SelectedIndex;
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
