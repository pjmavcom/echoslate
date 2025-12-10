using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

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
		private ICollectionView _allAvailableTagsView;
		public ICollectionView AllAvailableTagsView {
			get {
				if (_allAvailableTagsView == null) {
					_allAvailableTagsView = CollectionViewSource.GetDefaultView(AllAvailableTags);
					_allAvailableTagsView.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
				}
				return _allAvailableTagsView;
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


		private ObservableCollection<string> _tags;
		private ObservableCollection<string> _originalTags;
		private ObservableCollection<string> _originalTagHolders;
		private ObservableCollection<string> _previousTags;
		public ObservableCollection<string> NewTags;
		public bool Result;
		public bool Multi;


		public TagPicker(bool multi = false) {
			InitializeComponent();
			DataContext = this;
			SelectedTags = [];

			Loaded += OnLoaded;
			
			NewTags = new ObservableCollection<string>();
			_previousTags = new ObservableCollection<string>();
			CenterWindowOnMouse();
			cbMulti.IsEnabled = multi ? true : false;
			cbMulti.Visibility = multi ? Visibility.Visible : Visibility.Hidden;
			cbMulti.IsChecked = multi ? true : false;
		}
		private void OnLoaded(object sender, RoutedEventArgs e) {
			// foreach (string tag in SelectedTags) {
				// lbTags.SelectedItems.Add(tag);
			// }
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
		public void LoadTags(ObservableCollection<string> tags, ObservableCollection<string> th) {
			// if (tags == null || th == null)
				// return;
			// _originalTags = tags;
			// _originalTagHolders = th;
			// _tags = tags;
			// lbTags.ItemsSource = _tags;
			// lbTags.Items.Refresh();

			// if (!Multi) {
				// foreach (string s in th) {
					// int index = _tags.IndexOf(s);
					// _previousTags.Add(s);

					// if (index >= 0)
						// lbTags.SelectedItems.Add(lbTags.Items.GetItemAt(index));
				// }
			// }
		}
		private void btnAdd_OnClick(object sender, RoutedEventArgs e) {
			// foreach (object tag in lbTags.SelectedItems)
				// NewTags.Add(tag.ToString());
			// Result = true;
			// if (cbMulti.IsChecked != null)
				// Multi = (bool)cbMulti.IsChecked;

			// Close();
		}
		private void btnNewTag_OnClick(object sender, RoutedEventArgs e) {
			// if (tbNewTag.Text == "")
				// return;
			// string newTag = tbNewTag.Text.ToUpper();
			// if (!newTag.StartsWith("#"))
				// newTag = "#" + newTag;

			// _tags.Add(newTag);
			// _previousTags.Add(newTag);

			// _tags.Sort();
			// var sorted = _tags.OrderBy(x => x).ToList();
			// _tags.Clear();
			// foreach (string s in sorted) {
				// _tags.Add(s);
			// }

			// lbTags.Items.Refresh();

			// foreach (string s in _previousTags) {
				// int index = _tags.IndexOf(s);

				// if (index >= 0)
					// lbTags.SelectedItems.Add(lbTags.Items.GetItemAt(index));
			// }
		}
		private void cbMulti_OnChecked(object sender, RoutedEventArgs e) {
			// if (cbMulti.IsChecked == true)
				// LoadTags(_originalTags, _originalTagHolders);
			// else
				// lbTags.SelectedIndex = -1;
			// lbTags.Items.Refresh();
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