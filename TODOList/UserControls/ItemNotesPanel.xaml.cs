using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Resources;
using Echoslate.ViewModels;

namespace Echoslate.UserControls {
	public partial class ItemNotesPanel : UserControl {
		public static readonly DependencyProperty SelectedTodoItemProperty =
			DependencyProperty.Register(nameof(SelectedTodoItem), typeof(TodoItemHolder), typeof(ItemNotesPanel), new PropertyMetadata(null));
		
		
		public TodoItemHolder SelectedTodoItem {
			get => (TodoItemHolder)GetValue(SelectedTodoItemProperty);
			set => SetValue(SelectedTodoItemProperty, value);
		}
		public ItemNotesPanel() {
			InitializeComponent();
		}
		private void RefreshAll() {
			if (VisualTreeHelper.GetParent(this) is DependencyObject parent) {
				var todoListView = this.TryFindParent<TodoDisplayView>();
				if (todoListView?.DataContext is TodoListViewModel vm) {
					vm.RefreshAll();
				}
			}
		}
		
		public ICommand NotesPanelCompleteCommand => new RelayCommand(() => RaiseEvent(new RoutedEventArgs(NotesPanelCompleteRequestedEvent)));
		public static readonly RoutedEvent NotesPanelCompleteRequestedEvent =
			EventManager.RegisterRoutedEvent("NotesPanelCompleteRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ItemNotesPanel));
		public event RoutedEventHandler NotesPanelCompleteRequested {
			add => AddHandler(NotesPanelCompleteRequestedEvent, value);
			remove => RemoveHandler(NotesPanelCompleteRequestedEvent, value);
		}
		
		public ICommand NotesPanelAddTagCommand => new RelayCommand(() => RaiseEvent(new RoutedEventArgs(NotesPanelEditTagsRequestedEvent)));
		public static readonly RoutedEvent NotesPanelEditTagsRequestedEvent = 
			EventManager.RegisterRoutedEvent("NotesPanelEditTagsRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ItemNotesPanel));
		public event RoutedEventHandler NotesPanelEditTagsRequested {
			add => AddHandler(NotesPanelEditTagsRequestedEvent, value);
			remove => RemoveHandler(NotesPanelEditTagsRequestedEvent, value);
		}

	}
}