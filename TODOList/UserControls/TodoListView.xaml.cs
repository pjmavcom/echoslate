using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace TODOList.UserControls {
	public partial class TodoListView : UserControl {
		public int SelectedTagIndex { get; set; } = 0;
		public List<TodoItemHolder> CurrentItems { get; private set; } = new();

		public TodoListView() {
			InitializeComponent();
			Loaded += TodoListView_OnLoaded;
		}
		private void TodoListView_OnLoaded(object sender, RoutedEventArgs e) {
			cbHashTags.ItemsSource = new List<TagHolder>();
	 		RefreshList();
			BuildTagFilterButtons();
		}
		public void RefreshList() {
			if (Database.AllTagGroups == null || SelectedTagIndex >= Database.AllTagGroups.Count) {
				return;
			}
			
			CurrentItems = Database.AllTagGroups[SelectedTagIndex];
			lbTodos.ItemsSource = new List<TodoItemHolder>();
			if (CurrentItems != null) {
				lbTodos.ItemsSource = CurrentItems;
			}
		}

		private void BuildTagFilterButtons() {
			var buttonPanel = FindName("spTagFilterPanel") as Panel;
			if (buttonPanel == null) {
				Log.Error("Can not find tag filter button panel");
				return;
			}
			buttonPanel.Children.Clear();

			for (int i = 0; i < Database.AllTagGroups.Count; i++) {
				var tagName = i == 0 ? "All" : GetTagNameFromIndex(i);
				var count = Database.AllTagGroups[i].Count;

				var btn = new Button {
										 Content = $"{tagName} {count}",
										 Tag = i,
										 Margin = new Thickness(2),
										 Padding = new Thickness(6, 2, 6, 2),
										 Background = (i == SelectedTagIndex) ? Brushes.LightGreen : Brushes.LightGray
									 };
				btn.Click += TagButton_OnClick;
				buttonPanel.Children.Add(btn);
			}
		}
		private void TagButton_OnClick(object sender, RoutedEventArgs e) {
			if (sender is Button btn && btn.Tag is int index) {
				SelectedTagIndex = index;
				RefreshList();
				BuildTagFilterButtons();
			}
		}
		private string GetTagNameFromIndex(int index) {
			return index switch {
					   0 => "All",
					   1 => "#Other",
					   2 => "#Feature",
					   3 => "#Bug",
					   _ => $"Tag {index}"
				   };
		}

		private void Sort_OnClick(object sender, RoutedEventArgs e) {
		}
		private void mnuContextMenu_OnClick(object sender, RoutedEventArgs e) {
		}
		private void Severity_OnClick(object sender, RoutedEventArgs e) {
		}
		private void RankAdjust_OnClick(object sender, RoutedEventArgs e) {
		}
		private void TimeTakenTimer_OnClick(object sender, MouseButtonEventArgs e) {
		}
		private void SeverityComboBox_OnSelectionChange(object sender, SelectionChangedEventArgs e) {
		}
		private void SeverityComboBox_OnIsLoaded(object sender, RoutedEventArgs e) {
		}
		private void Add_OnClick(object sender, RoutedEventArgs e) {
			
		}
		private void HashTags_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
		}
	}
}