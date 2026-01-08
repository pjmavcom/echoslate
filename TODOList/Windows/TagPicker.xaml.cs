using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;

namespace Echoslate {
	public partial class TagPicker : Window, INotifyPropertyChanged {
		private ObservableCollection<string> _allAvailableTags;
		public ObservableCollection<string> AllAvailableTags {
			get => _allAvailableTags;
			set {
				_allAvailableTags = value;
				OnPropertyChanged();
			}
		}
		public IEnumerable AllAvailableTagsView {
			get {
				var items = AllAvailableTags.OrderBy(t => t, StringComparer.OrdinalIgnoreCase);
				return items;
			}
		}

		private List<string> _selectedTags;
		public List<string> SelectedTags {
			get => _selectedTags;
			set {
				_selectedTags = value;
				OnPropertyChanged();
			}
		}

		private string _newTagName;
		public string NewTagName {
			get => _newTagName;
			set {
				_newTagName = value;
				OnPropertyChanged();
			}
		}

		private List<TodoItem> _selectedTodoItems;
		public List<TodoItem> SelectedTodoItems {
			get => _selectedTodoItems;
			set {
				_selectedTodoItems = value;
				OnPropertyChanged();
			}
		}

		public bool Result;


		public TagPicker() {
			InitializeComponent();
			Loaded += OnLoaded;
			DataContext = this;
			CenterWindowOnMouse();
			
			SelectedTags = [];
		}
		private void OnLoaded(object sender, RoutedEventArgs e) {
			foreach (string tag in SelectedTags) {
				lbTags.SelectedItems.Add(tag);
			}
		}
		private void CenterWindowOnMouse() {
			Window? win = Application.Current.MainWindow;
			if (win == null) {
				return;
			}
			
			double centerX = win.Width / 2 + win.Left;
			double centerY = win.Height / 2 + win.Top;
			Left = centerX - Width / 2;
			Top = centerY - Height / 2;
		}
		public void LoadTags(ObservableCollection<string> tags, ObservableCollection<string> th) {
		}
		public void NewTag() {
			if (NewTagName == "") {
				return;
			}
			
			string newTag = NewTagName.ToUpper();
			if (!newTag.StartsWith("#")) {
				newTag = "#" + newTag;
			}

			AllAvailableTags.Add(newTag);
			SelectedTags.Add(newTag);
			lbTags.SelectedItems.Add(newTag);
			NewTagName = "";
		}
		public ICommand NewTagCommand => new RelayCommand(NewTag);
		public ICommand OkCommand => new RelayCommand(() => {
			Result = true;
			SelectedTags.Clear();
			foreach (string tag in lbTags.SelectedItems) {
				SelectedTags.Add(tag);
			}
			Close();
		});
		public ICommand CancelCommand => new RelayCommand(() => {
			Result = false;
			Close();
		});
		
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}