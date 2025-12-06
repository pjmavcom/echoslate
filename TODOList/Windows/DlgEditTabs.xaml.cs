using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TODOList
{
	public partial class DlgEditTabs
	{
		private readonly List<TabItemHolder> _newTabItemList;
		private List<string> _tabNames;
		public bool Result;
		public List<string> ResultList;
		
		public DlgEditTabs(List<TabItem> list)
		{
			_newTabItemList = new List<TabItemHolder>();
			_tabNames = new List<string>();
			
			foreach (TabItem ti in list)
			{
				_newTabItemList.Add(new TabItemHolder(ti));
				_tabNames.Add(ti.Name);
			}

			_tabNames.Remove("All");
			RefreshTabOrder();
			InitializeComponent();
			lbTabs.ItemsSource = _tabNames;
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
		private string UpperFirstLetter(string s)
		{
			string result = "";
			for (int i = 0; i < s.Length; i++)
			{
				if (i == 0)
				{
					result += s[i].ToString().ToUpper();
				}
				else
				{
					result += s[i];
				}
			}

			return result;
		}
		private void AddNewTab()
		{
			string newTabName = tbNewTab.Text.Trim();
			if (newTabName != string.Empty)
			{
				newTabName = UpperFirstLetter(newTabName);
				if (!_tabNames.Contains(newTabName))
				{
					_tabNames.Add(newTabName);
					lbTabs.Items.Refresh();
					tbNewTab.Text = string.Empty;
				}
			}
		}
		private void btnNewTab_OnClick(object sender, EventArgs e)
		{
			AddNewTab();
		}
		private void btnDelete_OnClick(object sender, EventArgs e)
		{
			if (lbTabs.SelectedItems.Count > 0)
			{
				foreach (string s in lbTabs.SelectedItems)
					_tabNames.Remove(s);
				lbTabs.Items.Refresh();
			}
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
		private void btnOk_OnClick(object sender, EventArgs e)
		{
			List<string> resultsWithoutSpaces = new List<string>();
			resultsWithoutSpaces.Add("All");
			
			foreach (string s in _tabNames)
			{
				string newName = "";
				if (s.Contains(' '))
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
		private void btnCancel_OnClick(object sender, EventArgs e)
		{
			Result = false;
			Close();
		}
		private void tbNewTab_OnKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
				AddNewTab();
		}
		private void btnMoveUp_OnClick(object sender, RoutedEventArgs e)
		{
			int currentIndex = 0;
			List<string> selectedItems = new List<string>();
			List<string> selectedItemsOrdered = new List<string>(_tabNames);
			
			foreach (string s in lbTabs.SelectedItems)
				selectedItems.Add(s);
			foreach (string s in _tabNames)
			{
				if (selectedItems.Contains(s))
					continue;
				selectedItemsOrdered.Remove(s);
			}
			
			foreach (string s in selectedItemsOrdered)
			{
				int index = _tabNames.IndexOf(s);
				if (index <= currentIndex++)
					continue;
				_tabNames[index] = _tabNames[index - 1];
				_tabNames[index - 1] = s;
			}
			
			lbTabs.Items.Refresh();
		}
		private void btnMoveDown_OnClick(object sender, RoutedEventArgs e)
		{
			int currentIndex = _tabNames.Count - 1;
			List<string> selectedItems = new List<string>();
			List<string> selectedItemsOrdered = new List<string>(_tabNames);
			
			foreach (string s in lbTabs.SelectedItems)
				selectedItems.Add(s);
			foreach (string s in _tabNames)
			{
				if (selectedItems.Contains(s))
					continue;
				selectedItemsOrdered.Remove(s);
			}

			selectedItemsOrdered.Reverse();
			foreach (string s in selectedItemsOrdered)
			{
				int index = _tabNames.IndexOf(s);
				if (index >= currentIndex--)
					continue;
				_tabNames[index] = _tabNames[index + 1];
				_tabNames[index + 1] = s;
			}
			
			lbTabs.Items.Refresh();
		}
	}
}
