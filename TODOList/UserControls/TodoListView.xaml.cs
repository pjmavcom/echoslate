using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TODOList.ViewModels;

namespace TODOList.UserControls {
	public partial class TodoListView : UserControl {
		
		
		public TodoListView() {
			InitializeComponent();
			Loaded += TodoListView_OnLoaded;
		}
		private void TodoListView_OnLoaded(object sender, RoutedEventArgs e) {
			// This helps it load the first time just a bit faster
			var dummy = new TodoItem(); 
			var tempList = new List<TodoItem> { dummy };
			lbTodos.ItemsSource = tempList;
			Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
			if (DataContext is TodoListViewModel vm) {
				lbTodos.ItemsSource = vm.DisplayedItems;
			}
		}

		private void Sort_OnClick(object sender, RoutedEventArgs e) {
			if (sender is Button b) {
				Log.Print($"Sorting by {b.CommandParameter}");

				return;
			}
			Log.Warn("No button here");
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
		private void TodoListView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
			if (e.NewValue is true && DataContext is TodoListViewModel vm) {
				vm.RefreshAvailableTags();
				vm.RefreshDisplayedItems();
				vm.GetCurrentHashTags();
			}
		}
	}
}