using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TODOList.UserControls {
	public partial class IncompleteItemsNotesPanel : UserControl {
		public IncompleteItemsNotesPanel() {
			InitializeComponent();
		}
		private void TodoComplete_OnClick(object sender, RoutedEventArgs e) {
			MainWindow.GetActiveWindow().TodoComplete_OnClick(sender, e);
		}
		private void AddTag_OnClick(object sender, RoutedEventArgs e) {
			MainWindow.GetActiveWindow().AddTag_OnClick(sender, e);
		}
		private void TodoTitle_OnLostFocus(object sender, RoutedEventArgs e) {
			MainWindow.GetActiveWindow().TodoTitle_OnLostFocus(sender, e);
		}
		private void TodoTitle_OnGotFocus(object sender, RoutedEventArgs e) {
			MainWindow.GetActiveWindow().TodoTitle_OnGotFocus(sender, e);
		}
		private void Notes_OnLostFocus(object sender, RoutedEventArgs e) {
			MainWindow.GetActiveWindow().Notes_OnLostFocus(sender, e);
		}
		private void Notes_OnGotFocus(object sender, RoutedEventArgs e) {
			MainWindow.GetActiveWindow().Notes_OnGotFocus(sender, e);
		}
		private void Problem_OnLostFocus(object sender, RoutedEventArgs e) {
			MainWindow.GetActiveWindow().Problem_OnLostFocus(sender, e);
		}
		private void Problem_OnGotFocus(object sender, RoutedEventArgs e) {
			MainWindow.GetActiveWindow().Problem_OnGotFocus(sender, e);
		}
		private void Solution_OnLostFocus(object sender, RoutedEventArgs e) {
			MainWindow.GetActiveWindow().Solution_OnLostFocus(sender, e);
		}
		private void Solution_OnGotFocus(object sender, RoutedEventArgs e) {
			MainWindow.GetActiveWindow().Solution_OnGotFocus(sender, e);
		}
	}
}