using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Resources;

namespace Echoslate {
	public partial class DlgEditTabs : INotifyPropertyChanged {
		private readonly List<TabItemHolder> _newTabItemList;
		private ObservableCollection<string> _tabNames;
		public ObservableCollection<string> TabNames {
			get => _tabNames;
			set {
				_tabNames = value;
				OnPropertyChanged();
			}
		}
		private string _newTabName;
		public string NewTabName {
			get => _newTabName;
			set {
				_newTabName = value;
				OnPropertyChanged();
			}
		}
		public bool Result;
		public List<string> ResultList;

		public DlgEditTabs(List<string> tabNames) {
			InitializeComponent();
			DataContext = this;

			TabNames = new ObservableCollection<string>(tabNames);
			TabNames.Remove("All");
			CenterWindowOnMouse();
		}
		private void CenterWindowOnMouse() {
			Window win = Application.Current.MainWindow;

			if (win == null)
				return;
			double centerX = win.Width / 2 + win.Left;
			double centerY = win.Height / 2 + win.Top;
			Left = centerX - Width / 2;
			Top = centerY - Height / 2;
		}
		public ICommand NewTabCommand => new RelayCommand(() => {
			TabNames.Add(NewTabName.CapitalizeFirstLetter());
			NewTabName = string.Empty;
		});
		public ICommand DeleteCommand => new RelayCommand<ListBox>(lb => {
			List<string> tabsToRemove = lb.SelectedItems.Cast<string>().ToList();
			foreach (string s in tabsToRemove) {
				if (TabNames.Contains(s)) {
					TabNames.Remove(s);
				}
			}
		});
		public ICommand MoveUpCommand => new RelayCommand<ListBox>(lb => {
			HashSet<string> selectedSet = lb.SelectedItems.Cast<string>().ToHashSet();
			List<string> listToRemove = TabNames.Where(selectedSet.Contains).ToList();

			int bufferIndex = 0;
			foreach (string s in listToRemove) {
				int index = TabNames.IndexOf(s);
				if (index <= bufferIndex) {
					bufferIndex++;
					continue;
				}
				(TabNames[index], TabNames[index - 1]) = (TabNames[index - 1], TabNames[index]);
			}
			lb.SelectedItems.Clear();
			foreach (string s in listToRemove) {
				lb.SelectedItems.Add(s);
			}
		});
		public ICommand MoveDownCommand => new RelayCommand<ListBox>(lb => {
			HashSet<string> selectedSet = lb.SelectedItems.Cast<string>().ToHashSet();
			List<string> listToRemove = TabNames.Where(selectedSet.Contains).ToList();
			listToRemove.Reverse();

			int bufferIndex = TabNames.Count - 1;
			foreach (string s in listToRemove) {
				int index = TabNames.IndexOf(s);
				if (index >= bufferIndex) {
					bufferIndex--;
					continue;
				}
				(TabNames[index], TabNames[index + 1]) = (TabNames[index + 1], TabNames[index]);
			}
			listToRemove.Reverse();
			lb.SelectedItems.Clear();
			foreach (string s in listToRemove) {
				lb.SelectedItems.Add(s);
			}
		});
		public ICommand CancelCommand => new RelayCommand(() => {
			Result = false;
			Close();
		});
		public ICommand OkCommand => new RelayCommand(() => {
			ResultList = ["All"];
			ResultList.AddRange(TabNames);
			Result = true;
			Close();
		});

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}