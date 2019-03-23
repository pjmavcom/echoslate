using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TODOList
{
	public partial class DlgEditTabs
	{
		private readonly List<TabItemHolder> _newTabItemList;
		public bool Result;
		public List<string> ResultList;
		
		public DlgEditTabs(List<TabItem> list)
		{
			_newTabItemList = new List<TabItemHolder>();
			foreach (TabItem ti in list)
				_newTabItemList.Add(new TabItemHolder(ti));
			RefreshTabOrder();
			InitializeComponent();
			lbTabs.ItemsSource = _newTabItemList;
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
		private void AddNewTab_OnClick(object sender, EventArgs e)
		{
			if (!(sender is Button b))
				return;
			TabItemHolder tih = b.DataContext as TabItemHolder;
			var index = _newTabItemList.IndexOf(tih);
			TabItem ti = new TabItem();
			string name = "NewTab";
			int tabNumber = 0;
			bool nameExists = false;
			do
			{
				foreach (TabItemHolder t in _newTabItemList)
				{
					if (t.Name == name + tabNumber.ToString())
					{
						tabNumber++;
						nameExists = true;
						break;
					}
					nameExists = false;
				}
			} while (nameExists);
			ti.Name = name + tabNumber;
			ti.Header = name + tabNumber;
			_newTabItemList.Insert(index + 1, new TabItemHolder(ti));

			RefreshTabOrder();
			lbTabs.Items.Refresh();
		}
		private void RemoveTab_OnClick(object sender, EventArgs e)
		{
			if (!(sender is Button b))
				return;
			TabItemHolder tih = b.DataContext as TabItemHolder;
			_newTabItemList.Remove(tih);

			RefreshTabOrder();
			lbTabs.Items.Refresh();
		}
		private void RefreshTabOrder()
		{
			int index = 0;
			foreach (TabItemHolder tih in _newTabItemList)
			{
				tih.MaxIndex = _newTabItemList.Count;
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
				
				if (_newTabItemList.Count == 0)
					return;
				
				var index = _newTabItemList.IndexOf(tih);
				if ((string) b.CommandParameter == "up")
				{
					if (index <= 0)
						return;
					temp = _newTabItemList[index - 1];
					if (tih != null)
					{
						_newTabItemList[index - 1] = tih;
						_newTabItemList[index] = temp;
					}
				}
				else if ((string) b.CommandParameter == "down")
				{
					if (index >= _newTabItemList.Count)
						return;
					temp = _newTabItemList[index + 1];
					if (tih != null)
					{
						_newTabItemList[index + 1] = tih;
						_newTabItemList[index] = temp;
					}
				}
			}
			RefreshTabOrder();
			lbTabs.Items.Refresh();
		}
		private void TextBox_OnPreviewTextInput(object sender, EventArgs e)
		{
			if (!(sender is TextBox tb))
				return;
			if (tb.DataContext is TabItemHolder tih)
				tih.Name = tb.Text;
		}
		private void Ok_OnClick(object sender, EventArgs e)
		{
			ResultList = new List<string>();
			foreach (TabItemHolder tih in _newTabItemList)
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
	}
}
