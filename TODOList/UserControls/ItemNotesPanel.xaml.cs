using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Echoslate.UserControls {
	public partial class ItemNotesPanel : UserControl, INotifyPropertyChanged {
		
		public static readonly DependencyProperty SelectedTodoItemProperty =
			DependencyProperty.Register(nameof(SelectedTodoItem), typeof(TodoItemHolder), typeof(ItemNotesPanel), new PropertyMetadata(null));
		
		public TodoItemHolder SelectedTodoItem {
			get => (TodoItemHolder)GetValue(SelectedTodoItemProperty);
			set => SetValue(SelectedTodoItemProperty, value);
		}

		
		
		public ItemNotesPanel() {
			InitializeComponent();
		}
		private void TodoComplete_OnClick(object sender, RoutedEventArgs e) {
		}
		private void AddTag_OnClick(object sender, RoutedEventArgs e) {
		}
		private void TodoTitle_OnLostFocus(object sender, RoutedEventArgs e) {
		}
		private void TodoTitle_OnGotFocus(object sender, RoutedEventArgs e) {
		}
		private void Notes_OnLostFocus(object sender, RoutedEventArgs e) {
		}
		private void Notes_OnGotFocus(object sender, RoutedEventArgs e) {
		}
		private void Problem_OnLostFocus(object sender, RoutedEventArgs e) {
		}
		private void Problem_OnGotFocus(object sender, RoutedEventArgs e) {
		}
		private void Solution_OnLostFocus(object sender, RoutedEventArgs e) {
		}
		private void Solution_OnGotFocus(object sender, RoutedEventArgs e) {
		}

		public void AddTag() {
			if (SelectedTodoItem == null) {
				return;
			}
			
		}


		public ICommand AddTagCommand => new RelayCommand(() => RaiseEvent(new RoutedEventArgs(EditTagsRequestedEvent)));
		public static readonly RoutedEvent EditTagsRequestedEvent = 
			EventManager.RegisterRoutedEvent("EditTagsRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ItemNotesPanel));
		public event RoutedEventHandler EditTagsRequested {
			add { AddHandler(EditTagsRequestedEvent, value); }
			remove { RemoveHandler(EditTagsRequestedEvent, value); }
		}
		
		
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}