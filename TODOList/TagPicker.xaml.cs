using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace TODOList
{
	public partial class TagPicker : Window
	{
		private ObservableCollection<string> _tags;
		private ObservableCollection<string> _originalTags;
		private ObservableCollection<string> _originalTagHolders;
		private ObservableCollection<string> _previousTags;
		public ObservableCollection<string> NewTags;
		public bool Result;
		public bool Multi;
		
		
		public TagPicker(bool multi = false)
		{
			InitializeComponent();
			NewTags = new ObservableCollection<string>();
			_previousTags = new ObservableCollection<string>();
			CenterWindowOnMouse();
			cbMulti.IsEnabled = multi ? true : false;
			cbMulti.Visibility = multi ? Visibility.Visible : Visibility.Hidden;
			cbMulti.IsChecked = multi ? true : false;
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
		public void LoadTags(ObservableCollection<string> tags, ObservableCollection<string> th)
		{
			if (tags == null || th == null)
				return;
			_originalTags = tags;
			_originalTagHolders = th;
			_tags = tags;
			lbTags.ItemsSource = _tags;
			lbTags.Items.Refresh();

			if (!Multi)
			{
				foreach (string s in th)
				{
					int index = _tags.IndexOf(s);
					_previousTags.Add(s);
					
					if (index >= 0)
						lbTags.SelectedItems.Add(lbTags.Items.GetItemAt(index));
				}
			}
		}
		private void btnCancel_OnClick(object sender, RoutedEventArgs e)
		{
			Result = false;
			Close();
		}
		private void btnAdd_OnClick(object sender, RoutedEventArgs e)
		{
			foreach (object tag in lbTags.SelectedItems)
				NewTags.Add(tag.ToString());
			Result = true;
			if (cbMulti.IsChecked != null)
				Multi = (bool)cbMulti.IsChecked;
			
			Close();
		}
		private void btnNewTag_OnClick(object sender, RoutedEventArgs e)
		{
			if (tbNewTag.Text == "")
				return;
			string newTag = tbNewTag.Text.ToUpper();
			if (!newTag.StartsWith("#"))
				newTag = "#" + newTag;

			_tags.Add(newTag);
			_previousTags.Add(newTag);
			
			// TODO Figure out how to sort the observableCollections
			// _tags.Sort();
			var sorted = _tags.OrderBy(x => x).ToList();
			_tags.Clear();
			foreach (string s in sorted) {
				_tags.Add(s);
			}
			
			lbTags.Items.Refresh();
			
			foreach (string s in _previousTags)
			{
				int index = _tags.IndexOf(s);
				
				if (index >= 0)
					lbTags.SelectedItems.Add(lbTags.Items.GetItemAt(index));
			}
		}
		private void cbMulti_OnChecked(object sender, RoutedEventArgs e)
		{
			if (cbMulti.IsChecked == true)
				LoadTags(_originalTags, _originalTagHolders);
			else
				lbTags.SelectedIndex = -1;
			lbTags.Items.Refresh();
		}
	}
}