using System.Collections.Generic;
using System.Windows;

namespace TODOList
{
	public partial class TagPicker : Window
	{
		private List<string> _tags;
		private List<string> _originalTags;
		private List<string> _originalTagHolders;
		private List<string> _previousTags;
		public List<string> NewTags;
		public bool Result;
		public bool Multi;
		
		
		public TagPicker(bool multi = false)
		{
			InitializeComponent();
			NewTags = new List<string>();
			_previousTags = new List<string>();
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
		public void LoadTags(List<string> tags, List<string> th)
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
			
			_tags.Sort();
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