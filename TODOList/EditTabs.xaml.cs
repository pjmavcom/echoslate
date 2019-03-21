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
	public partial class EditTabs : Window
	{
		public List<TabItemHolder> newTabItemList;
		public bool Result;
		
		public EditTabs(List<TabItem> list)
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
				newTabItemList.Insert(index, new TabItemHolder(ti));

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
	}

	public class TabItemHolder : INotifyPropertyChanged
	{
		private TabItem _ti;
		public bool _canMoveUp;
		public bool _canMoveDown;
		private int _currentIndex;
		private int _maxIndex;

		public string Header
		{
			get => _ti.Header as string;
			set
			{
				_ti.Header = value;
				OnPropertyChanged();
			}
		}
		public string Name
		{
			get => _ti.Name;
			set 
			{
				_ti.Name = value;
				OnPropertyChanged();
			}
		}
		public bool CanMoveUp
		{
			get => _canMoveUp;
			set
			{
				_canMoveUp = value;
				OnPropertyChanged();
			}
		}
		public bool CanMoveDown
		{
			get => _canMoveDown;
			set
			{
				_canMoveDown = value;
				OnPropertyChanged();
			}
		}
		public int CurrentIndex
		{
			get => _currentIndex;
			set
			{
				CanMoveUp = true;
				CanMoveDown = true;
				
				if (value <= 0)
				{
					_currentIndex = 0;
					CanMoveUp = false;
				}
				else if (value >= _maxIndex - 1)
				{
					_currentIndex = _maxIndex - 1;
					CanMoveDown = false;
				}
				else
				{
					_currentIndex = value;
				}
				OnPropertyChanged();
			}
		}
		public int MaxIndex
		{
			get => _maxIndex;
			set => _maxIndex = value;
		}

		public TabItemHolder(TabItem ti)
		{
			_ti = ti;
		}
		
		
		
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
