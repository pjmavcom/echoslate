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
		private readonly List<string> _newTabItemList;
		private ObservableCollection<string> _filterNames;
		public ObservableCollection<string> FilterNames {
			get => _filterNames;
			set {
				_filterNames = value;
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
		private List<string> _resultList;
		public ObservableCollection<string> ResultList;

		public DlgEditTabs(ObservableCollection<string> filterNames) {
			InitializeComponent();
			DataContext = this;

			FilterNames = new ObservableCollection<string>(filterNames);
			FilterNames.Remove("All");
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
			FilterNames.Add(NewTabName.CapitalizeFirstLetter());
			NewTabName = string.Empty;
		});
		public ICommand DeleteCommand => new RelayCommand<ListBox>(lb => {
			List<string> tabsToRemove = lb.SelectedItems.Cast<string>().ToList();
			foreach (string s in tabsToRemove) {
				if (FilterNames.Contains(s)) {
					FilterNames.Remove(s);
				}
			}
		});
		public ICommand MoveUpCommand => new RelayCommand<ListBox>(lb => {
			HashSet<string> selectedSet = lb.SelectedItems.Cast<string>().ToHashSet();
			List<string> listToRemove = FilterNames.Where(selectedSet.Contains).ToList();

			int bufferIndex = 0;
			foreach (string s in listToRemove) {
				int index = FilterNames.IndexOf(s);
				if (index <= bufferIndex) {
					bufferIndex++;
					continue;
				}
				(FilterNames[index], FilterNames[index - 1]) = (FilterNames[index - 1], FilterNames[index]);
			}
			lb.SelectedItems.Clear();
			foreach (string s in listToRemove) {
				lb.SelectedItems.Add(s);
			}
		});
		public ICommand MoveDownCommand => new RelayCommand<ListBox>(lb => {
			HashSet<string> selectedSet = lb.SelectedItems.Cast<string>().ToHashSet();
			List<string> listToRemove = FilterNames.Where(selectedSet.Contains).ToList();
			listToRemove.Reverse();

			int bufferIndex = FilterNames.Count - 1;
			foreach (string s in listToRemove) {
				int index = FilterNames.IndexOf(s);
				if (index >= bufferIndex) {
					bufferIndex--;
					continue;
				}
				(FilterNames[index], FilterNames[index + 1]) = (FilterNames[index + 1], FilterNames[index]);
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
			_resultList = [];
			_resultList.AddRange(FilterNames);
			ResultList = new ObservableCollection<string>(_resultList);
			Result = true;
			Close();
		});

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}