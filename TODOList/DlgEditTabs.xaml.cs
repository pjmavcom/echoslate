using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace TODOList
{
	public partial class DlgEditTabs : Window
	{
		public List<TabItemHolder> newTabItemList;
		public List<string> ResultList;
		public bool Result;
		
		public DlgEditTabs(List<TabItem> list)
		{
			newTabItemList = new List<TabItemHolder>();
			foreach (TabItem ti in list)
				newTabItemList.Add(new TabItemHolder(ti));
			RefreshTabOrder();
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
			ResultList = new List<string>();
			foreach (TabItemHolder tih in newTabItemList)
			{
				ResultList.Add(tih.Name);
			}

			List<string> resultsWithoutSpaces = new List<string>();
			foreach (string s in ResultList)
			{
				string newName = "";
				if(s.Contains(' '))
				{
					foreach (char c in s)
						if (c != ' ')
							newName += c;
				}
				else
					newName = s;
				resultsWithoutSpaces.Add(newName);
			}
			ResultList = resultsWithoutSpaces;
			Result = true;
			Close();
		}
		private void Cancel_OnClick(object sender, EventArgs e)
		{
			Result = false;
			Close();
		}
		private void AddNewTab_OnClick(object sender, EventArgs e)
		{
			if (sender is Button b)
			{
				TabItemHolder tih = b.DataContext as TabItemHolder;
				var index = newTabItemList.IndexOf(tih);
				TabItem ti = new TabItem();
				string name = "NewTab";
				int tabNumber = 0;
				bool nameExists = false;
				do
				{
					foreach (TabItemHolder t in newTabItemList)
					{
						if (t.Name == name + tabNumber.ToString())
						{
							tabNumber++;
							nameExists = true;
							break;
						}
						else 
							nameExists = false;
					}
				} while (nameExists);
				ti.Name = name + tabNumber;
				ti.Header = name + tabNumber;
				newTabItemList.Insert(index + 1, new TabItemHolder(ti));

				RefreshTabOrder();
				lbTabs.Items.Refresh();
			}
		}
		private void RemoveTab_OnClick(object sender, EventArgs e)
		{
			if (sender is Button b)
			{
				TabItemHolder tih = b.DataContext as TabItemHolder;
				newTabItemList.Remove(tih);

				RefreshTabOrder();
				lbTabs.Items.Refresh();
			}
		}
		private void RefreshTabOrder()
		{
			int index = 0;
			foreach (TabItemHolder tih in newTabItemList)
			{
				tih.MaxIndex = newTabItemList.Count;
				tih.CurrentIndex = index;
				index++;
			}
		}
		private void Move_OnClick(object sender, EventArgs e)
		{
			if (sender is Button b)
			{
				TabItemHolder tih = b.DataContext as TabItemHolder;
				TabItemHolder temp;
				
				if (newTabItemList.Count == 0)
					return;
				
				var index = newTabItemList.IndexOf(tih);
				if ((string) b.CommandParameter == "up")
				{
					if (index <= 0)
						return;
					temp = newTabItemList[index - 1];
					if (tih != null)
					{
						newTabItemList[index - 1] = tih;
						newTabItemList[index] = temp;
					}
				}
				else if ((string) b.CommandParameter == "down")
				{
					if (index >= newTabItemList.Count)
						return;
					temp = newTabItemList[index + 1];
					if (tih != null)
					{
						newTabItemList[index + 1] = tih;
						newTabItemList[index] = temp;
					}
				}
			}
			RefreshTabOrder();
			lbTabs.Items.Refresh();
		}
		private void TextBox_OnPreviewTextInput(object sender, EventArgs e)
		{
			if (sender is TextBox tb)
			{
				TabItemHolder tih = tb.DataContext as TabItemHolder;
				
				tih.Name = tb.Text;
			}
		}
	}

	
}
